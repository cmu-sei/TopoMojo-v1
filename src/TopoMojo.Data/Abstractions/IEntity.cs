// Copyright 2019 Carnegie Mellon University. All Rights Reserved.
// Licensed under the MIT (SEI) License. See LICENSE.md in the project root for license information.

namespace TopoMojo.Data.Abstractions
{
    public interface IEntity
    {
        int Id { get; set; }
        System.DateTime WhenCreated { get; set; }
    }

    public interface IEntityPrimary : IEntity
    {
        string GlobalId { get; set; }
        string Name { get; set; }

    }
}