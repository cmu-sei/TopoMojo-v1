// Copyright 2020 Carnegie Mellon University. All Rights Reserved.
// Released under a 3 Clause BSD-style license. See LICENSE.md in the project root for license information.

using System;
using System.Linq;
using System.Threading.Tasks;

namespace TopoMojo.Data.Abstractions
{
    public interface IGamespaceStore : IDataStore<Gamespace>
    {
        Task<Gamespace> FindByContext(int workspaceId, string subjectId);
        Task<Gamespace> FindByContext(string workspaceId, string subjectId);
        Task<Gamespace> FindByShareCode(string code);
        Task<Gamespace> FindByPlayer(int playerId);
        // IQueryable<Gamespace> ListByProfile(int id);
        IQueryable<Gamespace> ListByProfile(string subjectId);
    }
}
