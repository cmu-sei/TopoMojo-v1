using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TopoMojo.Data.Abstractions;
using TopoMojo.Data.Entities;

namespace TopoMojo.Data.EntityFrameworkCore
{
    public class TemplateRepository : Repository<Template>, ITemplateRepository
    {
        public TemplateRepository (
            TopoMojoDbContext db
        ) : base(db) { }

        public override async Task<Template> Load(int id)
        {
            return await DbContext.Templates
                .Include(tt => tt.Parent)
                .Include(tt => tt.Topology)
                .Where(tt => tt.Id == id)
                .SingleOrDefaultAsync();
        }

        public async Task<bool> HasLinkedTemplates(int parentId)
        {
             return await DbContext.Templates
                .Where(t => t.ParentId == parentId)
                .AnyAsync();
        }

        public async Task<Template[]> ListLinkedTemplates(int parentId)
        {
            return await DbContext.Templates
                .Include(t => t.Topology)
                .Where(t => t.ParentId == parentId)
                .ToArrayAsync();
        }

        public override async Task<bool> CanEdit(int topologyId, Profile profile)
        {
            if (profile.IsAdmin)
                return true;

            return await DbContext.Workers
                .Where(p => p.TopologyId == topologyId
                    && p.PersonId == profile.Id
                    && p.Permission.HasFlag(Permission.Editor))
                .AnyAsync();

        }
        }
}