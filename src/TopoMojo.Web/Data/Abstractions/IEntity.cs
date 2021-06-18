// Copyright 2020 Carnegie Mellon University. All Rights Reserved.
// Released under a 3 Clause BSD-style license. See LICENSE.md in the project root for license information.

namespace TopoMojo.Data.Abstractions
{
    public interface IEntity
    {
        int Id { get; set; }
        string GlobalId { get; set; }
        System.DateTime WhenCreated { get; set; }
    }

}
