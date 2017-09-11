using System.Collections.Generic;

namespace TopoMojo.Data.Entities
{
    public class Team
    {
        public int Id { get; set; }
        public string GlobalId { get; set; }
        public virtual ICollection<Member> Members { get; set; } = new List<Member>();

    }

    public class Member
    {
        public int Id { get; set; }
        public int TeamId { get; set; }
        public virtual Team Team { get; set; }
        public int PersonId { get; set; }
        public virtual Profile Person { get; set; }
        public Permission Permission { get; set; }
    }
}