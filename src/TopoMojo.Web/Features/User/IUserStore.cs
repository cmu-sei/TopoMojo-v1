// Copyright 2020 Carnegie Mellon University. All Rights Reserved.
// Released under a 3 Clause BSD-style license. See LICENSE.md in the project root for license information.

using System.Threading.Tasks;
using TopoMojo.Models;

namespace TopoMojo.Data.Abstractions
{
    public interface IUserStore : IStore<User>
    {
        Task<User> Load(string id);
        Task<bool> CanInteract(string isolationId, string userId);
    }
}
