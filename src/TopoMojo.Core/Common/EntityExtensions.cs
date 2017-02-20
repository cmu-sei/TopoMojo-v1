using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace TopoMojo.Core
{
    public static class EntityExtensions
    {
        public static IQueryable<T> ApplyPaging<T>(this IQueryable<T> q, Search search)
            where T : BaseModel
        {
            if (search.Skip > 0)
            {
                q = q.OrderBy(o => o.Name);
                q = q.Skip(search.Skip);
            }

            if (search.Take > 0)
            {
                q = q.Take(search.Take);
            }

            return q;
        }

        public async static Task<PermissionFlag> PermissionFor(this DbSet<Permission> db, int personId, int entityId, EntityType type)
        {
            Permission permission = await db.Where(m => m.PersonId == personId && m.TopologyId == entityId)
                .FirstOrDefaultAsync();
            return (permission != null)
                ? permission.Value
                : PermissionFlag.None;
        }

        public static bool CanManage(this PermissionFlag flag)
        {
            return flag.HasFlag(PermissionFlag.Manager);
        }
        public static bool CanEdit(this PermissionFlag flag)
        {
            return flag.HasFlag(PermissionFlag.Editor);
        }
        public static bool IsPending(this PermissionFlag flag)
        {
            return flag == PermissionFlag.None;
        }
    }
}