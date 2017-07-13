using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using TopoMojo.Core.Data;
using TopoMojo.Core.Entities;

namespace TopoMojo.Web.Data.Migrations.TopoMojo
{
    [DbContext(typeof(TopoMojoDbContext))]
    [Migration("20170706022112_topomojo-schema")]
    partial class topomojoschema
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
            modelBuilder
                .HasAnnotation("ProductVersion", "1.1.2");

            modelBuilder.Entity("TopoMojo.Core.Entities.Gamespace", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("GlobalId");

                    b.Property<string>("Name");

                    b.Property<string>("ShareCode");

                    b.Property<int>("TopologyId");

                    b.Property<DateTime>("WhenCreated");

                    b.HasKey("Id");

                    b.HasIndex("TopologyId");

                    b.ToTable("Gamespaces");
                });

            modelBuilder.Entity("TopoMojo.Core.Entities.Linker", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Description");

                    b.Property<string>("Iso");

                    b.Property<string>("Name");

                    b.Property<string>("Networks");

                    b.Property<int>("TemplateId");

                    b.Property<int>("TopologyId");

                    b.HasKey("Id");

                    b.HasIndex("TemplateId");

                    b.HasIndex("TopologyId");

                    b.ToTable("Linkers");
                });

            modelBuilder.Entity("TopoMojo.Core.Entities.Player", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("GamespaceId");

                    b.Property<int>("Permission");

                    b.Property<int>("PersonId");

                    b.HasKey("Id");

                    b.HasIndex("GamespaceId");

                    b.HasIndex("PersonId");

                    b.ToTable("Players");
                });

            modelBuilder.Entity("TopoMojo.Core.Entities.Profile", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("GlobalId");

                    b.Property<bool>("IsAdmin");

                    b.Property<string>("Name");

                    b.Property<DateTime>("WhenCreated");

                    b.HasKey("Id");

                    b.ToTable("Profiles");
                });

            modelBuilder.Entity("TopoMojo.Core.Entities.Template", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Description");

                    b.Property<string>("Detail");

                    b.Property<string>("GlobalId");

                    b.Property<bool>("IsPublished");

                    b.Property<string>("Name");

                    b.Property<int>("OwnerId");

                    b.Property<DateTime>("WhenCreated");

                    b.HasKey("Id");

                    b.ToTable("Templates");
                });

            modelBuilder.Entity("TopoMojo.Core.Entities.Topology", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Description");

                    b.Property<string>("DocumentUrl");

                    b.Property<string>("GlobalId");

                    b.Property<bool>("IsPublished");

                    b.Property<string>("Name");

                    b.Property<string>("ShareCode");

                    b.Property<DateTime>("WhenCreated");

                    b.HasKey("Id");

                    b.ToTable("Topologies");
                });

            modelBuilder.Entity("TopoMojo.Core.Entities.Worker", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("Permission");

                    b.Property<int>("PersonId");

                    b.Property<int>("TopologyId");

                    b.HasKey("Id");

                    b.HasIndex("PersonId");

                    b.HasIndex("TopologyId");

                    b.ToTable("Workers");
                });

            modelBuilder.Entity("TopoMojo.Core.Entities.Gamespace", b =>
                {
                    b.HasOne("TopoMojo.Core.Entities.Topology", "Topology")
                        .WithMany("Gamespaces")
                        .HasForeignKey("TopologyId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("TopoMojo.Core.Entities.Linker", b =>
                {
                    b.HasOne("TopoMojo.Core.Entities.Template", "Template")
                        .WithMany("Linkers")
                        .HasForeignKey("TemplateId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("TopoMojo.Core.Entities.Topology", "Topology")
                        .WithMany("Linkers")
                        .HasForeignKey("TopologyId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("TopoMojo.Core.Entities.Player", b =>
                {
                    b.HasOne("TopoMojo.Core.Entities.Gamespace", "Gamespace")
                        .WithMany("Players")
                        .HasForeignKey("GamespaceId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("TopoMojo.Core.Entities.Profile", "Person")
                        .WithMany("Gamespaces")
                        .HasForeignKey("PersonId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("TopoMojo.Core.Entities.Worker", b =>
                {
                    b.HasOne("TopoMojo.Core.Entities.Profile", "Person")
                        .WithMany("Workspaces")
                        .HasForeignKey("PersonId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("TopoMojo.Core.Entities.Topology", "Topology")
                        .WithMany("Workers")
                        .HasForeignKey("TopologyId")
                        .OnDelete(DeleteBehavior.Cascade);
                });
        }
    }
}
