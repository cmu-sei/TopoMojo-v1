// Copyright 2019 Carnegie Mellon University. All Rights Reserved.
// Released under a 3 Clause BSD-style license. See LICENSE.md in the project root for license information.

namespace TopoMojo.Data.Abstractions
{
    public interface IStoreUserContext
    {
        int UserId { get; set; }
        bool UserIsAdmin { get; set; }
    }
}
