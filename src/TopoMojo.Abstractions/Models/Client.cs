// Copyright 2020 Carnegie Mellon University.
// Released under a MIT (SEI) license. See LICENSE.md in the project root.

namespace TopoMojo.Models
{
    public class Client
    {
        public string Id { get; set; }
        public string Scope { get; set; }
        public string Url { get; set; }
        public int SessionLimit { get; set; }
        public int GamespaceLimit { get; set; }
        public int PlayerGamespaceLimit { get; set; }
        public int MaxMinutes { get; set; }
    }
}
