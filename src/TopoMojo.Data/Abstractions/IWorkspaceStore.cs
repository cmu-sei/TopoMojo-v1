// Copyright 2019 Carnegie Mellon University. All Rights Reserved.
// Released under a 3 Clause BSD-style license. See LICENSE.md in the project root for license information.

using System.Threading.Tasks;

namespace TopoMojo.Data.Abstractions
{
    public interface IWorkspaceStore : IStore<Topology>
    {
        Task<Topology> FindByShareCode(string code);
        Task<Topology> FindByWorker(int id);
        Task<int> GetWorkspaceCount(int profileId);
        Task<Topology> LoadWithParents(int id);
    }
}
