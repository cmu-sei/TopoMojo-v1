// Copyright 2021 Carnegie Mellon University. All Rights Reserved.
// Released under a 3 Clause BSD-style license. See LICENSE.md in the project root for license information.

using System;
using TopoMojo.Api.Data.Abstractions;

namespace TopoMojo.Api.Data
{
    public class ApiKey: IEntity
    {
        public string Id { get; set; }
        public string UserId { get; set; }
        public string Name { get; set; }
        public string Hash { get; set; }
        public DateTime WhenCreated { get; set; }
        public User User { get; set; }
    }
}
