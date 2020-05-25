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
        IQueryable<TEntity> List(string term = null);
        Task<TEntity> Load(int id);
        Task<TEntity> Load(string id);
        Task<TEntity> Add(TEntity entity);
        Task Update(TEntity entity);
        Task Delete(int id);
        Task<bool> Exists(int id);
    }
}
