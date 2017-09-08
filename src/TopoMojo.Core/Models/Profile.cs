using System;
using System.Collections.Generic;

namespace TopoMojo.Core.Models
{
    public class Profile
    {
        public int Id { get; set; }
        public string GlobalId { get; set; }
        public string Name { get; set; }
        public bool IsAdmin { get; set; }
    }

}