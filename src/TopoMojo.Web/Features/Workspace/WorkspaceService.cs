// Copyright 2020 Carnegie Mellon University. All Rights Reserved.
// Released under a 3 Clause BSD-style license. See LICENSE.md in the project root for license information.

using System;
using System.IO;
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
    public class WorkspaceService : _Service
    {
        public WorkspaceService(
            IWorkspaceStore workspaceStore,
            IGamespaceStore gamespaceStore,
            IHypervisorService podService,
            ILogger<WorkspaceService> logger,
            IMapper mapper,
            CoreOptions options,
            IIdentityResolver identityResolver
        ) : base (logger, mapper, options, identityResolver)
        {
            _workspaceStore = workspaceStore;
            _gamespaceStore = gamespaceStore;
            _pod = podService;
        }

        private readonly IWorkspaceStore _workspaceStore;
        private readonly IGamespaceStore _gamespaceStore;
        private readonly IHypervisorService _pod;

        /// <summary>
        /// List workspace summaries.
        /// </summary>
        /// <returns>Array of WorkspaceSummary</returns>
        public async Task<WorkspaceSummary[]> List(WorkspaceSearch search, User Actor, CancellationToken ct = default(CancellationToken))
        {
            var q = _workspaceStore.List(search.Term);

            if (!Actor.IsAdmin && !Actor.IsAgent)
                q = q.Where(p => p.Workers.Select(w => w.PersonId).Contains(Actor.Id));

            if (Actor.IsAgent)
                q = q.Where(w => w.Audience.Contains(search.aud));

            q = search.Sort == "age"
                ? q.OrderByDescending(w => w.WhenCreated)
                : q.OrderBy(w => w.Name);

            if (search.Skip > 0)
                q = q.Skip(search.Skip);

            if (search.Take > 0)
                q = q.Take(search.Take);

            return Mapper.Map<WorkspaceSummary[]>(
                await q.ToArrayAsync(ct)
            );
        }

        /// <summary>
        /// Lists workspaces with template detail.  This should only be exposed to priviledged users.
        /// </summary>
        /// <returns>Array of Workspaces</returns>
        public async Task<Workspace[]> ListDetail(Search search, CancellationToken ct = default(CancellationToken))
        {
            var q = _workspaceStore.List(search.Term);

            q = q.Include(t => t.Templates)
                    .Include(t => t.Workers)
                    .ThenInclude(w => w.Person);

            q = search.Sort == "age"
                ? q.OrderByDescending(w => w.WhenCreated)
                : q.OrderBy(w => w.Name);

            if (search.Skip > 0)
                q = q.Skip(search.Skip);

            if (search.Take > 0)
                q = q.Take(search.Take);

            return Mapper.Map<Workspace[]>(
                await q.ToArrayAsync(ct)
            );
        }

        internal async Task<Boolean> CheckWorkspaceLimit(string id)
        {
            return await _workspaceStore.CheckWorkspaceLimit(id);
        }

        /// <summary>
        /// Load a workspace by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns>Workspace</returns>
        // public async Task<Workspace> Load(int id)
        // {
        //     Data.Workspace entity = await _workspaceStore.Load(id);

        //     return Mapper.Map<Workspace>(entity);
        // }

        public async Task<Workspace> Load(string id)
        {
            Data.Workspace entity = await _workspaceStore.Load(id);

            return Mapper.Map<Workspace>(entity);
        }

        /// <summary>
        /// Create a new workspace
        /// </summary>
        /// <param name="model"></param>
        /// <param name="actorId"></param>
        /// <returns>Workspace</returns>
        public async Task<Workspace> Create(NewWorkspace model, int actorId)
        {
            var workspace = Mapper.Map<Data.Workspace>(model);

            workspace.TemplateLimit = _options.DefaultTemplateLimit;

            // workspace.ShareCode = Guid.NewGuid().ToString("N");

            // workspace.Author = User.Name;

            workspace.LastActivity = DateTime.UtcNow;

            if (workspace.Challenge.IsEmpty())
                workspace.Challenge = JsonSerializer.Serialize<Models.v2.ChallengeSpec>(
                    new Models.v2.ChallengeSpec()
                );

            workspace.Workers.Add(new Data.Worker
            {
                PersonId = actorId,
                Permission = Permission.Manager
            });

            workspace = await _workspaceStore.Create(workspace);
            // await _workspaceStore.Update(workspace);

            // TODO: consider handling document here

            return Mapper.Map<Workspace>(workspace);
        }

        /// <summary>
        /// Update an existing workspace.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<Workspace> Update(ChangedWorkspace model)
        {
            var entity = await _workspaceStore.Retrieve(model.GlobalId);

            Mapper.Map<ChangedWorkspace, Data.Workspace>(model, entity);

            await _workspaceStore.Update(entity);

            return Mapper.Map<Workspace>(entity);
        }

        public async Task UpdateChallenge(int id, ChallengeSpec spec)
        {
            var entity = await _workspaceStore.Retrieve(id);

            if (entity == null || !entity.CanEdit(User))
                throw new InvalidOperationException();

            // spec.Randoms.Clear();

            // hydrate question weights
            var unweighted = spec.Questions.Where(q => q.Weight == 0).ToArray();
            float max = spec.Questions.Sum(q => q.Weight);
            if (unweighted.Any())
            {
                float val = (1 - max) / unweighted.Length;
                foreach(var q in unweighted.Take(unweighted.Length - 1))
                {
                    q.Weight = val;
                    max += val;
                }
                unweighted.Last().Weight = 1 - max;
            }

            entity.Challenge = JsonSerializer.Serialize(spec, jsonOptions);

            await _workspaceStore.Update(entity);
        }

        public async Task<Models.v2.ChallengeSpec> GetChallenge(string id)
        {
            var entity = await _workspaceStore.Retrieve(id);

            string spec = entity.Challenge ??
                JsonSerializer.Serialize<Models.v2.ChallengeSpec>(new Models.v2.ChallengeSpec());

            return JsonSerializer.Deserialize<Models.v2.ChallengeSpec>(spec, jsonOptions);
        }

        public async Task UpdateChallenge(string id, Models.v2.ChallengeSpec spec)
        {
            var entity = await _workspaceStore.Retrieve(id);

            entity.Challenge = JsonSerializer.Serialize<Models.v2.ChallengeSpec>(spec, jsonOptions);

            await _workspaceStore.Update(entity);
        }

        /// <summary>
        /// Delete a workspace
        /// </summary>
        /// <param name="id"></param>
        /// <returns>Workspace</returns>
        public async Task<Workspace> Delete(string id)
        {
            var entity = await _workspaceStore.Retrieve(id);

            await _pod.DeleteAll(id);

            await _workspaceStore.Delete(id);

            return Mapper.Map<Workspace>(entity);
        }

        /// <summary>
        /// Determine if current user can edit workspace.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="actor"></param>
        /// <returns></returns>
        public async Task<bool> CanEdit(string id, User actor)
        {
            var entity = await _workspaceStore.Retrieve(id);

            return entity?.CanEdit(actor) ?? false;
        }

        /// <summary>
        /// Determine if current user can manage workspace.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="actor"></param>
        /// <returns></returns>
        public async Task<bool> CanManage(string id, User actor)
        {
            var entity = await _workspaceStore.Retrieve(id);

            return entity?.CanManage(actor) ?? false;
        }

        public async Task<bool> CanManage(int id, User actor)
        {
            var entity = await _workspaceStore.Retrieve(id);

            if (entity is null)
                entity = await _workspaceStore.FindByWorker(id);

            return entity?.CanManage(actor) ?? false;
        }

        /// <summary>
        /// Generate a new invitation code for a workspace.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<WorkspaceInvitation> Invite(string id)
        {
            var workspace = await _workspaceStore.Retrieve(id);

            workspace.ShareCode = Guid.NewGuid().ToString("N");

            await _workspaceStore.Update(workspace);

            return Mapper.Map<WorkspaceInvitation>(workspace);
        }

        /// <summary>
        /// Redeem an invitation code to join user to workspace.
        /// </summary>
        /// <param name="code"></param>
        /// <param name="actor"></param>
        /// <returns></returns>
        public async Task<WorkspaceSummary> Enlist(string code, User actor)
        {
            var workspace = await _workspaceStore.FindByShareCode(code);

            if (workspace == null)
                throw new ResourceNotFound();

            if (!workspace.Workers.Where(m => m.PersonId == actor.Id).Any())
            {
                workspace.Workers.Add(new Data.Worker
                {
                    PersonId = actor.Id,
                    Permission = workspace.Workers.Count > 0
                        ? Permission.Editor
                        : Permission.Manager
                });

                await _workspaceStore.Update(workspace);
            }

            return Mapper.Map<WorkspaceSummary>(workspace);
        }

        /// <summary>
        /// Remove a worker from a workspace.
        /// </summary>
        /// <param name="workerId"></param>
        /// <param name="actor"></param>
        /// <returns></returns>
        public async Task Delist(int workerId, User actor)
        {
            var workspace = await _workspaceStore.FindByWorker(workerId);

            var member = workspace.Workers
                .Where(p => p.Id == workerId)
                .SingleOrDefault();

            int managers = workspace.Workers
                .Count(w => w.Permission.HasFlag(Permission.Manager));

            // Only admins can remove the last remaining workspace manager
            if (!actor.IsAdmin
                && member.CanManage()
                && managers == 1)
                throw new InvalidOperationException();

            if (member != null)
            {
                workspace.Workers.Remove(member);

                await _workspaceStore.Update(workspace);
            }
        }

        /// <summary>
        /// Retrieve existing gamestates for a workspace.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<GameState[]> GetGames(string id)
        {
            var workspace = await _workspaceStore.LoadWithGamespaces(id);

            return Mapper.Map<GameState[]>(workspace.Gamespaces);
        }

        /// <summary>
        /// Delete all existing gamespaces of a workspace.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<GameState[]> KillGames(string id)
        {
            var workspace = await _workspaceStore.LoadWithGamespaces(id);

            foreach (var gamespace in workspace.Gamespaces)
            {
                await _pod.DeleteAll(gamespace.GlobalId);

                await _gamespaceStore.Delete(gamespace.Id);
            }

            return Mapper.Map<GameState[]>(workspace.Gamespaces);
        }
    }
}
