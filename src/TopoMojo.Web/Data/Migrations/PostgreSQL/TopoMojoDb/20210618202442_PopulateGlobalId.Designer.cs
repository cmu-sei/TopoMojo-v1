﻿// <auto-generated />
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
    [Migration("20210618202442_PopulateGlobalId")]
    partial class PopulateGlobalId
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .UseIdentityByDefaultColumns()
                .HasAnnotation("Relational:MaxIdentifierLength", 63)
                .HasAnnotation("ProductVersion", "5.0.0");

            modelBuilder.Entity("TopoMojo.Data.ApiKey", b =>
                {
                    b.Property<string>("Id")
                        .HasMaxLength(36)
                        .HasColumnType("character(36)")
                        .IsFixedLength(true);

                    b.Property<string>("UserId")
                        .HasColumnType("character(36)");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("ApiKey");
                });

            modelBuilder.Entity("TopoMojo.Data.Gamespace", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .UseIdentityByDefaultColumn();

                    b.Property<bool>("AllowReset")
                        .HasColumnType("boolean");

                    b.Property<string>("Challenge")
                        .HasColumnType("text");

                    b.Property<string>("ClientId")
                        .HasColumnType("text");

                    b.Property<DateTime>("ExpirationTime")
                        .HasColumnType("timestamp without time zone");

                    b.Property<string>("GlobalId")
                        .IsRequired()
                        .HasMaxLength(36)
                        .HasColumnType("character(36)")
                        .IsFixedLength(true);

                    b.Property<string>("Name")
                        .HasMaxLength(64)
                        .HasColumnType("character varying(64)");

                    b.Property<string>("ShareCode")
                        .HasColumnType("text");

                    b.Property<DateTime>("StartTime")
                        .HasColumnType("timestamp without time zone");

                    b.Property<DateTime>("StopTime")
                        .HasColumnType("timestamp without time zone");

                    b.Property<DateTime>("WhenCreated")
                        .HasColumnType("timestamp without time zone");

                    b.Property<string>("WorkspaceGlobalId")
                        .HasColumnType("text");

                    b.Property<int>("WorkspaceId")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasAlternateKey("GlobalId");

                    b.HasIndex("WorkspaceId");

                    b.ToTable("Gamespaces");
                });

            modelBuilder.Entity("TopoMojo.Data.Player", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .UseIdentityByDefaultColumn();

                    b.Property<string>("GamespaceGlobalId")
                        .HasMaxLength(36)
                        .HasColumnType("character(36)")
                        .IsFixedLength(true);

                    b.Property<int?>("GamespaceId")
                        .HasColumnType("integer");

                    b.Property<int>("Permission")
                        .HasColumnType("integer");

                    b.Property<string>("SubjectId")
                        .HasColumnType("text");

                    b.Property<string>("SubjectName")
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("GamespaceId");

                    b.ToTable("Players");
                });

            modelBuilder.Entity("TopoMojo.Data.Template", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .UseIdentityByDefaultColumn();

                    b.Property<string>("Description")
                        .HasMaxLength(255)
                        .HasColumnType("character varying(255)");

                    b.Property<string>("Detail")
                        .HasMaxLength(4096)
                        .HasColumnType("character varying(4096)");

                    b.Property<string>("GlobalId")
                        .IsRequired()
                        .HasMaxLength(36)
                        .HasColumnType("character(36)")
                        .IsFixedLength(true);

                    b.Property<string>("Guestinfo")
                        .HasMaxLength(1024)
                        .HasColumnType("character varying(1024)");

                    b.Property<bool>("IsHidden")
                        .HasColumnType("boolean");

                    b.Property<bool>("IsPublished")
                        .HasColumnType("boolean");

                    b.Property<string>("Iso")
                        .HasColumnType("text");

                    b.Property<string>("Name")
                        .HasMaxLength(64)
                        .HasColumnType("character varying(64)");

                    b.Property<string>("Networks")
                        .HasMaxLength(64)
                        .HasColumnType("character varying(64)");

                    b.Property<string>("ParentGlobalId")
                        .HasMaxLength(36)
                        .HasColumnType("character(36)")
                        .IsFixedLength(true);

                    b.Property<int?>("ParentId")
                        .HasColumnType("integer");

                    b.Property<int>("Replicas")
                        .HasColumnType("integer");

                    b.Property<DateTime>("WhenCreated")
                        .HasColumnType("timestamp without time zone");

                    b.Property<string>("WorkspaceGlobalId")
                        .HasMaxLength(36)
                        .HasColumnType("character(36)")
                        .IsFixedLength(true);

                    b.Property<int?>("WorkspaceId")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasAlternateKey("GlobalId");

                    b.HasIndex("ParentId");

                    b.HasIndex("WorkspaceId");

                    b.ToTable("Templates");
                });

            modelBuilder.Entity("TopoMojo.Data.User", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .UseIdentityByDefaultColumn();

                    b.Property<string>("CallbackUrl")
                        .HasColumnType("text");

                    b.Property<int>("GamespaceLimit")
                        .HasColumnType("integer");

                    b.Property<int>("GamespaceMaxMinutes")
                        .HasColumnType("integer");

                    b.Property<string>("GlobalId")
                        .IsRequired()
                        .HasMaxLength(36)
                        .HasColumnType("character(36)")
                        .IsFixedLength(true);

                    b.Property<string>("Name")
                        .HasMaxLength(64)
                        .HasColumnType("character varying(64)");

                    b.Property<int>("Role")
                        .HasColumnType("integer");

                    b.Property<string>("Scope")
                        .HasColumnType("text");

                    b.Property<int>("SessionLimit")
                        .HasColumnType("integer");

                    b.Property<DateTime>("WhenCreated")
                        .HasColumnType("timestamp without time zone");

                    b.Property<int>("WorkspaceLimit")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("TopoMojo.Data.Worker", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .UseIdentityByDefaultColumn();

                    b.Property<int>("Permission")
                        .HasColumnType("integer");

                    b.Property<string>("SubjectId")
                        .HasColumnType("text");

                    b.Property<string>("SubjectName")
                        .HasColumnType("text");

                    b.Property<string>("WorkspaceGlobalId")
                        .HasMaxLength(36)
                        .HasColumnType("character(36)")
                        .IsFixedLength(true);

                    b.Property<int?>("WorkspaceId")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("WorkspaceId");

                    b.ToTable("Workers");
                });

            modelBuilder.Entity("TopoMojo.Data.Workspace", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .UseIdentityByDefaultColumn();

                    b.Property<string>("Audience")
                        .HasMaxLength(64)
                        .HasColumnType("character varying(64)");

                    b.Property<string>("Author")
                        .HasMaxLength(64)
                        .HasColumnType("character varying(64)");

                    b.Property<string>("Challenge")
                        .HasColumnType("text");

                    b.Property<string>("Description")
                        .HasMaxLength(255)
                        .HasColumnType("character varying(255)");

                    b.Property<string>("DocumentUrl")
                        .HasMaxLength(128)
                        .HasColumnType("character varying(128)");

                    b.Property<string>("GlobalId")
                        .IsRequired()
                        .HasMaxLength(36)
                        .HasColumnType("character(36)")
                        .IsFixedLength(true);

                    b.Property<bool>("IsPublished")
                        .HasColumnType("boolean");

                    b.Property<DateTime>("LastActivity")
                        .HasColumnType("timestamp without time zone");

                    b.Property<int>("LaunchCount")
                        .HasColumnType("integer");

                    b.Property<string>("Name")
                        .HasMaxLength(64)
                        .HasColumnType("character varying(64)");

                    b.Property<string>("ShareCode")
                        .HasColumnType("text");

                    b.Property<int>("TemplateLimit")
                        .HasColumnType("integer");

                    b.Property<bool>("UseUplinkSwitch")
                        .HasColumnType("boolean");

                    b.Property<DateTime>("WhenCreated")
                        .HasColumnType("timestamp without time zone");

                    b.HasKey("Id");

                    b.HasAlternateKey("GlobalId");

                    b.ToTable("Workspaces");
                });

            modelBuilder.Entity("TopoMojo.Data.ApiKey", b =>
                {
                    b.HasOne("TopoMojo.Data.User", "User")
                        .WithMany("ApiKeys")
                        .HasForeignKey("UserId")
                        .HasPrincipalKey("GlobalId");

                    b.Navigation("User");
                });

            modelBuilder.Entity("TopoMojo.Data.Gamespace", b =>
                {
                    b.HasOne("TopoMojo.Data.Workspace", "Workspace")
                        .WithMany("Gamespaces")
                        .HasForeignKey("WorkspaceId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Workspace");
                });

            modelBuilder.Entity("TopoMojo.Data.Player", b =>
                {
                    b.HasOne("TopoMojo.Data.Gamespace", "Gamespace")
                        .WithMany("Players")
                        .HasForeignKey("GamespaceId");

                    b.Navigation("Gamespace");
                });

            modelBuilder.Entity("TopoMojo.Data.Template", b =>
                {
                    b.HasOne("TopoMojo.Data.Template", "Parent")
                        .WithMany("Children")
                        .HasForeignKey("ParentId");

                    b.HasOne("TopoMojo.Data.Workspace", "Workspace")
                        .WithMany("Templates")
                        .HasForeignKey("WorkspaceId");

                    b.Navigation("Parent");

                    b.Navigation("Workspace");
                });

            modelBuilder.Entity("TopoMojo.Data.Worker", b =>
                {
                    b.HasOne("TopoMojo.Data.Workspace", "Workspace")
                        .WithMany("Workers")
                        .HasForeignKey("WorkspaceId");

                    b.Navigation("Workspace");
                });

            modelBuilder.Entity("TopoMojo.Data.Gamespace", b =>
                {
                    b.Navigation("Players");
                });

            modelBuilder.Entity("TopoMojo.Data.Template", b =>
                {
                    b.Navigation("Children");
                });

            modelBuilder.Entity("TopoMojo.Data.User", b =>
                {
                    b.Navigation("ApiKeys");
                });

            modelBuilder.Entity("TopoMojo.Data.Workspace", b =>
                {
                    b.Navigation("Gamespaces");

                    b.Navigation("Templates");

                    b.Navigation("Workers");
                });
#pragma warning restore 612, 618
        }
    }
}
