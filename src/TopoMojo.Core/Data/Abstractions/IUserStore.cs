// Copyright 2020 Carnegie Mellon University. All Rights Reserved.
// Released under a 3 Clause BSD-style license. See LICENSE.md in the project root for license information.

using System.Threading.Tasks;
using TopoMojo.Models;

namespace TopoMojo.Data.Abstractions
{
    public interface IUserStore : IDataStore<User>
    {
        Task<User> LoadDetail(int id);
        Task<bool> MemberOf(string globalId, Models.User profile);
    }
}
