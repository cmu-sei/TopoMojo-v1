// Copyright 2019 Carnegie Mellon University. All Rights Reserved.
// Released under a 3 Clause BSD-style license. See LICENSE.md in the project root for license information.

using System.Linq;
using System.Threading.Tasks;
using TopoMojo.Data.Entities;

namespace TopoMojo.Data.Abstractions
{
    public interface IGamespaceRepository : IRepository<Gamespace>
    {
        IQueryable<Gamespace> ListByProfile(int id);
        Task<Gamespace> FindByContext(int topoId, int profileId);
        Task<Gamespace> FindByPlayer(int playerId);
        Task<Gamespace> FindByShareCode(string code);
        IQueryable<Player> ListPlayers(int gamespaceId);
    }
}
