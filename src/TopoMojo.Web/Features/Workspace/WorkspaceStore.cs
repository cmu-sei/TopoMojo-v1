// Copyright 2020 Carnegie Mellon University. All Rights Reserved.
// Released under a 3 Clause BSD-style license. See LICENSE.md in the project root for license information.

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using TopoMojo.Data.Abstractions;

namespace TopoMojo.Data
{
    public class WorkspaceStore : Store<Workspace>, IWorkspaceStore
    {
        public WorkspaceStore (
            TopoMojoDbContext db
        ) : base(db) { }

        public override IQueryable<Workspace> List(string term = null)
        {
            var q = base.List();

            if (!string.IsNullOrEmpty(term))
            {
                term = term.ToLower();

                q = q.Where(t =>
                    t.Name.ToLower().Contains(term) ||
                    t.Description.ToLower().Contains(term) ||
                    t.Author.ToLower().Contains(term) ||
                    t.Audience.ToLower().Contains(term)
                );
            }

            return q.Include(t => t.Workers);
        }

        public async Task<Workspace> Load(int id)
        {
            return await base.Retrieve(id, query => query
                .Include(t => t.Templates)
                .Include(t => t.Workers).ThenInclude(w => w.Person)
                .Include(t => t.Gamespaces)
            );
        }

        public async Task<Workspace> Load(string id)
        {
            return await base.Retrieve(id, query => query
                .Include(t => t.Templates)
                .Include(t => t.Workers).ThenInclude(w => w.Person)
                .Include(t => t.Gamespaces)
            );
        }

        public async Task<Workspace> LoadWithGamespaces(string id)
        {
            return await base.Retrieve(id, query => query
                .Include(t => t.Gamespaces)
                .Include(t => t.Workers)
            );
        }

        public async Task<Workspace> LoadWithParents(int id)
        {
            return await base.Retrieve(id, query => query
                .Include(t => t.Workers)
                .Include(t => t.Templates)
                .ThenInclude(o => o.Parent)
            );
        }

        public async Task<Workspace> FindByShareCode(string code)
        {
            return await DbContext.Workspaces
                .Include(t => t.Templates)
                .Include(t => t.Workers).ThenInclude(w => w.Person)
                .Where(t => t.ShareCode == code)
                .SingleOrDefaultAsync();
        }

        public async Task<Workspace> FindByWorker(int workerId)
        {
            int id = await DbContext.Workers
                .Where(p => p.Id == workerId)
                .Select(p => p.WorkspaceId)
                .SingleOrDefaultAsync();

            return (id > 0)
                ? await Retrieve(id)
                : null;
        }


        // public override async Task Delete(string id)
        // {
        //     var workspace = await Retrieve(id);

        //     DbContext.Templates.RemoveRange(workspace.Templates);

        //     DbContext.Remove(workspace);

        //     await DbContext.SaveChangesAsync();
        // }

        public async Task<int> GetWorkspaceCount(int profileId)
        {
            return await DbContext.Workers
                .CountAsync(w => w.PersonId == profileId && w.Permission.HasFlag(Permission.Manager));
        }

        public async Task<bool> HasGames(string id)
        {
            return await DbContext.Gamespaces.AnyAsync(g =>
                g.Workspace.GlobalId == id
            );
        }

        public async Task<Workspace[]> DeleteStale(DateTime staleMarker, bool published, bool dryrun = true)
        {
            var query = published
                ? DbContext.Workspaces.Where(w => w.IsPublished || !string.IsNullOrWhiteSpace(w.Audience))
                : DbContext.Workspaces.Where(w => !w.IsPublished && string.IsNullOrWhiteSpace(w.Audience));

            var results = await query
                .Where(g => g.LastActivity < staleMarker)
                .ToArrayAsync();

            if (!dryrun)
            {
                DbContext.Workspaces.RemoveRange(results);

                await DbContext.SaveChangesAsync();
            }

            return results;
        }

        public async Task<bool> CheckWorkspaceLimit(string userId)
        {
            var user = await DbContext.Users
                .Include(u => u.Workspaces)
                .FirstOrDefaultAsync(u => u.GlobalId == userId);

            return user is Data.User
                && user.Workspaces.Count(w => w.Permission == Permission.Manager) < user.WorkspaceLimit;

        }
    }
}
