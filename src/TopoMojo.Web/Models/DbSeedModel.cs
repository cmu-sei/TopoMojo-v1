namespace TopoMojo.Models
{
    public class DbSeedModel
    {
        public DbSeedUser[] Users { get; set; } = new DbSeedUser[] {};
    }

    public class DbSeedUser
    {
        public string Name { get; set; }
        public string GlobalId { get; set; }
        public bool IsAdmin { get; set; }
    }
}