using Microsoft.EntityFrameworkCore;
using TopoMojo.Core.Entities;

namespace TopoMojo.Core.Data
{
    public class TopoMojoDbContext : DbContext
    {
        public TopoMojoDbContext(DbContextOptions<TopoMojoDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
        }

        public DbSet<Topology> Topologies { get; set; }
        public DbSet<Template> Templates { get; set; }
        public DbSet<Linker> Linkers { get; set; }
        public DbSet<Gamespace> Gamespaces { get; set; }
        public DbSet<Profile> Profiles { get; set; }
        public DbSet<Worker> Workers { get; set; }
        public DbSet<Player> Players { get; set; }
    }
}
