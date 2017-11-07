namespace TopoMojo.Core.Models
{
    public class Player
    {
        public int Id { get; set; }
        public int PersonId { get; set; }
        public string PersonName { get; set; }
        public bool CanManage { get; set; }
        public bool CanEdit { get; set; }
        //public bool Online { get; set; }
    }
}