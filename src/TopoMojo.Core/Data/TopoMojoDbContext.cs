using System;
using Microsoft.EntityFrameworkCore;
using TopoMojo.Models;

namespace TopoMojo.Core
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

        //public DbSet<Simulation> Simulations { get; set; }
        public DbSet<Topology> Topologies { get; set; }
        public DbSet<Template> Templates { get; set; }
        //public DbSet<Document> Documents { get; set; }
        public DbSet<Person> People { get; set; }
        public DbSet<Permission> Permissions { get; set; }
        public DbSet<TemplateReference> TTLinkage { get; set; }
        public DbSet<Instance> Instances { get; set; }
        public DbSet<InstanceMember> InstanceMembers { get; set; }
    }
}
