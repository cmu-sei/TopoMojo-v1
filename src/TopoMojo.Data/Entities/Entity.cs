using System;
using System.Collections.Generic;

namespace TopoMojo.Data.Entities
{
    public class Entity
    {
        public int Id { get; set; }
        public string GlobalId { get; set; }
        public string Name { get; set; }
        public DateTime WhenCreated { get; set; }
    }

}