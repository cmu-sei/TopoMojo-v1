// Copyright 2020 Carnegie Mellon University. All Rights Reserved.
// Released under a 3 Clause BSD-style license. See LICENSE.md in the project root for license information.

using System;
using System.Linq;
using System.Threading.Tasks;

namespace TopoMojo.Data.Abstractions
{
    public interface IGamespaceStore : IStore<Gamespace>
    {
        Task<Gamespace> Load(int id);
        Task<Gamespace> Load(string id);
        Task<Gamespace> LoadActiveByContext(string workspaceId, string subjectId);
        Task<Gamespace[]> ListByContext(string workspaceId, string subjectId);
        Task<Gamespace> FindByShareCode(string code);
        Task<Gamespace> FindByPlayer(int playerId);
        IQueryable<Gamespace> ListByProfile(string subjectId);
        Task<Player[]> LoadPlayers(string id);
        Task<bool> CanInteract(string id, string actorId);
    }
}
