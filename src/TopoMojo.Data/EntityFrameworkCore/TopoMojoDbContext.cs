// Copyright 2019 Carnegie Mellon University. All Rights Reserved.
// Released under a 3 Clause BSD-style license. See LICENSE.md in the project root for license information.

using Microsoft.EntityFrameworkCore;

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

            builder.Entity<Topology>().HasAlternateKey(t => t.GlobalId);
            builder.Entity<Template>().HasAlternateKey(t => t.GlobalId);
            builder.Entity<Gamespace>().HasAlternateKey(t => t.GlobalId);
            builder.Entity<Profile>().HasAlternateKey(t => t.GlobalId);
            builder.Entity<Message>().HasIndex(m => m.RoomId);
        }

        public DbSet<Topology> Topologies { get; set; }
        public DbSet<Template> Templates { get; set; }
        public DbSet<Gamespace> Gamespaces { get; set; }
        public DbSet<Profile> Profiles { get; set; }
        public DbSet<Worker> Workers { get; set; }
        public DbSet<Player> Players { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<Activity> History { get; set; }
    }
}
