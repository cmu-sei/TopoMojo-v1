// Copyright 2019 Carnegie Mellon University. All Rights Reserved.
// Licensed under the MIT (SEI) License. See LICENSE.md in the project root for license information.

namespace TopoMojo.Models
{
    public class Map
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Image { get; set; }
        public string Resolution { get; set; }
        public string Palette { get; set; }
        public MapNode[] Nodes { get; set; }
    }

    public class MapNode
    {
        public string Name { get; set; }
        public string Loc { get; set; }
        public int I { get; set; }
        public bool Hot { get; set; }
        public bool Link { get; set; }
    }
}