using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TopoMojo.Data.Abstractions
{
    public interface IStore<TEntity>
        where TEntity : class, IEntity
    {
        TopoMojoDbContext DbContext { get; }

        IQueryable<TEntity> List(string term = null);

        Task<TEntity> Create(TEntity entity);

        Task<IEnumerable<TEntity>> Create(IEnumerable<TEntity> range);

        Task<TEntity> Retrieve(int id, Func<IQueryable<TEntity>, IQueryable<TEntity>> includes = null);

        Task<TEntity> Retrieve(string id, Func<IQueryable<TEntity>, IQueryable<TEntity>> includes = null);

        Task Update(TEntity entity);

        Task Update(IEnumerable<TEntity> range);

        Task Delete(string id);

    }

}
