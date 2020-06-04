// Copyright 2020 Carnegie Mellon University. 
// Released under a MIT (SEI) license. See LICENSE.md in the project root. 

namespace TopoMojo.Client
{
    public class Options
    {
        public string Url { get; set; }
        public string Key { get; set; }
        public int MaxRetries { get; set; } = 2;
    }
}
