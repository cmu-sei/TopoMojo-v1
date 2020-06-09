// Copyright 2020 Carnegie Mellon University. All Rights Reserved.
// Released under a 3 Clause BSD-style license. See LICENSE.md in the project root for license information.

namespace TopoMojo.Web
{
    public class DatabaseOptions
    {
        public string Provider { get; set; } = "InMemory";
        public string ConnectionString { get; set; } = "topomojo_db";
        public string SeedFile { get; set; } = "seed-data.json";
    }
}
