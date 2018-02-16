using Microsoft.EntityFrameworkCore;
using TopoMojo.Data.Entities;

namespace TopoMojo.Data.EntityFrameworkCore
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

            builder.Entity<Message>().HasIndex(m => m.RoomId);
        }

        public DbSet<Topology> Topologies { get; set; }
        public DbSet<Template> Templates { get; set; }
        public DbSet<Gamespace> Gamespaces { get; set; }
        public DbSet<Profile> Profiles { get; set; }
        public DbSet<Worker> Workers { get; set; }
        public DbSet<Player> Players { get; set; }
        public DbSet<Message> Messages { get; set; }
    }
}
