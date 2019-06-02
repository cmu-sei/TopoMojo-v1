using System;
using TopoMojo.Data.Abstractions;

namespace TopoMojo.Data.Entities
{
    public class Player
    {
        public int Id { get; set; }
        public int GamespaceId { get; set; }
        public virtual Gamespace Gamespace { get; set; }
        public int PersonId { get; set; }
        public virtual Profile Person { get; set; }
        public Permission Permission { get; set; }
        public DateTime? LastSeen { get; set; }
    }
}
