using System.Collections.Generic;

namespace TopoMojo.Core
{
    public class Person: BaseModel
    {
        public bool IsAdmin { get; set; }
        public virtual ICollection<Permission> Permissions { get; set; } = new List<Permission>();
    }
}