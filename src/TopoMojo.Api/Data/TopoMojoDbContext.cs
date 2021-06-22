// Copyright 2021 Carnegie Mellon University. All Rights Reserved.
// Released under a 3 Clause BSD-style license. See LICENSE.md in the project root for license information.

using Microsoft.EntityFrameworkCore;

namespace TopoMojo.Api.Data
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

            builder.Entity<Workspace>(b => {
                b.Property(w => w.Id).IsFixedLength().HasMaxLength(KEYLENGTH);
                b.Property(w => w.Name).HasMaxLength(64);
                b.Property(w => w.Author).HasMaxLength(64);
                b.Property(w => w.Audience).HasMaxLength(64);
                b.Property(w => w.TemplateScope).HasMaxLength(64);
                b.Property(w => w.Description).HasMaxLength(255);
            });

            builder.Entity<Gamespace>(b => {
                b.HasOne(g => g.Workspace).WithMany(w => w.Gamespaces).OnDelete(DeleteBehavior.SetNull);
                b.Property(w => w.Id).IsFixedLength().HasMaxLength(KEYLENGTH);
                b.Property(w => w.Name).HasMaxLength(64);
            });

            builder.Entity<Template>(b => {
                b.Property(w => w.Id).IsFixedLength().HasMaxLength(KEYLENGTH);
                b.Property(w => w.ParentId).IsFixedLength().HasMaxLength(KEYLENGTH);
                b.Property(g => g.WorkspaceId).IsFixedLength().HasMaxLength(KEYLENGTH);
                b.Property(w => w.Name).HasMaxLength(64);
                b.Property(w => w.Description).HasMaxLength(255);
                b.Property(w => w.Audience).HasMaxLength(64);
                b.Property(w => w.Guestinfo).HasMaxLength(1024);
                b.Property(w => w.Networks).HasMaxLength(64);
                b.Property(w => w.Detail).HasMaxLength(4096);
            });

            builder.Entity<User>(b => {
                b.Property(w => w.Id).IsFixedLength().HasMaxLength(KEYLENGTH);
                b.Property(w => w.Name).HasMaxLength(64);
            });

            builder.Entity<Player>(b => {
                b.HasKey(g => new { g.SubjectId, g.GamespaceId });
                b.Property(g => g.GamespaceId).IsFixedLength().HasMaxLength(KEYLENGTH);
                b.Property(g => g.SubjectId).IsFixedLength().HasMaxLength(KEYLENGTH);
                b.Property(g => g.SubjectName).HasMaxLength(64);
            });

            builder.Entity<Worker>(b => {
                b.HasKey(w => new { w.SubjectId, w.WorkspaceId });
                b.Property(g => g.WorkspaceId).IsFixedLength().HasMaxLength(KEYLENGTH);
                b.Property(g => g.SubjectId).IsFixedLength().HasMaxLength(KEYLENGTH);
                b.Property(w => w.SubjectName).HasMaxLength(64);
            });

            builder.Entity<ApiKey>(b => {
                b.HasOne(a => a.User).WithMany(u => u.ApiKeys).OnDelete(DeleteBehavior.Cascade);
                b.Property(w => w.Id).IsFixedLength().HasMaxLength(KEYLENGTH);
                b.Property(w => w.Hash).IsFixedLength().HasMaxLength(64);
                b.HasIndex(w => w.Hash);
            });

        }

        public DbSet<Workspace> Workspaces { get; set; }
        public DbSet<Template> Templates { get; set; }
        public DbSet<Gamespace> Gamespaces { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Worker> Workers { get; set; }
        public DbSet<Player> Players { get; set; }
        public DbSet<ApiKey> ApiKeys { get; set; }
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
