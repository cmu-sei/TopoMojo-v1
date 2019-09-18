// Copyright 2019 Carnegie Mellon University. All Rights Reserved.
// Released under a 3 Clause BSD-style license. See LICENSE.md in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using TopoMojo.Abstractions;
using TopoMojo.Core.Abstractions;
using TopoMojo.Core.Models.Extensions;
using TopoMojo.Data.Abstractions;
using TopoMojo.Data.Entities;
using TopoMojo.Extensions;
using TopoMojo.Models;

namespace TopoMojo.Core
{
    public class EngineService : EntityManager<Gamespace>
    {
        public EngineService(
            IGamespaceRepository repo,
            ITopologyRepository topos,
            ITemplateRepository templateRepository,
            ILoggerFactory mill,
            CoreOptions options,
            IProfileResolver profileResolver,
            IPodManager podManager
        ) : base (mill, options, profileResolver)
        {
            _pod = podManager;
            _repo = repo;
            _topos = topos;
            _templates = templateRepository;
        }

        private readonly IPodManager _pod;
        private readonly IGamespaceRepository _repo;
        private readonly ITopologyRepository _topos;
        private readonly ITemplateRepository _templates;
        public async Task<Models.GameState> Launch(Models.WorkspaceSpec spec, string IsolationId)
        {
            Gamespace game = await _repo.FindByGlobalId(IsolationId);

            if (game == null)
            {
                Topology topology = await _topos.Load(spec.Id);
                if (topology == null)
                    throw new InvalidOperationException();

                game = new Gamespace
                {
                    GlobalId = IsolationId,
                    Name = topology.Name,
                    TopologyId = spec.Id,
                    ShareCode = Guid.NewGuid().ToString("N")
                };

                await _repo.Add(game);
            }

            return await Deploy(await _repo.Load(game.Id), spec);

        }

        private async Task<Models.GameState> Deploy(Gamespace gamespace, Models.WorkspaceSpec spec)
        {
            List<Task<TopoMojo.Models.Virtual.Vm>> tasks = new List<Task<TopoMojo.Models.Virtual.Vm>>();

            try
            {
                ExpandTemplates(gamespace.Topology.Templates, spec);

                await AddNetworkServer(gamespace.Topology.Templates, spec);

                foreach (Template template in gamespace.Topology.Templates)
                {
                    string iso = template.Iso;
                    if (!String.IsNullOrEmpty(spec.Iso) && String.IsNullOrEmpty(template.Iso))
                        iso =  $"{_pod.Options.IsoStore}/{_options.GameEngineIsoFolder}/{spec.Iso}";

                    TemplateUtility tu = new TemplateUtility(template.Detail ?? template.Parent.Detail);
                    tu.Name = template.Name;
                    tu.Networks = template.Networks;
                    tu.Iso = iso;
                    tu.IsolationTag = gamespace.GlobalId;
                    tu.Id = template.Id.ToString();
                    tasks.Add(_pod.Deploy(tu.AsTemplate(), !spec.HostAffinity));
                }

                Task.WaitAll(tasks.ToArray());

                if (spec.HostAffinity)
                {
                    await _pod.SetAffinity(gamespace.GlobalId, tasks.Select(t => t.Result).ToArray(), true);
                }
            } catch (Exception ex)
            {
                _logger.LogError(ex, "Error deploying Engine mojo");
                throw ex;
            }
            return await LoadState(gamespace, gamespace.TopologyId);
        }

        private async Task AddNetworkServer(ICollection<Data.Entities.Template> templates, Models.WorkspaceSpec spec)
        {
            if (spec.Network == null)
                return;

            List<TopoMojo.Models.KeyValuePair> settings = new List<TopoMojo.Models.KeyValuePair>();
            settings.Add(new TopoMojo.Models.KeyValuePair { Key = "newip", Value = spec.Network.NewIp });
            settings.Add(new TopoMojo.Models.KeyValuePair { Key = "hosts", Value = String.Join(";", spec.Network.Hosts) });
            settings.Add(new TopoMojo.Models.KeyValuePair { Key = "dhcp", Value = String.Join(";", spec.Network.Dnsmasq) });
            var t = await _templates.Load(_options.DefaultNetServerTemplateId);
            var nettemplate = t.Map<Template>();
            TemplateUtility tu = new TemplateUtility(nettemplate.Detail);
            tu.GuestSettings = settings.ToArray();
            nettemplate.Detail = tu.ToString();
            templates.Add(nettemplate);
        }

        private void ExpandTemplates(ICollection<Data.Entities.Template> templates, Models.WorkspaceSpec spec)
        {
            List<Data.Entities.Template> ts = String.IsNullOrEmpty(spec.Templates)
                ? templates.ToList()
                : JsonConvert.DeserializeObject<List<Data.Entities.Template>>(spec.Templates);

            templates.Clear();
            foreach (var t in ts)
            {
                templates.Add(t);
                var vmspec = spec.Vms?.SingleOrDefault(v => v.Name == t.Name);
                if (vmspec != null && vmspec.Replicas > 1)
                {
                    for (int i = 1; i < vmspec.Replicas; i++)
                    {
                        var tt = t.Map<Data.Entities.Template>();
                        tt.Name += $"_{i}";
                        templates.Add(tt);
                    }
                }
            }
        }

        private async Task<Models.GameState> LoadState(Gamespace gamespace, int topoId)
        {
            Models.GameState state = null;

            if (gamespace == null)
            {
                Data.Entities.Topology topo = await _topos.Load(topoId);
                if (topo == null || !topo.IsPublished)
                    throw new InvalidOperationException();

                state = new Models.GameState();
                state.Name = gamespace?.Name ?? topo.Name;
                state.TopologyDocument = topo.Document;
                state.Vms = topo.Templates
                    .Where(t => !t.IsHidden)
                    .Select(t => new Models.VmState { Name = t.Name, TemplateId = t.Id})
                    .ToArray();
            }
            else
            {
                state = Mapper.Map<Models.GameState>(gamespace);
                state.Vms = gamespace.Topology.Templates
                    .Where(t => !t.IsHidden)
                    .Select(t => new Models.VmState { Name = t.Name, TemplateId = t.Id})
                    .ToArray();
                state.MergeVms(await _pod.Find(gamespace.GlobalId));
            }

            return state;
        }

        public async Task<Models.GameState> Destroy(string globalId)
        {
            Gamespace gamespace = await _repo.FindByGlobalId(globalId);
            if (gamespace == null)
                throw new InvalidOperationException();

            // List<Task<TopoMojo.Models.Virtual.Vm>> tasks = new List<Task<TopoMojo.Models.Virtual.Vm>>();
            // foreach (TopoMojo.Models.Virtual.Vm vm in await _pod.Find(gamespace.GlobalId))
            //     tasks.Add(_pod.Delete(vm.Id));
            // Task.WaitAll(tasks.ToArray());
            await _pod.DeleteMatches(gamespace.GlobalId);

            await _repo.Remove(gamespace);
            return Mapper.Map<Models.GameState>(gamespace);
        }

        public async Task<TopoMojo.Models.Virtual.DisplayInfo> Ticket(string vmId)
        {
            var info = await _pod.Display(vmId);
            if (info.Url.HasValue())
            {
                var src = new Uri(info.Url);
                info.Url = info.Url.Replace(src.Host, _options.ConsoleHost) + $"?vmhost={src.Host}";
            }
            return info;
        }

        public async Task<string> GetTemplates(int topoId)
        {
            Data.Entities.Topology topo = await _topos.Load(topoId);
            return (topo != null)
                ? JsonConvert.SerializeObject(topo.Templates, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore})
                : "";
        }
    }
}
