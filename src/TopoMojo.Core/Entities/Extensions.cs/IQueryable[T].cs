using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace TopoMojo.Core.Entities.Extensions
{
    public static class IQueryableExtensions
    {
        public static IQueryable<T> ApplyPaging<T>(this IQueryable<T> q, Search search)
            where T : Entity
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
    }
}