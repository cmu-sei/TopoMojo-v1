// Copyright 2019 Carnegie Mellon University. All Rights Reserved.
// Released under a 3 Clause BSD-style license. See LICENSE.md in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.Extensions.Logging;
using TopoMojo.Abstractions;
using TopoMojo.Data.Abstractions;
using TopoMojo.Extensions;
using TopoMojo.Models;

namespace TopoMojo.Core
{
    public class EngineService : EntityService<Data.Gamespace>
    {
        public EngineService(
            IGamespaceStore gamespaceStore,
            IWorkspaceStore workspaceStore,
            ITemplateStore templateStore,
            IHypervisorService podService,
            ILoggerFactory mill,
            IMapper mapper,
            CoreOptions options,
            IIdentityResolver identityResolver
        ) : base (mill, mapper, options, identityResolver)
        {
            _pod = podService;
            _gamespaceStore = gamespaceStore;
            _workspaceStore = workspaceStore;
            _templateStore = templateStore;
        }

        private readonly IHypervisorService _pod;
        private readonly IGamespaceStore _gamespaceStore;
        private readonly IWorkspaceStore _workspaceStore;
        private readonly ITemplateStore _templateStore;

        public async Task<GameState> Launch(int workspaceId, string isolationId)
        {
            return await Launch(new GamespaceSpec{ WorkspaceId = workspaceId }, isolationId);
        }

        public async Task<GameState> Launch(GamespaceSpec spec, string isolationId)
        {
            var game = await _gamespaceStore.FindByGlobalId(isolationId);

            if (game == null)
            {
                Data.Topology topology = await _workspaceStore.Load(spec.WorkspaceId);
                if (topology == null)
                    throw new InvalidOperationException();

                game = new Data.Gamespace
                {
                    GlobalId = isolationId,
                    Name = topology.Name,
                    TopologyId = spec.WorkspaceId,
                    ShareCode = Guid.NewGuid().ToString("N")
                };

                await _gamespaceStore.Add(game);
            }

            return await Deploy(await _gamespaceStore.Load(game.Id), spec);

        }

        private async Task<GameState> Deploy(Data.Gamespace gamespace, GamespaceSpec spec)
        {
            var deployTasks = new List<Task<Vm>>();

            try
            {
                var templates = Mapper.Map<List<ConvergedTemplate>>(gamespace.Topology.Templates);

                ApplyIso(templates, spec);

                ExpandTemplates(templates, spec);

                await AddNetworkServer(templates, spec);

                foreach (var template in templates)
                {

                    var virtualTemplate = template.ToVirtualTemplate(gamespace.GlobalId);

                    if (spec.HostAffinity)
                        virtualTemplate.AutoStart = false;

                    deployTasks.Add(_pod.Deploy(virtualTemplate));
                }

                Task.WaitAll(deployTasks.ToArray());

                if (spec.HostAffinity)
                {
                    await _pod.SetAffinity(gamespace.GlobalId, deployTasks.Select(t => t.Result).ToArray(), true);
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deploying Engine mojo");
                throw ex;
            }

            return await LoadState(gamespace, gamespace.TopologyId);
        }

        private async Task AddNetworkServer(ICollection<ConvergedTemplate> templates, GamespaceSpec spec)
        {
            if (spec.Network == null)
                return;

            var settings = new KeyValuePair<string,string>[]
            {
                new KeyValuePair<string,string>("newip", spec.Network.IpAddress),
                new KeyValuePair<string,string>("hosts", String.Join(";", spec.Network.HostFileEntries)),
                new KeyValuePair<string,string>("dhcp", String.Join(";", spec.Network.Dnsmasq))
            };

            var netServerTemplateEntity = await _templateStore.Load(_options.DefaultNetServerTemplateId);

            if (netServerTemplateEntity != null)
            {
                var netServerTemplate = Mapper.Map<ConvergedTemplate>(netServerTemplateEntity);

                netServerTemplate.AddSettings(settings);

                templates.Add(netServerTemplate);
            }
        }

        private void ExpandTemplates(ICollection<ConvergedTemplate> templates, GamespaceSpec spec)
        {
            var ts = String.IsNullOrEmpty(spec.Templates)
                ? templates.ToList()
                : JsonSerializer.Deserialize<List<ConvergedTemplate>>(spec.Templates);

            templates.Clear();

            foreach (var t in ts)
            {
                templates.Add(t);

                var vmspec = spec.Vms?.SingleOrDefault(v => v.Name == t.Name);

                if (vmspec != null && vmspec.Replicas > 1)
                {
                    for (int i = 1; i < vmspec.Replicas; i++)
                    {
                        var tt = t.Clone<ConvergedTemplate>();

                        tt.Name += $"_{i}";

                        templates.Add(tt);
                    }

                    t.Name += "_0";
                }
            }
        }

        private void ApplyIso(ICollection<ConvergedTemplate> templates, GamespaceSpec spec)
        {
            if (string.IsNullOrEmpty(spec.Iso))
                return;

            string[] targets = spec.IsoTarget.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            foreach (var template in templates.Where(t => string.IsNullOrEmpty(t.Iso)))
            {
                if (targets.Length > 0 && !targets.Contains(template.Name))
                    continue;

                var vmspec = spec.Vms.FirstOrDefault(v => v.Name == template.Name);

                if (vmspec != null && vmspec.SkipIso)
                    continue;

                template.Iso =  $"{_pod.Options.IsoStore}/{_options.GameEngineIsoFolder}/{spec.Iso}";
            }
        }

        private async Task<GameState> LoadState(Data.Gamespace gamespace, int topoId)
        {
            GameState state = null;

            if (gamespace == null)
            {
                Data.Topology topo = await _workspaceStore.Load(topoId);
                if (topo == null || !topo.IsPublished)
                    throw new InvalidOperationException();

                state = new GameState();
                state.Name = gamespace?.Name ?? topo.Name;
                state.TopologyDocument = topo.Document;
                state.Vms = topo.Templates
                    .Where(t => !t.IsHidden)
                    .Select(t => new VmState { Name = t.Name, TemplateId = t.Id})
                    .ToArray();
            }
            else
            {
                state = Mapper.Map<GameState>(gamespace);
                state.Vms = gamespace.Topology.Templates
                    .Where(t => !t.IsHidden)
                    .Select(t => new VmState { Name = t.Name, TemplateId = t.Id})
                    .ToArray();
                state.MergeVms(await _pod.Find(gamespace.GlobalId));
            }

            return state;
        }

        public async Task<GameState> Destroy(string globalId)
        {
            var gamespace = await _gamespaceStore.FindByGlobalId(globalId);
            if (gamespace == null)
                throw new InvalidOperationException();

            await _pod.DeleteAll(gamespace.GlobalId);

            await _gamespaceStore.Remove(gamespace);
            return Mapper.Map<GameState>(gamespace);
        }

        public async Task<ConsoleSummary> Ticket(string vmId)
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
            Data.Topology topo = await _workspaceStore.Load(topoId);
            return (topo != null)
                ? JsonSerializer.Serialize(Mapper.Map<List<ConvergedTemplate>>(topo.Templates), null)
                : "";
        }

        public async Task<bool> ChangeVm(VmAction vmAction)
        {
            bool result = false;

            if (!Guid.TryParse(vmAction.Id, out Guid guid))
            {
                // lookup id from name
                vmAction.Id = (await _pod.Find(vmAction.Id)).FirstOrDefault()?.Id ?? Guid.Empty.ToString();
            }

            switch (vmAction.Type)
            {
                case "stop":
                    try
                    {
                        await _pod.StopAll(vmAction.Id);
                    } catch {}
                    break;

                case "start":
                case "restart":

                    try
                    {
                        if (vmAction.Type == "restart")
                            await _pod.StopAll(vmAction.Id);

                    } catch {}

                    try
                    {
                        await _pod.StartAll(vmAction.Id);
                    } catch {}
                    result = true;
                    break;

                case "iso":
                    // look up iso path
                    var vm = (await _pod.Find(vmAction.Id)).FirstOrDefault();
                    var gamespace = await _gamespaceStore.FindByGlobalId(vm.Name.Tag());
                    var allowedIsos = await _pod.GetVmIsoOptions(gamespace.Topology.GlobalId);
                    string path = allowedIsos.Iso.Where(x => x.Contains(vmAction.Message)).FirstOrDefault();
                    _logger.LogDebug($"{vm.Name}, {vmAction.Message}, {gamespace.Topology.Name}, {String.Join(" ", allowedIsos.Iso)}");
                    await _pod.ChangeConfiguration(vmAction.Id, new KeyValuePair<string,string>("iso", path));
                    result = true;
                    break;

            }

            return result;
        }

    }
}
