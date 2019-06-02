// Copyright 2019 Carnegie Mellon University. All Rights Reserved.
// Licensed under the MIT (SEI) License. See LICENSE.md in the project root for license information.

namespace TopoMojo.Data.Abstractions
{
    public interface IRepositoryContext
    {
        int UserId { get; set; }
        bool UserIsAdmin { get; set; }
    }
}