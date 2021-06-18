// Copyright 2020 Carnegie Mellon University.
// Released under a MIT (SEI) license. See LICENSE.md in the project root.

using System;
using System.Linq;
using System.Threading.Tasks;

namespace TopoMojo.Data.Abstractions
{
    public interface IDataStore<TEntity>
        where TEntity : class
    {
        TopoMojoDbContext DbContext { get; }
        IQueryable<TEntity> List(string term = null);
        Task<TEntity> Create(TEntity entity);
        Task<TEntity> Retrieve(int id);
        Task<TEntity> Retrieve(string id);
        Task Update(TEntity entity);
        Task Delete(int id);
    }
}
