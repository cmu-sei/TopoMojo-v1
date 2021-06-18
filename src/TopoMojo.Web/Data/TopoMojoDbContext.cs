// Copyright 2020 Carnegie Mellon University. All Rights Reserved.
// Released under a 3 Clause BSD-style license. See LICENSE.md in the project root for license information.

using Microsoft.EntityFrameworkCore;

namespace TopoMojo.Data
{
    public class TopoMojoDbContext : DbContext
    {
        private const int KEYLENGTH = 36;
        public TopoMojoDbContext(DbContextOptions options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.Entity<Workspace>().HasKey(e => e.GlobalId);
            builder.Entity<Workspace>(b => {
                // b.HasAlternateKey(t => t.GlobalId);
                b.Property(w => w.GlobalId).IsFixedLength().HasMaxLength(KEYLENGTH);
                b.Property(w => w.Name).HasMaxLength(64);
                b.Property(w => w.Author).HasMaxLength(64);
                b.Property(w => w.Audience).HasMaxLength(64);
                b.Property(w => w.DocumentUrl).HasMaxLength(128);
                b.Property(w => w.Description).HasMaxLength(255);
            });

            builder.Entity<Gamespace>().HasKey(e => e.GlobalId);
            builder.Entity<Gamespace>(b => {
                // b.HasAlternateKey(t => t.GlobalId);
                b.Property(w => w.GlobalId).IsFixedLength().HasMaxLength(KEYLENGTH);
                b.Property(w => w.Name).HasMaxLength(64);
            });
            builder.Entity<Gamespace>()
                .HasOne(a => a.Workspace).WithMany(t => t.Gamespaces)
                .HasForeignKey(a => a.WorkspaceGlobalId)
                .HasPrincipalKey(u => u.GlobalId)
            ;

            builder.Entity<Template>().HasKey(e => e.GlobalId);
            builder.Entity<Template>(b => {
                // b.HasAlternateKey(t => t.GlobalId);
                b.Property(w => w.GlobalId).IsFixedLength().HasMaxLength(KEYLENGTH);
                b.Property(w => w.ParentGlobalId).IsFixedLength().HasMaxLength(KEYLENGTH);
                b.Property(g => g.WorkspaceGlobalId).IsFixedLength().HasMaxLength(KEYLENGTH);
                b.Property(w => w.Name).HasMaxLength(64);
                b.Property(w => w.Description).HasMaxLength(255);
                b.Property(w => w.Guestinfo).HasMaxLength(1024);
                b.Property(w => w.Networks).HasMaxLength(64);
                b.Property(w => w.Detail).HasMaxLength(4096);
            });
            builder.Entity<Template>()
                .HasOne(a => a.Parent).WithMany(t => t.Children)
                .HasForeignKey(a => a.ParentGlobalId)
                .HasPrincipalKey(u => u.GlobalId)
            ;
            builder.Entity<Template>()
                .HasOne(a => a.Workspace).WithMany(t => t.Templates)
                .HasForeignKey(a => a.WorkspaceGlobalId)
                .HasPrincipalKey(u => u.GlobalId)
            ;

            builder.Entity<User>().HasKey(e => e.GlobalId);
            builder.Entity<User>(b => {
                // b.HasAlternateKey(t => t.GlobalId);
                b.Property(w => w.GlobalId).IsFixedLength().HasMaxLength(KEYLENGTH);
                b.Property(w => w.Name).HasMaxLength(64);

            });

            builder.Entity<Player>(b => {
                b.Property(g => g.GamespaceGlobalId).IsFixedLength().HasMaxLength(KEYLENGTH);
            });
            builder.Entity<Player>()
                .HasOne(a => a.Gamespace)
                .WithMany(u => u.Players)
                .HasForeignKey(a => a.GamespaceGlobalId)
                .HasPrincipalKey(u => u.GlobalId)
            ;

            builder.Entity<Worker>(b => {
                b.Property(g => g.WorkspaceGlobalId).IsFixedLength().HasMaxLength(KEYLENGTH);
            });
            builder.Entity<Worker>()
                .HasOne(a => a.Workspace)
                .WithMany(u => u.Workers)
                .HasForeignKey(a => a.WorkspaceGlobalId)
                .HasPrincipalKey(u => u.GlobalId)
            ;

            builder.Entity<ApiKey>()
                .HasOne(a => a.User)
                .WithMany(u => u.ApiKeys)
                .HasForeignKey(a => a.UserId)
                .HasPrincipalKey(u => u.GlobalId)
            ;

            builder.Entity<ApiKey>(b => {
                b.Property(w => w.Id).IsFixedLength().HasMaxLength(KEYLENGTH);
            });

        }

        public DbSet<Workspace> Workspaces { get; set; }
        public DbSet<Template> Templates { get; set; }
        public DbSet<Gamespace> Gamespaces { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Worker> Workers { get; set; }
        public DbSet<Player> Players { get; set; }
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
