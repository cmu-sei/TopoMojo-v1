// Copyright 2019 Carnegie Mellon University. All Rights Reserved.
// Released under a 3 Clause BSD-style license. See LICENSE.md in the project root for license information.

using System.Threading.Tasks;
using TopoMojo.Data.Entities;

namespace TopoMojo.Data.Abstractions
{
    public interface IProfileRepository : IRepository<Profile>
    {
        Task<Profile> LoadDetail(int id);
        Task<bool> CanEditSpace(string globalId, Profile profile);
        Task<bool> IsEmpty();
    }
}
