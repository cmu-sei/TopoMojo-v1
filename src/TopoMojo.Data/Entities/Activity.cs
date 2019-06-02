// Copyright 2019 Carnegie Mellon University. All Rights Reserved.
// Licensed under the MIT (SEI) License. See LICENSE.md in the project root for license information.

using System;
using TopoMojo.Data.Entities;

namespace TopoMojo.Data
{
    public class Activity
    {
        public int Id { get; set; }
        public int ActorId { get; set; }
        public int AssetId { get; set; }
        public ActivityType Action { get; set; }
        public DateTime At { get; set; }
        public string Actor { get; set; }
        public string Asset { get; set; }
        public string Annotation { get; set; }
    }

    public enum ActivityType
    {
        Created,
        Deleted,
        Published,
        Unpublished,
        Launched
    }
}
