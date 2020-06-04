// Copyright 2020 Carnegie Mellon University. All Rights Reserved.
// Released under a 3 Clause BSD-style license. See LICENSE.md in the project root for license information.

using System;
using System.Linq;
using System.Threading.Tasks;

namespace TopoMojo.Data.Abstractions
{
    public interface IGamespaceStore : IDataStore<Gamespace>
    {
        Task<Gamespace> FindByContext(int topoId, int profileId);
        Task<Gamespace> FindByPlayer(int playerId);
        Task<Gamespace> FindByShareCode(string code);
        IQueryable<Gamespace> ListByProfile(int id);
        Task<Gamespace[]> DeleteStale(DateTime staleAfter, bool dryrun = true);
    }
}
