using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using Microsoft.Extensions.Options;
using TopoMojo.Abstractions;

namespace TopoMojo.Core
{
    public class EntityManager<T>
        where T : BaseModel, new()
    {
        public EntityManager(TopoMojoDbContext db,
            IUserResolver userResolver,
            IOptions<CoreOptions> options,
            ILoggerFactory mill)
        {
            _db = db;
            _mill = mill;
            _userResolver = userResolver;

            _user = userResolver.GetCurrentUserAsync().Result;
            _logger = mill?.CreateLogger(this.GetType());
            _optAccessor = options;
            _options = options.Value;
            // if (_user != null && _user.Id > 0)
            //     _db.Attach(_user);
        }

        protected readonly TopoMojoDbContext _db;
        protected readonly Person _user;
        protected readonly ILogger _logger;
        protected readonly ILoggerFactory _mill;
        protected readonly IUserResolver _userResolver;
        protected readonly CoreOptions _options;
        protected readonly IOptions<CoreOptions> _optAccessor;

        public virtual async Task<Search<T>> ListAsync(Search<T> search)
        {
            IQueryable<T> q = ListQuery(search);
            search.Total = q.Count();
            search.Results = await (
                q.OrderBy(o=>o.Name)
                .ApplyPaging(search).ToArrayAsync());
            return search;
        }

        protected virtual IQueryable<T> ListQuery(Search search)
        {
            IQueryable<T> q = _db.Set<T>();

            if (search.Term.HasValue())
            {
                q = q.Where(o => o.Name.IndexOf(search.Term, StringComparison.CurrentCultureIgnoreCase) >= 0);
            }

            return q;
        }


        public virtual async Task<T> LoadAsync(int id)
        {
            return await _db.Set<T>().FindAsync(id);
        }

        public virtual async Task<T> SaveAsync (T t)
        {
            Normalize(t);
            if (t.Id > 0)
                _db.Update(t);
            else
                _db.Add(t);
            await _db.SaveChangesAsync();
            return t;
        }

        public virtual async Task<T> SaveSummaryAsync(T t)
        {
            T entity = null;
            Normalize(t);

            if (t.Id > 0)
            {
                entity = _db.Set<T>().Find(t.Id);
                entity.Name = t.Name;
                _db.Update(entity);
            }
            else
            {
                entity = new T();
                entity.Name = t.Name;
                _db.Add(t);
            }
            await _db.SaveChangesAsync();

            return t;
        }

        public virtual async Task<List<T>> SaveRangeAsync(List<T> list)
        {
            _db.UpdateRange(list.Where(x => x.Id > 0));
            _db.AddRange(list.Where(x => x.Id == 0));

            await _db.SaveChangesAsync();

            return list;
        }

        public virtual async Task DeleteAsync(int id)
        {
            T t = _db.Set<T>().Find(id);
            if (t != null)
            {
                _db.Remove(t);
                await _db.SaveChangesAsync();
            }
        }

        protected virtual void Normalize(T t)
        {
            if (!t.GlobalId.HasValue())
                t.GlobalId = Guid.NewGuid().ToString();

            if (t.WhenCreated == DateTime.MinValue)
                t.WhenCreated = DateTime.UtcNow;

            if (t.Name.HasValue() && t.Name.Length > 100)
                t.Name = t.Name.Substring(0,100);

        }

    }
}
