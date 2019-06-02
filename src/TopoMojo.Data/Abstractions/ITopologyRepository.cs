// Copyright 2019 Carnegie Mellon University. All Rights Reserved.
// Licensed under the MIT (SEI) License. See LICENSE.md in the project root for license information.

using System.Threading.Tasks;
using TopoMojo.Data.Entities;

namespace TopoMojo.Data.Abstractions
{
    public interface ITopologyRepository : IRepository<Topology>
    {
        Task<Topology> FindByShareCode(string code);
        Task<Topology> FindByWorker(int id);
        Task<int> GetWorkspaceCount(int profileId);
        Task<Topology> LoadWithParents(int id);
    }
}
