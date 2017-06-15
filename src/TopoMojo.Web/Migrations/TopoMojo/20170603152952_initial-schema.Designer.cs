using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using TopoMojo.Core;

namespace TopoMojo.Web.Migrations.TopoMojo
{
    [DbContext(typeof(TopoMojoDbContext))]
    [Migration("20170603152952_initial-schema")]
    partial class initialschema
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
            modelBuilder
                .HasAnnotation("ProductVersion", "1.1.2");

            modelBuilder.Entity("TopoMojo.Core.Instance", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("GlobalId");

                    b.Property<string>("Name");

                    b.Property<int>("TopologyId");

                    b.Property<DateTime>("WhenCreated");

                    b.HasKey("Id");

                    b.HasIndex("TopologyId");

                    b.ToTable("Instances");
                });

            modelBuilder.Entity("TopoMojo.Core.InstanceMember", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("InstanceId");

                    b.Property<int>("PersonId");

                    b.Property<bool>("isAdmin");

                    b.HasKey("Id");

                    b.HasIndex("InstanceId");

                    b.HasIndex("PersonId");

                    b.ToTable("InstanceMembers");
                });

            modelBuilder.Entity("TopoMojo.Core.Permission", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("PersonId");

                    b.Property<int>("TopologyId");

                    b.Property<int>("Value");

                    b.HasKey("Id");

                    b.HasIndex("PersonId");

                    b.HasIndex("TopologyId");

                    b.ToTable("Permissions");
                });

            modelBuilder.Entity("TopoMojo.Core.Person", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("GlobalId");

                    b.Property<bool>("IsAdmin");

                    b.Property<string>("Name");

                    b.Property<DateTime>("WhenCreated");

                    b.HasKey("Id");

                    b.ToTable("People");
                });

            modelBuilder.Entity("TopoMojo.Core.Template", b =>
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

            modelBuilder.Entity("TopoMojo.Core.TemplateReference", b =>
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

                    b.ToTable("TTLinkage");
                });

            modelBuilder.Entity("TopoMojo.Core.Topology", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Description");

                    b.Property<string>("DocumentUrl");

                    b.Property<string>("GlobalId");

                    b.Property<string>("Name");

                    b.Property<DateTime>("WhenCreated");

                    b.HasKey("Id");

                    b.ToTable("Topologies");
                });

            modelBuilder.Entity("TopoMojo.Core.Instance", b =>
                {
                    b.HasOne("TopoMojo.Core.Topology", "Topology")
                        .WithMany()
                        .HasForeignKey("TopologyId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("TopoMojo.Core.InstanceMember", b =>
                {
                    b.HasOne("TopoMojo.Core.Instance", "Instance")
                        .WithMany("Members")
                        .HasForeignKey("InstanceId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("TopoMojo.Core.Person", "Person")
                        .WithMany()
                        .HasForeignKey("PersonId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("TopoMojo.Core.Permission", b =>
                {
                    b.HasOne("TopoMojo.Core.Person", "Person")
                        .WithMany("Permissions")
                        .HasForeignKey("PersonId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("TopoMojo.Core.Topology", "Topology")
                        .WithMany("Permissions")
                        .HasForeignKey("TopologyId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("TopoMojo.Core.TemplateReference", b =>
                {
                    b.HasOne("TopoMojo.Core.Template", "Template")
                        .WithMany()
                        .HasForeignKey("TemplateId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("TopoMojo.Core.Topology", "Topology")
                        .WithMany("Templates")
                        .HasForeignKey("TopologyId")
                        .OnDelete(DeleteBehavior.Cascade);
                });
        }
    }
}
