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
        public int WorkspaceLimit { get; set; }
        public string WhenCreated { get; set; }
    }

    public class ChangedProfile
    {
        public string GlobalId { get; set; }
        public string Name { get; set; }
    }

}
