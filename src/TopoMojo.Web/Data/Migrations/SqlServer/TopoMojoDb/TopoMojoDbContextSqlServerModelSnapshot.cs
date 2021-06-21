﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using TopoMojo.Data;

namespace TopoMojo.Web.Data.Migrations.SqlServer.TopoMojoDb
{
    [DbContext(typeof(TopoMojoDbContextSqlServer))]
    partial class TopoMojoDbContextSqlServerModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .UseIdentityColumns()
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("ProductVersion", "5.0.0");

            modelBuilder.Entity("TopoMojo.Data.ApiKey", b =>
                {
                    b.Property<string>("Id")
                        .HasMaxLength(36)
                        .HasColumnType("nchar(36)")
                        .IsFixedLength(true);

                    b.Property<string>("Hash")
                        .HasMaxLength(64)
                        .HasColumnType("nchar(64)")
                        .IsFixedLength(true);

                    b.Property<string>("Name")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("UserId")
                        .HasColumnType("nchar(36)");

                    b.Property<DateTime>("WhenCreated")
                        .HasColumnType("datetime2");

                    b.HasKey("Id");

                    b.HasIndex("Hash");

                    b.HasIndex("UserId");

                    b.ToTable("ApiKeys");
                });

            modelBuilder.Entity("TopoMojo.Data.Gamespace", b =>
                {
                    b.Property<string>("Id")
                        .HasMaxLength(36)
                        .HasColumnType("nchar(36)")
                        .IsFixedLength(true);

                    b.Property<bool>("AllowReset")
                        .HasColumnType("bit");

                    b.Property<string>("Challenge")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("CleanupGraceMinutes")
                        .HasColumnType("int");

                    b.Property<DateTime>("EndTime")
                        .HasColumnType("datetime2");

                    b.Property<DateTime>("ExpirationTime")
                        .HasColumnType("datetime2");

                    b.Property<string>("ManagerId")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Name")
                        .HasMaxLength(64)
                        .HasColumnType("nvarchar(64)");

                    b.Property<string>("ShareCode")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("StartTime")
                        .HasColumnType("datetime2");

                    b.Property<DateTime>("WhenCreated")
                        .HasColumnType("datetime2");

                    b.Property<string>("WorkspaceId")
                        .HasColumnType("nchar(36)");

                    b.HasKey("Id");

                    b.HasIndex("WorkspaceId");

                    b.ToTable("Gamespaces");
                });

            modelBuilder.Entity("TopoMojo.Data.Player", b =>
                {
                    b.Property<string>("SubjectId")
                        .HasMaxLength(36)
                        .HasColumnType("nchar(36)")
                        .IsFixedLength(true);

                    b.Property<string>("GamespaceId")
                        .HasMaxLength(36)
                        .HasColumnType("nchar(36)")
                        .IsFixedLength(true);

                    b.Property<int>("Permission")
                        .HasColumnType("int");

                    b.Property<string>("SubjectName")
                        .HasMaxLength(64)
                        .HasColumnType("nvarchar(64)");

                    b.HasKey("SubjectId", "GamespaceId");

                    b.HasIndex("GamespaceId");

                    b.ToTable("Players");
                });

            modelBuilder.Entity("TopoMojo.Data.Template", b =>
                {
                    b.Property<string>("Id")
                        .HasMaxLength(36)
                        .HasColumnType("nchar(36)")
                        .IsFixedLength(true);

                    b.Property<string>("Description")
                        .HasMaxLength(255)
                        .HasColumnType("nvarchar(255)");

                    b.Property<string>("Detail")
                        .HasMaxLength(4096)
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Guestinfo")
                        .HasMaxLength(1024)
                        .HasColumnType("nvarchar(1024)");

                    b.Property<bool>("IsHidden")
                        .HasColumnType("bit");

                    b.Property<bool>("IsPublished")
                        .HasColumnType("bit");

                    b.Property<string>("Iso")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Name")
                        .HasMaxLength(64)
                        .HasColumnType("nvarchar(64)");

                    b.Property<string>("Networks")
                        .HasMaxLength(64)
                        .HasColumnType("nvarchar(64)");

                    b.Property<string>("ParentId")
                        .HasMaxLength(36)
                        .HasColumnType("nchar(36)")
                        .IsFixedLength(true);

                    b.Property<int>("Replicas")
                        .HasColumnType("int");

                    b.Property<DateTime>("WhenCreated")
                        .HasColumnType("datetime2");

                    b.Property<string>("WorkspaceId")
                        .HasMaxLength(36)
                        .HasColumnType("nchar(36)")
                        .IsFixedLength(true);

                    b.HasKey("Id");

                    b.HasIndex("ParentId");

                    b.HasIndex("WorkspaceId");

                    b.ToTable("Templates");
                });

            modelBuilder.Entity("TopoMojo.Data.User", b =>
                {
                    b.Property<string>("Id")
                        .HasMaxLength(36)
                        .HasColumnType("nchar(36)")
                        .IsFixedLength(true);

                    b.Property<int>("GamespaceCleanupGraceMinutes")
                        .HasColumnType("int");

                    b.Property<int>("GamespaceLimit")
                        .HasColumnType("int");

                    b.Property<int>("GamespaceMaxMinutes")
                        .HasColumnType("int");

                    b.Property<string>("Name")
                        .HasMaxLength(64)
                        .HasColumnType("nvarchar(64)");

                    b.Property<int>("Role")
                        .HasColumnType("int");

                    b.Property<string>("Scope")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("WhenCreated")
                        .HasColumnType("datetime2");

                    b.Property<int>("WorkspaceLimit")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("TopoMojo.Data.Worker", b =>
                {
                    b.Property<string>("SubjectId")
                        .HasMaxLength(36)
                        .HasColumnType("nchar(36)")
                        .IsFixedLength(true);

                    b.Property<string>("WorkspaceId")
                        .HasMaxLength(36)
                        .HasColumnType("nchar(36)")
                        .IsFixedLength(true);

                    b.Property<int>("Permission")
                        .HasColumnType("int");

                    b.Property<string>("SubjectName")
                        .HasMaxLength(64)
                        .HasColumnType("nvarchar(64)");

                    b.HasKey("SubjectId", "WorkspaceId");

                    b.HasIndex("WorkspaceId");

                    b.ToTable("Workers");
                });

            modelBuilder.Entity("TopoMojo.Data.Workspace", b =>
                {
                    b.Property<string>("Id")
                        .HasMaxLength(36)
                        .HasColumnType("nchar(36)")
                        .IsFixedLength(true);

                    b.Property<string>("Audience")
                        .HasMaxLength(64)
                        .HasColumnType("nvarchar(64)");

                    b.Property<string>("Author")
                        .HasMaxLength(64)
                        .HasColumnType("nvarchar(64)");

                    b.Property<string>("Challenge")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Description")
                        .HasMaxLength(255)
                        .HasColumnType("nvarchar(255)");

                    b.Property<bool>("HostAffinity")
                        .HasColumnType("bit");

                    b.Property<bool>("IsPublished")
                        .HasColumnType("bit");

                    b.Property<DateTime>("LastActivity")
                        .HasColumnType("datetime2");

                    b.Property<int>("LaunchCount")
                        .HasColumnType("int");

                    b.Property<string>("Name")
                        .HasMaxLength(64)
                        .HasColumnType("nvarchar(64)");

                    b.Property<string>("ShareCode")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("TemplateLimit")
                        .HasColumnType("int");

                    b.Property<bool>("UseUplinkSwitch")
                        .HasColumnType("bit");

                    b.Property<DateTime>("WhenCreated")
                        .HasColumnType("datetime2");

                    b.HasKey("Id");

                    b.ToTable("Workspaces");
                });

            modelBuilder.Entity("TopoMojo.Data.ApiKey", b =>
                {
                    b.HasOne("TopoMojo.Data.User", "User")
                        .WithMany("ApiKeys")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.Navigation("User");
                });

            modelBuilder.Entity("TopoMojo.Data.Gamespace", b =>
                {
                    b.HasOne("TopoMojo.Data.Workspace", "Workspace")
                        .WithMany("Gamespaces")
                        .HasForeignKey("WorkspaceId")
                        .OnDelete(DeleteBehavior.SetNull);

                    b.Navigation("Workspace");
                });

            modelBuilder.Entity("TopoMojo.Data.Player", b =>
                {
                    b.HasOne("TopoMojo.Data.Gamespace", "Gamespace")
                        .WithMany("Players")
                        .HasForeignKey("GamespaceId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

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
                        .HasForeignKey("WorkspaceId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

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
