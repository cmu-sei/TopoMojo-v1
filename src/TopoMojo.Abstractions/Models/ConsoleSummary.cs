// Copyright 2020 Carnegie Mellon University. 
// Released under a MIT (SEI) license. See LICENSE.md in the project root. 

namespace TopoMojo.Models
{
    public class ConsoleSummary
    {
        public string Id { get; set; }
        public string IsolationId { get; set; }
        public string Name { get; set; }
        public string Url { get; set; }
        public bool IsRunning { get; set; }
    }
}
