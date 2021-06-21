// Copyright 2021 Carnegie Mellon University. All Rights Reserved.
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
using TopoMojo.Data.Abstractions;
using TopoMojo.Extensions;
using TopoMojo.Hypervisor;
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
            CoreOptions options
        ) : base (logger, mapper, options)
        {
            _store = workspaceStore;
            _gamespaceStore = gamespaceStore;
            _pod = podService;
        }

        private readonly IWorkspaceStore _store;
        private readonly IGamespaceStore _gamespaceStore;
        private readonly IHypervisorService _pod;

        /// <summary>
        /// List workspace summaries.
        /// </summary>
        /// <returns>Array of WorkspaceSummary</returns>
        public async Task<WorkspaceSummary[]> List(WorkspaceSearch search, string subjectId, bool sudo, CancellationToken ct = default(CancellationToken))
        {
            var q = _store.List(search.Term);

            if (search.WantsAudience)
                q = q.Where(w => w.Audience.Contains(search.aud));

            else if (!sudo)
                q = q.Where(p => p.Workers.Any(w => w.SubjectId == subjectId));

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
            var q = _store.List(search.Term);

            q = q.Include(t => t.Templates)
                    .Include(t => t.Workers)
                    // .ThenInclude(w => w.Person)
            ;

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

        public async Task<Boolean> CheckWorkspaceLimit(string id)
        {
            return await _store.CheckWorkspaceLimit(id);
        }

        public async Task<Workspace> Load(string id)
        {
            Data.Workspace entity = await _store.Load(id);

            return Mapper.Map<Workspace>(entity);
        }

        /// <summary>
        /// Create a new workspace
        /// </summary>
        /// <param name="model"></param>
        /// <param name="subjectId"></param>
        /// <returns>Workspace</returns>
        public async Task<Workspace> Create(NewWorkspace model, string subjectId)
        {
            var workspace = Mapper.Map<Data.Workspace>(model);

            workspace.TemplateLimit = _options.DefaultTemplateLimit;

            // workspace.ShareCode = Guid.NewGuid().ToString("N");

            // workspace.Author = User.Name;

            workspace.LastActivity = DateTime.UtcNow;

            if (workspace.Challenge.IsEmpty())
                workspace.Challenge = JsonSerializer.Serialize<ChallengeSpec>(
                    new ChallengeSpec()
                );

            workspace.Workers.Add(new Data.Worker
            {
                SubjectName = subjectId,
                Permission = Permission.Manager
            });

            workspace = await _store.Create(workspace);
            // await _workspaceStore.Update(workspace);

            // TODO: consider handling document here

            return Mapper.Map<Workspace>(workspace);
        }

        public async Task<Workspace> Clone(string id)
        {
            return Mapper.Map<Workspace>(
                await _store.Clone(id)
            );
        }

        /// <summary>
        /// Update an existing workspace.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<Workspace> Update(ChangedWorkspace model)
        {
            var entity = await _store.Retrieve(model.Id);

            Mapper.Map<ChangedWorkspace, Data.Workspace>(model, entity);

            await _store.Update(entity);

            return Mapper.Map<Workspace>(entity);
        }

        public async Task<ChallengeSpec> GetChallenge(string id)
        {
            var entity = await _store.Retrieve(id);

            string spec = entity.Challenge ??
                JsonSerializer.Serialize<ChallengeSpec>(new ChallengeSpec());

            return JsonSerializer.Deserialize<ChallengeSpec>(spec, jsonOptions);
        }

        public async Task UpdateChallenge(string id, ChallengeSpec spec)
        {
            var entity = await _store.Retrieve(id);

            entity.Challenge = JsonSerializer.Serialize<ChallengeSpec>(spec, jsonOptions);

            await _store.Update(entity);
        }

        /// <summary>
        /// Delete a workspace
        /// </summary>
        /// <param name="id"></param>
        /// <returns>Workspace</returns>
        public async Task<Workspace> Delete(string id)
        {
            var entity = await _store.Retrieve(id);

            await _pod.DeleteAll(id);

            // TODO: cleanly delete workspace templates

            await _store.Delete(id);

            return Mapper.Map<Workspace>(entity);
        }

        /// <summary>
        /// Determine if subject can edit workspace.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="subjectId"></param>
        /// <returns></returns>
        public async Task<bool> CanEdit(string id, string subjectId)
        {
            return await _store.CanEdit(id, subjectId);
        }

        /// <summary>
        /// Determine if subject can manage workspace.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="subjectId"></param>
        /// <returns></returns>
        public async Task<bool> CanManage(string id, string subjectId)
        {
            return await _store.CanManage(id, subjectId);
        }

        /// <summary>
        /// Generate a new invitation code for a workspace.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<WorkspaceInvitation> Invite(string id)
        {
            var workspace = await _store.Retrieve(id);

            workspace.ShareCode = Guid.NewGuid().ToString("N");

            await _store.Update(workspace);

            return Mapper.Map<WorkspaceInvitation>(workspace);
        }

        /// <summary>
        /// Redeem an invitation code to join user to workspace.
        /// </summary>
        /// <param name="code"></param>
        /// <param name="subjectId"></param>
        /// <param name="subjectName"></param>
        /// <returns></returns>
        public async Task<WorkspaceSummary> Enlist(string code, string subjectId, string subjectName)
        {
            var workspace = await _store.LoadFromInvitation(code);

            if (workspace == null)
                throw new ResourceNotFound();

            if (!workspace.Workers.Where(m => m.SubjectId == subjectId).Any())
            {
                workspace.Workers.Add(new Data.Worker
                {
                    SubjectId = subjectId,
                    SubjectName = subjectName,
                    Permission = workspace.Workers.Count > 0
                        ? Permission.Editor
                        : Permission.Manager
                });

                await _store.Update(workspace);
            }

            return Mapper.Map<WorkspaceSummary>(workspace);
        }

        /// <summary>
        /// Remove a worker from a workspace.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="subjectId"></param>
        /// <param name="sudo"></param>
        /// <returns></returns>
        public async Task Delist(string id, string subjectId, bool sudo)
        {
            var workspace = await _store.Load(id);

            var member = workspace.Workers
                .Where(p => p.SubjectId == subjectId)
                .SingleOrDefault();

            if (member == null)
                return;

            int managers = workspace.Workers
                .Count(w => w.Permission.HasFlag(Permission.Manager));

            // Only admins can remove the last remaining workspace manager
            if (!sudo
                && member.CanManage
                && managers == 1)
                throw new ActionForbidden();

            workspace.Workers.Remove(member);

            await _store.Update(workspace);
        }

        /// <summary>
        /// Retrieve existing gamestates for a workspace.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<GameState[]> GetGames(string id)
        {
            var workspace = await _store.LoadWithGamespaces(id);

            return Mapper.Map<GameState[]>(workspace.Gamespaces);
        }

        /// <summary>
        /// Delete all existing gamespaces of a workspace.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<GameState[]> KillGames(string id)
        {
            var workspace = await _store.LoadWithGamespaces(id);

            foreach (var gamespace in workspace.Gamespaces)
            {
                await _pod.DeleteAll(gamespace.Id);

                await _gamespaceStore.Delete(gamespace.Id);
            }

            return Mapper.Map<GameState[]>(workspace.Gamespaces);
        }
    }
}
