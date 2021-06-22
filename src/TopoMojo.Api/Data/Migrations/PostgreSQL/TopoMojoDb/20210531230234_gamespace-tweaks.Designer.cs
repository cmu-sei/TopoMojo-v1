﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using TopoMojo.Api.Data;

namespace TopoMojo.Api.Data.Migrations.PostgreSQL.TopoMojoDb
{
    [DbContext(typeof(TopoMojoDbContextPostgreSQL))]
    [Migration("20210531230234_gamespace-tweaks")]
    partial class gamespacetweaks
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                .HasAnnotation("ProductVersion", "3.1.4")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            modelBuilder.Entity("TopoMojo.Api.Data.Activity", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<int>("Action")
                        .HasColumnType("integer");

                    b.Property<string>("Actor")
                        .HasColumnType("text");

                    b.Property<int>("ActorId")
                        .HasColumnType("integer");

                    b.Property<string>("Annotation")
                        .HasColumnType("text");

                    b.Property<string>("Asset")
                        .HasColumnType("text");

                    b.Property<int>("AssetId")
                        .HasColumnType("integer");

                    b.Property<DateTime>("At")
                        .HasColumnType("timestamp without time zone");

                    b.HasKey("Id");

                    b.ToTable("History");
                });

            modelBuilder.Entity("TopoMojo.Api.Data.Gamespace", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

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
                        .HasColumnType("character(36)")
                        .IsFixedLength(true)
                        .HasMaxLength(36);

                    b.Property<string>("Name")
                        .HasColumnType("character varying(64)")
                        .HasMaxLength(64);

                    b.Property<string>("ShareCode")
                        .HasColumnType("text");

                    b.Property<DateTime>("StartTime")
                        .HasColumnType("timestamp without time zone");

                    b.Property<DateTime>("StopTime")
                        .HasColumnType("timestamp without time zone");

                    b.Property<DateTime>("WhenCreated")
                        .HasColumnType("timestamp without time zone");

                    b.Property<int>("WorkspaceId")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasAlternateKey("GlobalId");

                    b.HasIndex("WorkspaceId");

                    b.ToTable("Gamespaces");
                });

            modelBuilder.Entity("TopoMojo.Api.Data.Message", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<int>("AuthorId")
                        .HasColumnType("integer");

                    b.Property<string>("AuthorName")
                        .HasColumnType("character varying(64)")
                        .HasMaxLength(64);

                    b.Property<bool>("Edited")
                        .HasColumnType("boolean");

                    b.Property<string>("RoomId")
                        .HasColumnType("character(36)")
                        .IsFixedLength(true)
                        .HasMaxLength(36);

                    b.Property<string>("Text")
                        .HasColumnType("character varying(2048)")
                        .HasMaxLength(2048);

                    b.Property<DateTime>("WhenCreated")
                        .HasColumnType("timestamp without time zone");

                    b.HasKey("Id");

                    b.HasIndex("RoomId");

                    b.ToTable("Messages");
                });

            modelBuilder.Entity("TopoMojo.Api.Data.Player", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<int>("GamespaceId")
                        .HasColumnType("integer");

                    b.Property<int>("Permission")
                        .HasColumnType("integer");

                    b.Property<string>("SubjectId")
                        .HasColumnType("text");

                    b.Property<string>("SubjectName")
                        .HasColumnType("text");

                    b.Property<int?>("UserId")
                        .HasColumnType("integer");

                    b.Property<string>("WorkspaceId")
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("GamespaceId");

                    b.HasIndex("UserId");

                    b.HasIndex("SubjectId", "WorkspaceId");

                    b.ToTable("Players");
                });

            modelBuilder.Entity("TopoMojo.Api.Data.Template", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<string>("Description")
                        .HasColumnType("character varying(255)")
                        .HasMaxLength(255);

                    b.Property<string>("Detail")
                        .HasColumnType("character varying(4096)")
                        .HasMaxLength(4096);

                    b.Property<string>("GlobalId")
                        .IsRequired()
                        .HasColumnType("character(36)")
                        .IsFixedLength(true)
                        .HasMaxLength(36);

                    b.Property<string>("Guestinfo")
                        .HasColumnType("character varying(1024)")
                        .HasMaxLength(1024);

                    b.Property<bool>("IsHidden")
                        .HasColumnType("boolean");

                    b.Property<bool>("IsPublished")
                        .HasColumnType("boolean");

                    b.Property<string>("Iso")
                        .HasColumnType("text");

                    b.Property<string>("Name")
                        .HasColumnType("character varying(64)")
                        .HasMaxLength(64);

                    b.Property<string>("Networks")
                        .HasColumnType("character varying(64)")
                        .HasMaxLength(64);

                    b.Property<int?>("ParentId")
                        .HasColumnType("integer");

                    b.Property<int>("Replicas")
                        .HasColumnType("integer");

                    b.Property<DateTime>("WhenCreated")
                        .HasColumnType("timestamp without time zone");

                    b.Property<int?>("WorkspaceId")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasAlternateKey("GlobalId");

                    b.HasIndex("ParentId");

                    b.HasIndex("WorkspaceId");

                    b.ToTable("Templates");
                });

            modelBuilder.Entity("TopoMojo.Api.Data.User", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<string>("GlobalId")
                        .IsRequired()
                        .HasColumnType("character(36)")
                        .IsFixedLength(true)
                        .HasMaxLength(36);

                    b.Property<string>("Name")
                        .HasColumnType("character varying(64)")
                        .HasMaxLength(64);

                    b.Property<int>("Role")
                        .HasColumnType("integer");

                    b.Property<DateTime>("WhenCreated")
                        .HasColumnType("timestamp without time zone");

                    b.Property<int>("WorkspaceLimit")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasAlternateKey("GlobalId");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("TopoMojo.Api.Data.Worker", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<int>("Permission")
                        .HasColumnType("integer");

                    b.Property<int>("PersonId")
                        .HasColumnType("integer");

                    b.Property<int>("WorkspaceId")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("PersonId");

                    b.HasIndex("WorkspaceId");

                    b.ToTable("Workers");
                });

            modelBuilder.Entity("TopoMojo.Api.Data.Workspace", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<string>("Audience")
                        .HasColumnType("character varying(64)")
                        .HasMaxLength(64);

                    b.Property<string>("Author")
                        .HasColumnType("character varying(64)")
                        .HasMaxLength(64);

                    b.Property<string>("Challenge")
                        .HasColumnType("text");

                    b.Property<string>("Description")
                        .HasColumnType("character varying(255)")
                        .HasMaxLength(255);

                    b.Property<string>("DocumentUrl")
                        .HasColumnType("character varying(128)")
                        .HasMaxLength(128);

                    b.Property<string>("GlobalId")
                        .IsRequired()
                        .HasColumnType("character(36)")
                        .IsFixedLength(true)
                        .HasMaxLength(36);

                    b.Property<bool>("IsPublished")
                        .HasColumnType("boolean");

                    b.Property<DateTime>("LastActivity")
                        .HasColumnType("timestamp without time zone");

                    b.Property<int>("LaunchCount")
                        .HasColumnType("integer");

                    b.Property<string>("Name")
                        .HasColumnType("character varying(64)")
                        .HasMaxLength(64);

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

            modelBuilder.Entity("TopoMojo.Api.Data.Gamespace", b =>
                {
                    b.HasOne("TopoMojo.Api.Data.Workspace", "Workspace")
                        .WithMany("Gamespaces")
                        .HasForeignKey("WorkspaceId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("TopoMojo.Api.Data.Player", b =>
                {
                    b.HasOne("TopoMojo.Api.Data.Gamespace", "Gamespace")
                        .WithMany("Players")
                        .HasForeignKey("GamespaceId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("TopoMojo.Api.Data.User", null)
                        .WithMany("Gamespaces")
                        .HasForeignKey("UserId");
                });

            modelBuilder.Entity("TopoMojo.Api.Data.Template", b =>
                {
                    b.HasOne("TopoMojo.Api.Data.Template", "Parent")
                        .WithMany()
                        .HasForeignKey("ParentId");

                    b.HasOne("TopoMojo.Api.Data.Workspace", "Workspace")
                        .WithMany("Templates")
                        .HasForeignKey("WorkspaceId");
                });

            modelBuilder.Entity("TopoMojo.Api.Data.Worker", b =>
                {
                    b.HasOne("TopoMojo.Api.Data.User", "Person")
                        .WithMany("Workspaces")
                        .HasForeignKey("PersonId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("TopoMojo.Api.Data.Workspace", "Workspace")
                        .WithMany("Workers")
                        .HasForeignKey("WorkspaceId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });
#pragma warning restore 612, 618
        }
    }
}