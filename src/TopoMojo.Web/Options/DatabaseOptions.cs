// Copyright 2019 Carnegie Mellon University. All Rights Reserved.
// Released under a 3 Clause BSD-style license. See LICENSE.md in the project root for license information.

namespace TopoMojo
{
    public class DatabaseOptions
    {
        public string Provider { get; set; }
        public string ConnectionString { get; set; }
        public string SeedFile { get; set; } = "seed-data.json";
    }
}
