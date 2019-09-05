// Copyright 2019 Carnegie Mellon University. All Rights Reserved.
// Released under a 3 Clause BSD-style license. See LICENSE.md in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.Extensions.Logging;
using TopoMojo.Abstractions;
using TopoMojo.Core.Abstractions;
using TopoMojo.Core.Models.Extensions;
using TopoMojo.Data.Abstractions;
using TopoMojo.Data.Entities;

namespace TopoMojo.Core
{
    public class EngineService : EntityManager<Gamespace>
    {
        public EngineService(
            IGamespaceRepository repo,
            ITopologyRepository topos,
            ILoggerFactory mill,
            CoreOptions options,
            IProfileResolver profileResolver,
            IPodManager podManager
        ) : base (mill, options, profileResolver)
        {
            _pod = podManager;
            _repo = repo;
            _topos = topos;
        }

        private readonly IPodManager _pod;
        private readonly IGamespaceRepository _repo;
        private readonly ITopologyRepository _topos;

        public async Task<Models.GameState> Launch(int topoId, string IsolationId)
        {
            Gamespace game = await _repo.FindByGlobalId(IsolationId);

            if (game == null)
            {
                Topology topology = await _topos.Load(topoId);
                if (topology == null)
                    throw new InvalidOperationException();

                game = new Gamespace
                {
                    GlobalId = IsolationId,
                    Name = topology.Name,
                    TopologyId = topoId,
                    ShareCode = Guid.NewGuid().ToString("N")
                };

                await _repo.Add(game);
            }

            return await Deploy(await _repo.Load(game.Id));

        }

        private async Task<Models.GameState> Deploy(Gamespace gamespace)
        {
            List<Task<TopoMojo.Models.Virtual.Vm>> tasks = new List<Task<TopoMojo.Models.Virtual.Vm>>();
            foreach (Template template in gamespace.Topology.Templates)
            {
                string iso = String.IsNullOrEmpty(template.Iso)
                    ? $"{_pod.Options.IsoStore}/{gamespace.GlobalId}.iso"
                    : template.Iso;

                TemplateUtility tu = new TemplateUtility(template.Detail ?? template.Parent.Detail);
                tu.Name = template.Name;
                tu.Networks = template.Networks;
                tu.Iso = iso;
                tu.IsolationTag = gamespace.GlobalId;
                tu.Id = template.Id.ToString();
                tasks.Add(_pod.Deploy(tu.AsTemplate()));
            }
            Task.WaitAll(tasks.ToArray());

            return await LoadState(gamespace, gamespace.TopologyId);
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

            List<Task<TopoMojo.Models.Virtual.Vm>> tasks = new List<Task<TopoMojo.Models.Virtual.Vm>>();
            foreach (TopoMojo.Models.Virtual.Vm vm in await _pod.Find(gamespace.GlobalId))
                tasks.Add(_pod.Delete(vm.Id));
            Task.WaitAll(tasks.ToArray());
            //TODO: _pod.DeleteMatches(gamespace.GlobalId);

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
    }
}
