using System.Collections.Generic;

namespace TopoMojo.Core.Entities
{
    public class Profile: Entity
    {
        public bool IsAdmin { get; set; }
        public virtual ICollection<Worker> Workspaces { get; set; } = new List<Worker>();
        public virtual ICollection<Player> Gamespaces { get; set; } = new List<Player>();
    }
}