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
            var game = await _gamespaceStore.Load(spec.IsolationId);

            if (game == null)
            {
                var workspace = string.IsNullOrEmpty(spec.WorkspaceGuid)
                    ? await _workspaceStore.Load(spec.WorkspaceId)
                    : await _workspaceStore.Load(spec.WorkspaceGuid);

                if (workspace == null || !workspace.HasScope(Client.Scope))
                {
                    _logger.LogInformation($"No audience match for workspace {spec?.WorkspaceGuid}: [{workspace?.Audience}] [{Client?.Scope}]");
                    throw new InvalidOperationException();
                }

                game = new Data.Gamespace
                {
                    GlobalId = spec.IsolationId,
                    Name = workspace.Name,
                    Workspace = workspace,
                    // Audience = Client.Id.Untagged()
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

                await Task.WhenAll(deployTasks.ToArray());

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

            string[] targets = spec.IsoTarget?.Split(' ', StringSplitOptions.RemoveEmptyEntries) ?? new string[] {};

            foreach (var template in templates.Where(t => string.IsNullOrEmpty(t.Iso)))
            {
                if (targets.Length > 0 && !targets.Contains(template.Name))
                    continue;

                var vmspec = spec.Vms.FirstOrDefault(v => v.Name == template.Name);

                if (vmspec != null && vmspec.SkipIso)
                    continue;

                template.Iso =  $"{_options.GameEngineIsoFolder}/{spec.Iso}";
            }
        }

        private async Task<GameState> LoadState(Data.Gamespace gamespace, int topoId)
        {
            GameState state = null;

            // gamespace should never be null in the engine service
            if (gamespace == null)
                throw new InvalidOperationException();

            // get vm's, look up template, add if template not mark as hidden.

            state = Mapper.Map<GameState>(gamespace);

            var vmState = new List<VmState>();

            var vms = await _pod.Find(gamespace.GlobalId);

            foreach (Vm vm in vms)
            {
                string name = vm.Name.Untagged();

                // a vm could be a replica, denoted by `_1` or some number,
                // so strip that to find template.
                int x = name.LastIndexOf('_');

                var tmpl = gamespace.Workspace.Templates
                    .Where(t => !t.IsHidden && t.Name == name)
                    .FirstOrDefault();

                if (tmpl == null && x == name.Length - 2)
                {
                    name = name.Substring(0, x);

                    tmpl = gamespace.Workspace.Templates
                    .Where(t => !t.IsHidden && t.Name == name)
                    .FirstOrDefault();
                }

                if (tmpl != null)
                {
                    vmState.Add(new VmState
                    {
                        Id = vm.Id,
                        Name = vm.Name,
                        IsRunning = (vm.State == VmPowerState.Running)
                        // TemplateId = tmpl.Id
                    });
                }
            }

            state.Vms = vmState.ToArray();

            return state;
        }

        public async Task Destroy(string globalId)
        {
            await _pod.DeleteAll(globalId);

            var gamespace = await _gamespaceStore.Load(globalId);

            if (gamespace == null)
                return;

            await _gamespaceStore.Delete(gamespace.Id);
        }

        public async Task<ConsoleSummary> Ticket(string vmId)
        {
            var info = await _pod.Display(vmId);

            var gamespace = await _gamespaceStore.Load(info.IsolationId);

            // if (gamespace != null)
            // {
            //     gamespace.LastActivity = DateTime.UtcNow;
            //     await _gamespaceStore.Update(gamespace);
            // }

            if (info.Url.HasValue())
            {
                var src = new Uri(info.Url);

                info.Url = info.Url.Replace(src.Host, _options.ConsoleHost) + $"?vmhost={src.Host}";
            }

            return info;
        }

        public async Task<string> GetTemplates(int topoId)
        {
            var topo = await _workspaceStore.LoadWithParents(topoId);

            return (topo != null)
                ? JsonSerializer.Serialize(Mapper.Map<List<ConvergedTemplate>>(topo.Templates), null)
                : "";
        }

        public async Task<bool> ChangeVm(VmAction vmAction)
        {
            bool result = false;
            Vm vm = null;

            if (!Guid.TryParse(vmAction.Id, out Guid guid))
            {
                // lookup id from name
                vm = await _pod.Load(vmAction.Id);
                vmAction.Id = vm.Id;
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

                    if (vm == null)
                        return false;

                    // look up valid iso path
                    var gamespace = await _gamespaceStore.Load(vm.Name.Tag());

                    var allowedIsos = await _pod.GetVmIsoOptions(gamespace.Workspace.GlobalId);

                    string path = allowedIsos.Iso.Where(x => x.Contains(vmAction.Message)).FirstOrDefault();

                    _logger.LogDebug($"{vm.Name}, {vmAction.Message}, {gamespace.Workspace.Name}, {String.Join(" ", allowedIsos.Iso)}");

                    await _pod.ChangeConfiguration(
                        vmAction.Id,
                        new VmKeyValue
                        {
                            Key = "iso",
                            Value = path
                        }
                    );

                    result = true;

                    break;

                case "net":

                    await _pod.ChangeConfiguration(
                        vmAction.Id,
                        new VmKeyValue
                        {
                            Key = "net",
                            Value = vmAction.Message
                        }
                    );

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

            return Mapper.Map<WorkspaceSummary[]>(await q.ToArrayAsync(ct), WithActor());
        }

        public async Task<Registration> Register(RegistrationRequest registration)
        {
            // check client scope / workspace audience
            var workspace = await _workspaceStore.Load(registration.ResourceId);

            if (workspace == null)
                throw new ResourceNotFound();

            if (!workspace.HasScope(Client.Scope))
                throw new InvalidClientAudience();

            var game = await _gamespaceStore.ListByProfile(registration.SubjectId)
                .SingleOrDefaultAsync(m => m.GlobalId == registration.ResourceId);

            string id = game?.GlobalId ?? Guid.NewGuid().ToString();
            string token = Guid.NewGuid().ToString("n");

            Challenge challenge = null;
            try
            {
                var spec = JsonSerializer.Deserialize<ChallengeSpec>(workspace.Challenge, jsonOptions);
                challenge = Mapper.Map<Challenge>(spec);
                challenge.GamespaceId = id;
            }
            catch {}

            return new Registration{
                SubjectId = registration.SubjectId,
                SubjectName = registration.SubjectName,
                ResourceId = registration.ResourceId,
                GamespaceId = id,
                Token = token,
                RedirectUrl =  _options.LaunchUrl + token,
                ClientId = Client.Id,
                Challenge = challenge
            };
        }

        public async Task<Challenge> Grade(Challenge challenge)
        {
            var gamespace = await _gamespaceStore.Load(challenge.GamespaceId);

            if (gamespace == null || string.IsNullOrEmpty(gamespace.Challenge))
                throw new ResourceNotFound();

            var spec = JsonSerializer.Deserialize<ChallengeSpec>(gamespace.Challenge, jsonOptions);

            int index = -1;

            foreach (var q in challenge.Questions)
            {
                index += 1;

                var qs = spec.Questions.Skip(index).Take(1).FirstOrDefault();

                if (qs == null)
                    continue;

                q.IsCorrect = GradeQuestion(qs.Grader, qs.Answer, q.Answer);
            }

            challenge.Score = challenge.Questions
                .Where(q => q.IsCorrect)
                .Select(q => q.Weight)
                .Sum();

            return await Task.FromResult(challenge);
        }

        private bool GradeQuestion(AnswerGraderOld grader, string expected, string submitted)
        {
            string[] a = expected.ToLower().Replace(" ", "").Split('|');
            string b = submitted.ToLower().Replace(" ", "");

            switch (grader) {

                case AnswerGraderOld.Match:
                return a.First().Equals(b);

                case AnswerGraderOld.MatchAny:
                return a.Contains(b);

                case AnswerGraderOld.MatchAll:
                return a.Intersect(
                    b.Split(new char[] { ',', ';', ':', '|'})
                ).ToArray().Length == a.Length;

            }

            return false;
        }

        public async Task<Challenge> Hints(Challenge challenge)
        {
            var gamespace = await _gamespaceStore.Load(challenge.GamespaceId);

            if (gamespace == null || string.IsNullOrEmpty(gamespace.Challenge))
                throw new ResourceNotFound();

            var spec = JsonSerializer.Deserialize<ChallengeSpec>(gamespace.Challenge, jsonOptions);

            int index = -1;

            foreach (var q in challenge.Questions)
            {
                index += 1;

                var qs = spec.Questions.Skip(index).Take(1).FirstOrDefault();

                if (qs == null)
                    continue;

                q.Hint = qs.Hint;
            }

            return await Task.FromResult(challenge);
        }
    }
}
