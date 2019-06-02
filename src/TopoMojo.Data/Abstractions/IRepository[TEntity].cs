// Copyright 2019 Carnegie Mellon University. All Rights Reserved.
// Licensed under the MIT (SEI) License. See LICENSE.md in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TopoMojo.Data.Entities;

namespace TopoMojo.Data.Abstractions
{
    public interface IRepository<TEntity>
        where TEntity : class, IEntityPrimary
    {
        Task<TEntity> Add(TEntity entity);
        Task<bool> CanEdit(int id, Profile profile);
        Task<bool> CanManage(int id, Profile profile);
        IQueryable<TEntity> List();
        Task<TEntity> Load(int id);
        Task<TEntity> FindByGlobalId(string guid);
        Task Remove(TEntity entity);
        Task Update(TEntity entity);

    }
}