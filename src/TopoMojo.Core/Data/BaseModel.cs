using System;
using System.Collections.Generic;

namespace TopoMojo.Core
{
    public class BaseModel
    {
        public int Id { get; set; }
        public string GlobalId { get; set; }
        public string Name { get; set; }
        public DateTime WhenCreated { get; set; }
    }

}