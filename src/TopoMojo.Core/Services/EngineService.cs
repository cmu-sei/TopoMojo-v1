// Copyright 2020 Carnegie Mellon University. All Rights Reserved.
// Released under a 3 Clause BSD-style license. See LICENSE.md in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TopoMojo.Abstractions;
using TopoMojo.Data.Abstractions;
using TopoMojo.Data.Extensions;
using TopoMojo.Extensions;
using TopoMojo.Models;

namespace TopoMojo.Services
{
    public class EngineService : _Service
    {
        public EngineService(
            IGamespaceStore gamespaceStore,
            IWorkspaceStore workspaceStore,
            ITemplateStore templateStore,
            IHypervisorService podService,
            ILogger<EngineService> logger,
            IMapper mapper,
            CoreOptions options,
            IIdentityResolver identityResolver
        ) : base (logger, mapper, options, identityResolver)
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
            return await Launch(
                new GamespaceSpec{ WorkspaceId = workspaceId }
            );
        }

        public async Task<GameState> Launch(GamespaceSpec spec)
        {
            _logger.LogDebug(Newtonsoft.Json.JsonConvert.SerializeObject(spec));

            var game = await _gamespaceStore.Load(spec.IsolationId);

            if (game == null)
            {
                var workspace = await _workspaceStore.Load(spec.WorkspaceId);

                if (workspace == null || !workspace.HasScope(Client.Scope))
                {
                    _logger.LogInformation($"No audience match for workspace {spec?.WorkspaceId}: [{workspace?.Audience}] [{Client?.Scope}]");
                    throw new InvalidOperationException();
                }

                game = new Data.Gamespace
                {
                    GlobalId = spec.IsolationId,
                    Name = workspace.Name,
                    Workspace = workspace,
                    Audience = Client.Id.Untagged()
                    // ShareCode = Guid.NewGuid().ToString("N")
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
                var templates = String.IsNullOrEmpty(spec.Templates)
                    ? Mapper.Map<List<ConvergedTemplate>>(gamespace.Workspace.Templates)
                    : JsonSerializer.Deserialize<List<ConvergedTemplate>>(spec.Templates);

                ApplyIso(templates, spec);

                ExpandTemplates(templates, spec);

                // await AddNetworkServer(templates, spec);

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

            return await LoadState(gamespace, gamespace.WorkspaceId);
        }

        // private async Task AddNetworkServer(ICollection<ConvergedTemplate> templates, GamespaceSpec spec)
        // {
        //     if (spec.Network == null)
        //         return;

        //     var settings = new KeyValuePair<string,string>[]
        //     {
        //         new KeyValuePair<string,string>("newip", spec.Network.IpAddress),
        //         new KeyValuePair<string,string>("hosts", String.Join(";", spec.Network.HostFileEntries)),
        //         new KeyValuePair<string,string>("dhcp", String.Join(";", spec.Network.Dnsmasq))
        //     };

        //     var netServerTemplateEntity = await _templateStore.Load(_options.NetworkHostTemplateId);

        //     if (netServerTemplateEntity != null)
        //     {
        //         var netServerTemplate = Mapper.Map<ConvergedTemplate>(netServerTemplateEntity);

        //         netServerTemplate.AddSettings(settings);

        //         templates.Add(netServerTemplate);
        //     }
        // }

        private void ExpandTemplates(ICollection<ConvergedTemplate> templates, GamespaceSpec spec)
        {
            foreach (var t in templates.ToList())
            {
                var vmspec = spec.Vms?.SingleOrDefault(v => v.Name == t.Name);

                if (vmspec == null || vmspec.Replicas < 2)
                    continue;

                int total = Math.Min(vmspec.Replicas, _options.ReplicaLimit);

                for (int i = 1; i < total; i++)
                {
                    var tt = t.Clone<ConvergedTemplate>();

                    tt.Name += $"_{i}";

                    templates.Add(tt);
                }

                t.Name += "_0";
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
                Data.Workspace topo = await _workspaceStore.Load(topoId);

                if (topo == null || !topo.IsPublished)
                    throw new InvalidOperationException();

                state = new GameState();

                state.Name = gamespace?.Name ?? topo.Name;

                state.WorkspaceDocument = topo.Document;

                state.Vms = topo.Templates
                    .Where(t => !t.IsHidden)
                    .Select(t => new VmState { Name = t.Name, TemplateId = t.Id})
                    .ToArray();

            }
            else
            {
                state = Mapper.Map<GameState>(gamespace);

                state.Vms = gamespace.Workspace.Templates
                    .Where(t => !t.IsHidden)
                    .Select(t => new VmState { Name = t.Name, TemplateId = t.Id})
                    .ToArray();

                state.MergeVms(await _pod.Find(gamespace.GlobalId));
            }

            return state;
        }

        public async Task Destroy(string globalId)
        {
            var gamespace = await _gamespaceStore.Load(globalId);

            if (gamespace == null)
                throw new InvalidOperationException();

            await _pod.DeleteAll(gamespace.GlobalId);

            await _gamespaceStore.Delete(gamespace.Id);
        }

        public async Task<ConsoleSummary> Ticket(string vmId)
        {
            var info = await _pod.Display(vmId);

            var gamespace = await _gamespaceStore.Load(info.IsolationId);

            if (gamespace != null)
            {
                gamespace.LastActivity = DateTime.UtcNow;
                await _gamespaceStore.Update(gamespace);
            }

            if (info.Url.HasValue())
            {
                var src = new Uri(info.Url);

                info.Url = info.Url.Replace(src.Host, _options.ConsoleHost) + $"?vmhost={src.Host}";
            }

            return info;
        }

        public async Task<string> GetTemplates(int topoId)
        {
            var topo = await _workspaceStore.Load(topoId);

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

                    var gamespace = await _gamespaceStore.Load(vm.Name.Tag());

                    var allowedIsos = await _pod.GetVmIsoOptions(gamespace.Workspace.GlobalId);

                    string path = allowedIsos.Iso.Where(x => x.Contains(vmAction.Message)).FirstOrDefault();

                    _logger.LogDebug($"{vm.Name}, {vmAction.Message}, {gamespace.Workspace.Name}, {String.Join(" ", allowedIsos.Iso)}");

                    await _pod.ChangeConfiguration(vmAction.Id, new KeyValuePair<string,string>("iso", path));

                    result = true;

                    break;

            }

            return result;
        }

        public async Task<WorkspaceSummary[]> ListWorkspaces(Search search, CancellationToken ct = default(CancellationToken))
        {
            var q = _workspaceStore.List(search.Term)
                .Where(w => w.Audience.Contains(Client.Scope))
                .OrderBy(w => w.Name)
                .Skip(search.Skip);

            if (search.Take > 0)
                q = q.Take(search.Take);

            return Mapper.Map<WorkspaceSummary[]>(await q.ToArrayAsync(ct));
        }
    }
}
