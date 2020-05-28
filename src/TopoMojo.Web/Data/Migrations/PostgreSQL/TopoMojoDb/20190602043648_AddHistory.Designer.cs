// Copyright 2019 Carnegie Mellon University. All Rights Reserved.
// Released under a 3 Clause BSD-style license. See LICENSE.md in the project root for license information.

// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using TopoMojo.Data;

namespace TopoMojo.Web.Data.Migrations.PostgreSQL.TopoMojoDb
{
    [DbContext(typeof(TopoMojoDbContextPostgreSQL))]
    [Migration("20190602043648_AddHistory")]
    partial class AddHistory
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn)
                .HasAnnotation("ProductVersion", "2.2.4-servicing-10062")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            modelBuilder.Entity("TopoMojo.Data.Activity", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("Action");

                    b.Property<string>("Actor");

                    b.Property<int>("ActorId");

                    b.Property<string>("Annotation");

                    b.Property<string>("Asset");

                    b.Property<int>("AssetId");

                    b.Property<DateTime>("At");

                    b.HasKey("Id");

                    b.ToTable("History");
                });

            modelBuilder.Entity("TopoMojo.Data.Gamespace", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("GlobalId")
                        .IsRequired();

                    b.Property<string>("Name");

                    b.Property<string>("ShareCode");

                    b.Property<int>("TopologyId");

                    b.Property<DateTime>("WhenCreated");

                    b.HasKey("Id");

                    b.HasAlternateKey("GlobalId");

                    b.HasIndex("TopologyId");

                    b.ToTable("Gamespaces");
                });

            modelBuilder.Entity("TopoMojo.Data.Message", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("AuthorId");

                    b.Property<string>("AuthorName");

                    b.Property<bool>("Edited");

                    b.Property<string>("RoomId");

                    b.Property<string>("Text");

                    b.Property<DateTime>("WhenCreated");

                    b.HasKey("Id");

                    b.HasIndex("RoomId");

                    b.ToTable("Messages");
                });

            modelBuilder.Entity("TopoMojo.Data.Player", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("GamespaceId");

                    b.Property<DateTime?>("LastSeen");

                    b.Property<int>("Permission");

                    b.Property<int>("PersonId");

                    b.HasKey("Id");

                    b.HasIndex("GamespaceId");

                    b.HasIndex("PersonId");

                    b.ToTable("Players");
                });

            modelBuilder.Entity("TopoMojo.Data.Profile", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("GlobalId")
                        .IsRequired();

                    b.Property<bool>("IsAdmin");

                    b.Property<string>("Name");

                    b.Property<int>("Role");

                    b.Property<DateTime>("WhenCreated");

                    b.Property<int>("WorkspaceLimit");

                    b.HasKey("Id");

                    b.HasAlternateKey("GlobalId");

                    b.ToTable("Profiles");
                });

            modelBuilder.Entity("TopoMojo.Data.Template", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Description");

                    b.Property<string>("Detail");

                    b.Property<string>("GlobalId")
                        .IsRequired();

                    b.Property<bool>("IsHidden");

                    b.Property<bool>("IsPublished");

                    b.Property<string>("Iso");

                    b.Property<string>("Name");

                    b.Property<string>("Networks");

                    b.Property<int?>("ParentId");

                    b.Property<int?>("TopologyId");

                    b.Property<DateTime>("WhenCreated");

                    b.HasKey("Id");

                    b.HasAlternateKey("GlobalId");

                    b.HasIndex("ParentId");

                    b.HasIndex("TopologyId");

                    b.ToTable("Templates");
                });

            modelBuilder.Entity("TopoMojo.Data.Topology", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Author");

                    b.Property<string>("Description");

                    b.Property<string>("DocumentUrl");

                    b.Property<string>("GlobalId")
                        .IsRequired();

                    b.Property<bool>("IsEvergreen");

                    b.Property<bool>("IsLocked");

                    b.Property<bool>("IsPublished");

                    b.Property<DateTime>("LastLaunch");

                    b.Property<int>("LaunchCount");

                    b.Property<string>("Name");

                    b.Property<string>("ShareCode");

                    b.Property<int>("TemplateLimit");

                    b.Property<bool>("UseUplinkSwitch");

                    b.Property<DateTime>("WhenCreated");

                    b.Property<DateTime?>("WhenPublished");

                    b.HasKey("Id");

                    b.HasAlternateKey("GlobalId");

                    b.ToTable("Topologies");
                });

            modelBuilder.Entity("TopoMojo.Data.Worker", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTime?>("LastSeen");

                    b.Property<int>("Permission");

                    b.Property<int>("PersonId");

                    b.Property<int>("TopologyId");

                    b.HasKey("Id");

                    b.HasIndex("PersonId");

                    b.HasIndex("TopologyId");

                    b.ToTable("Workers");
                });

            modelBuilder.Entity("TopoMojo.Data.Gamespace", b =>
                {
                    b.HasOne("TopoMojo.Data.Topology", "Topology")
                        .WithMany("Gamespaces")
                        .HasForeignKey("TopologyId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("TopoMojo.Data.Player", b =>
                {
                    b.HasOne("TopoMojo.Data.Gamespace", "Gamespace")
                        .WithMany("Players")
                        .HasForeignKey("GamespaceId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("TopoMojo.Data.Profile", "Person")
                        .WithMany("Gamespaces")
                        .HasForeignKey("PersonId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("TopoMojo.Data.Template", b =>
                {
                    b.HasOne("TopoMojo.Data.Template", "Parent")
                        .WithMany()
                        .HasForeignKey("ParentId");

                    b.HasOne("TopoMojo.Data.Topology", "Topology")
                        .WithMany("Templates")
                        .HasForeignKey("TopologyId");
                });

            modelBuilder.Entity("TopoMojo.Data.Worker", b =>
                {
                    b.HasOne("TopoMojo.Data.Profile", "Person")
                        .WithMany("Workspaces")
                        .HasForeignKey("PersonId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("TopoMojo.Data.Topology", "Topology")
                        .WithMany("Workers")
                        .HasForeignKey("TopologyId")
                        .OnDelete(DeleteBehavior.Cascade);
                });
#pragma warning restore 612, 618
        }
    }
}
