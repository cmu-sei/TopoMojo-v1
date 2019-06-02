// Copyright 2019 Carnegie Mellon University. All Rights Reserved.
// Licensed under the MIT (SEI) License. See LICENSE.md in the project root for license information.

namespace TopoMojo.Models
{
    public class DbSeedModel
    {
        public DbSeedUser[] Users { get; set; } = new DbSeedUser[] {};
    }

    public class DbSeedUser
    {
        public string Name { get; set; }
        public string GlobalId { get; set; }
        public bool IsAdmin { get; set; }
    }
}