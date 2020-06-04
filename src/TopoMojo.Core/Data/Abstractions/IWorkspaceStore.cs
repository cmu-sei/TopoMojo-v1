// Copyright 2020 Carnegie Mellon University. All Rights Reserved.
// Released under a 3 Clause BSD-style license. See LICENSE.md in the project root for license information.

using System;
using System.Threading.Tasks;

namespace TopoMojo.Data.Abstractions
{
    public interface IWorkspaceStore : IDataStore<Workspace>
    {
        Task<Workspace> FindByShareCode(string code);
        Task<Workspace> FindByWorker(int id);
        Task<int> GetWorkspaceCount(int profileId);
        Task<Workspace> LoadWithGamespaces(int id);
        Task<Workspace> LoadWithParents(int id);
        Task<bool> HasGames(int id);
        Task<Workspace[]> DeleteStale(DateTime staleAfter, bool published, bool dryrun = true);
    }
}
