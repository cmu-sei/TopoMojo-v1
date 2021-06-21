// Copyright 2020 Carnegie Mellon University. All Rights Reserved.
// Released under a 3 Clause BSD-style license. See LICENSE.md in the project root for license information.

using System;
using System.Threading.Tasks;

namespace TopoMojo.Data.Abstractions
{
    public interface IWorkspaceStore : IStore<Workspace>
    {
        Task<Workspace> Load(string id);
        Task<Workspace> LoadFromInvitation(string code);
        Task<Workspace> LoadWithGamespaces(string id);
        Task<Workspace> LoadWithParents(string id);
        Task<Worker> FindWorker(string id, string subjectId);
        Task<bool> CanEdit(string id, string subjectId);
        Task<bool> CanManage(string id, string subjectId);
        Task<int> GetWorkspaceCount(string profileId);
        Task<bool> CheckWorkspaceLimit(string userId);
        Task<bool> HasGames(string id);
        Task<Workspace[]> DeleteStale(DateTime staleAfter, bool published, bool dryrun = true);
        Task<Workspace> Clone(string id);
    }
}
