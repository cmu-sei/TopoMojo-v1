// Copyright 2020 Carnegie Mellon University. All Rights Reserved.
// Released under a 3 Clause BSD-style license. See LICENSE.md in the project root for license information.

using System;
using System.Threading.Tasks;

namespace TopoMojo.Data.Abstractions
{
    public interface IWorkspaceStore : IStore<Workspace>
    {
        Task<Workspace> Load(string id);
        Task<Workspace> Load(int id);
        Task<Workspace> FindByShareCode(string code);
        Task<Workspace> FindByWorker(int id);
        Task<int> GetWorkspaceCount(string profileId);
        Task<Workspace> LoadWithGamespaces(string id);
        Task<Workspace> LoadWithParents(string id);
        Task<bool> HasGames(string id);
        Task<Workspace[]> DeleteStale(DateTime staleAfter, bool published, bool dryrun = true);
        Task<bool> CheckWorkspaceLimit(string userId);
    }
}
