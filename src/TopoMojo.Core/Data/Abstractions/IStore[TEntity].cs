// Copyright 2020 Carnegie Mellon University. All Rights Reserved.
// Released under a 3 Clause BSD-style license. See LICENSE.md in the project root for license information.

using System.Linq;
using System.Threading.Tasks;

namespace TopoMojo.Data.Abstractions
{
    public interface IStore<TEntity>
        where TEntity : class, IEntity
    {
        Task<TEntity> Add(TEntity entity);
        Task<bool> CanEdit(int id, User profile);
        Task<bool> CanManage(int id, User profile);
        IQueryable<TEntity> List();
        Task<TEntity> Load(int id);
        Task<TEntity> FindByGlobalId(string guid);
        Task Remove(TEntity entity);
        Task Update(TEntity entity);

    }
}
