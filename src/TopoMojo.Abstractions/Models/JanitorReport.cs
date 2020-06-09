// Copyright 2020 Carnegie Mellon University. 
// Released under a MIT (SEI) license. See LICENSE.md in the project root. 

using System;

namespace TopoMojo.Models
{
    public class JanitorReport
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Reason { get; set; }
        public DateTime Age { get; set; }
    }
}
