// Copyright 2020 Carnegie Mellon University. All Rights Reserved.
// Released under a 3 Clause BSD-style license. See LICENSE.md in the project root for license information.

using Microsoft.EntityFrameworkCore;

namespace TopoMojo.Data
{
    public class TopoMojoDbContext : DbContext
    {
        public TopoMojoDbContext(DbContextOptions options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Workspace>(b => {
                b.HasAlternateKey(t => t.GlobalId);
                b.Property(w => w.GlobalId).IsFixedLength().HasMaxLength(36);
                b.Property(w => w.Name).HasMaxLength(64);
                b.Property(w => w.Author).HasMaxLength(64);
                b.Property(w => w.Audience).HasMaxLength(64);
                b.Property(w => w.DocumentUrl).HasMaxLength(128);
                b.Property(w => w.Description).HasMaxLength(255);
            });

            builder.Entity<Gamespace>(b => {
                b.HasAlternateKey(t => t.GlobalId);
                b.Property(w => w.GlobalId).IsFixedLength().HasMaxLength(36);
                b.Property(w => w.Name).HasMaxLength(64);
                b.Property(w => w.Audience).HasMaxLength(64);
            });

            builder.Entity<Template>(b => {
                b.HasAlternateKey(t => t.GlobalId);
                b.Property(w => w.GlobalId).IsFixedLength().HasMaxLength(36);
                b.Property(w => w.Name).HasMaxLength(64);
                b.Property(w => w.Description).HasMaxLength(255);
                b.Property(w => w.Guestinfo).HasMaxLength(255);
                b.Property(w => w.Networks).HasMaxLength(64);
                b.Property(w => w.Detail).HasMaxLength(2048);
            });

            builder.Entity<User>(b => {
                b.HasAlternateKey(t => t.GlobalId);
                b.Property(w => w.GlobalId).IsFixedLength().HasMaxLength(36);
                b.Property(w => w.Name).HasMaxLength(64);
            });

            builder.Entity<Message>(b => {
                b.HasIndex(t => t.RoomId);
                b.Property(w => w.RoomId).IsFixedLength().HasMaxLength(36);
                b.Property(w => w.AuthorName).HasMaxLength(64);
                b.Property(w => w.Text).HasMaxLength(2048);
            });
        }

        public DbSet<Workspace> Workspaces { get; set; }
        public DbSet<Template> Templates { get; set; }
        public DbSet<Gamespace> Gamespaces { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Worker> Workers { get; set; }
        public DbSet<Player> Players { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<Activity> History { get; set; }
    }

    public class TopoMojoDbContextPostgreSQL: TopoMojoDbContext
    {
        public TopoMojoDbContextPostgreSQL(DbContextOptions<TopoMojoDbContextPostgreSQL> options) : base(options) {}
    }

    public class TopoMojoDbContextSqlServer: TopoMojoDbContext
    {
        public TopoMojoDbContextSqlServer(DbContextOptions<TopoMojoDbContextSqlServer> options) : base(options) {}
    }

    public class TopoMojoDbContextInMemory: TopoMojoDbContext
    {
        public TopoMojoDbContextInMemory(DbContextOptions<TopoMojoDbContextInMemory> options) : base(options) {}
    }
}
