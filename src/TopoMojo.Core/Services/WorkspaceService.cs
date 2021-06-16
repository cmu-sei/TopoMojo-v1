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
        public async Task<WorkspaceSummary[]> List(Search search, CancellationToken ct = default(CancellationToken))
        {
            var q = _workspaceStore.List(search.Term);

            if (!User.IsAdmin
                && !search.HasFilter("public")
                && !search.HasFilter("private")
            )
            {
                search.Filter = search.Filter.Append("private").ToArray();
            }

            if (search.HasFilter("public"))
                q = q.Where(t => t.IsPublished);

            if (search.HasFilter("private"))
                q = q.Where(p => p.Workers.Select(w => w.PersonId).Contains(User.Id));

            q = search.Sort == "age"
                ? q.OrderByDescending(w => w.WhenCreated)
                : q.OrderBy(w => w.Name);

            if (search.Skip > 0)
                q = q.Skip(search.Skip);

            if (search.Take > 0)
                q = q.Take(search.Take);

            return Mapper.Map<WorkspaceSummary[]>(
                await q.ToArrayAsync(ct),
                WithActor()
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
                await q.ToArrayAsync(ct),
                WithActor()
            );
        }

        /// <summary>
        /// Load a workspace by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns>Workspace</returns>
        public async Task<Workspace> Load(int id)
        {
            Data.Workspace entity = await _workspaceStore.Load(id);

            if (entity == null || !entity.CanEdit(User))
                throw new ActionForbidden();


            return Mapper.Map<Workspace>(entity, WithActor());
        }

        public async Task<Workspace> Load(string id)
        {
            Data.Workspace entity = await _workspaceStore.Load(id);

            if (entity == null || !entity.CanEdit(User))
                throw new ActionForbidden();

            return Mapper.Map<Workspace>(entity, WithActor());
        }

        /// <summary>
        /// Create a new workspace
        /// </summary>
        /// <param name="model"></param>
        /// <returns>Workspace</returns>
        public async Task<Workspace> Create(NewWorkspace model)
        {
            if (!User.IsCreator
                && User.WorkspaceLimit <= await _workspaceStore.GetWorkspaceCount(User.Id))
                throw new WorkspaceLimitReached();

            var workspace = Mapper.Map<Data.Workspace>(model);

            workspace.TemplateLimit = _options.DefaultTemplateLimit;

            workspace.ShareCode = Guid.NewGuid().ToString("N");

            workspace.Author = User.Name;

            workspace.LastActivity = DateTime.UtcNow;

            workspace.Challenge = JsonSerializer.Serialize<Models.v2.ChallengeSpec>(new Models.v2.ChallengeSpec());

            workspace = await _workspaceStore.Add(workspace);

            workspace.Workers.Add(new Data.Worker
            {
                PersonId = User.Id,
                Permission = Data.Permission.Manager
            });

            await _workspaceStore.Update(workspace);

            return Mapper.Map<Workspace>(workspace, WithActor());
        }

        /// <summary>
        /// Update an existing workspace.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<Workspace> Update(ChangedWorkspace model)
        {
            var entity = await _workspaceStore.Load(model.Id);

            if (entity == null || !entity.CanEdit(User))
                throw new InvalidOperationException();

            if (model.TemplateLimit == 0 || !User.IsAdmin)
                model.TemplateLimit = entity.TemplateLimit;

            Mapper.Map<ChangedWorkspace, Data.Workspace>(model, entity, WithActor());

            await _workspaceStore.Update(entity);

            return Mapper.Map<Workspace>(entity, WithActor());
        }

        public async Task UpdateChallenge(int id, ChallengeSpec spec)
        {
            var entity = await _workspaceStore.Load(id);

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
            var entity = await _workspaceStore.Load(id);

            if (entity == null || !entity.CanEdit(User))
                throw new InvalidOperationException();

            string spec = entity.Challenge ??
                JsonSerializer.Serialize<Models.v2.ChallengeSpec>(new Models.v2.ChallengeSpec());

            return JsonSerializer.Deserialize<Models.v2.ChallengeSpec>(spec, jsonOptions);
        }

        public async Task UpdateChallenge(string id, Models.v2.ChallengeSpec spec)
        {
            var entity = await _workspaceStore.Load(id);

            if (entity == null || !entity.CanEdit(User))
                throw new InvalidOperationException();

            entity.Challenge = JsonSerializer.Serialize<Models.v2.ChallengeSpec>(spec, jsonOptions);

            await _workspaceStore.Update(entity);
        }

        /// <summary>
        /// Delete a workspace
        /// </summary>
        /// <param name="id"></param>
        /// <returns>Workspace</returns>
        public async Task<Workspace> Delete(int id)
        {
            var entity = await _workspaceStore.Load(id);

            if (entity == null || !entity.CanManage(User))
                throw new InvalidOperationException();

            await _pod.DeleteAll(entity.GlobalId);

            await _workspaceStore.Delete(id);

            return Mapper.Map<Workspace>(entity, WithActor());
        }

        /// <summary>
        /// Determine if current user can edit workspace.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<bool> CanEdit(string id)
        {
            var entity = await _workspaceStore.Load(id);

            return entity != null
                ? entity.CanEdit(User)
                : false;
        }

        /// <summary>
        /// Generate a new invitation code for a workspace.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="revoke"></param>
        /// <returns></returns>
        public async Task<WorkspaceInvitation> Invite(int id)
        {
            var workspace = await _workspaceStore.Load(id);

            if (workspace == null || !workspace.CanEdit(User))
                throw new InvalidOperationException();

            workspace.ShareCode = Guid.NewGuid().ToString("N");

            await _workspaceStore.Update(workspace);

            return Mapper.Map<WorkspaceInvitation>(workspace);
        }

        /// <summary>
        /// Redeem an invitation code to join user to workspace.
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public async Task<WorkspaceSummary> Enlist(string code)
        {
            var workspace = await _workspaceStore.FindByShareCode(code);

            if (workspace == null)
                throw new InvalidOperationException();

            if (!workspace.Workers.Where(m => m.PersonId == User.Id).Any())
            {
                workspace.Workers.Add(new Data.Worker
                {
                    PersonId = User.Id,
                    Permission = Data.Permission.Editor
                    // LastSeen = DateTime.UtcNow
                });

                await _workspaceStore.Update(workspace);
            }

            return Mapper.Map<WorkspaceSummary>(workspace, WithActor());
        }

        /// <summary>
        /// Remove a worker from a workspace.
        /// </summary>
        /// <param name="workerId"></param>
        /// <returns></returns>
        public async Task Delist(int workerId)
        {
            var workspace = await _workspaceStore.FindByWorker(workerId);

            if (workspace == null || !workspace.CanManage(User))
                throw new InvalidOperationException();

            var member = workspace.Workers
                .Where(p => p.Id == workerId)
                .SingleOrDefault();

            // Only admins can remove the last remaining workspace manager
            if (!User.IsAdmin
                && member.Permission.CanManage()
                && workspace.Workers.Count(w => w.Permission.HasFlag(Data.Permission.Manager)) == 1)
                throw new InvalidOperationException();

            if (member != null)
            {
                workspace.Workers.Remove(member);

                await _workspaceStore.Update(workspace);
            }
        }

        /// <summary>
        /// Determine if gamespaces exist for a workspace.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<bool> HasGames(int id)
        {
           return await _workspaceStore.HasGames(id);
        }

        /// <summary>
        /// Retrieve existing gamestates for a workspace.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<GameState[]> GetGames(int id)
        {
            var workspace = await _workspaceStore.LoadWithGamespaces(id);

            if (workspace == null || !workspace.CanEdit(User))
                throw new InvalidOperationException();

            return Mapper.Map<GameState[]>(workspace.Gamespaces);
        }

        /// <summary>
        /// Delete all existing gamespaces of a workspace.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<GameState[]> KillGames(int id)
        {
            var workspace = await _workspaceStore.LoadWithGamespaces(id);

            if (workspace == null || !workspace.CanEdit(User))
                throw new InvalidOperationException();

            var result = workspace.Gamespaces.ToArray();

            foreach (var gamespace in result)
            {
                await _pod.DeleteAll(gamespace.GlobalId);

                await _gamespaceStore.Delete(gamespace.Id);
            }

            return Mapper.Map<GameState[]>(result);
        }
    }
}
