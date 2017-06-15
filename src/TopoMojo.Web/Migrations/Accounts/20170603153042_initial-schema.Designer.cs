using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Step.Accounts;

namespace TopoMojo.Web.Migrations.Accounts
{
    [DbContext(typeof(AccountDbContext))]
    [Migration("20170603153042_initial-schema")]
    partial class initialschema
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
            modelBuilder
                .HasAnnotation("ProductVersion", "1.1.2");

            modelBuilder.Entity("Step.Accounts.Account", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("AuthenticationFailures");

                    b.Property<string>("GlobalId");

                    b.Property<int>("LockedMinutes");

                    b.Property<long>("Role");

                    b.Property<int>("Status");

                    b.Property<DateTime>("WhenAuthenticated");

                    b.Property<DateTime>("WhenCreated");

                    b.Property<DateTime>("WhenLastAuthenticated");

                    b.Property<DateTime>("WhenLocked");

                    b.Property<string>("WhereAuthenticated");

                    b.Property<string>("WhereLastAuthenticated");

                    b.HasKey("Id");

                    b.HasIndex("GlobalId");

                    b.ToTable("Accounts");
                });

            modelBuilder.Entity("Step.Accounts.AccountCode", b =>
                {
                    b.Property<string>("Hash")
                        .ValueGeneratedOnAdd()
                        .HasMaxLength(40);

                    b.Property<int>("Code");

                    b.Property<DateTime>("WhenCreated");

                    b.HasKey("Hash");

                    b.ToTable("AccountCodes");
                });

            modelBuilder.Entity("Step.Accounts.AccountToken", b =>
                {
                    b.Property<string>("Hash")
                        .ValueGeneratedOnAdd()
                        .HasMaxLength(40);

                    b.Property<int>("Type");

                    b.Property<int>("UserId");

                    b.Property<DateTime>("WhenCreated");

                    b.HasKey("Hash");

                    b.HasIndex("UserId");

                    b.ToTable("AccountTokens");
                });

            modelBuilder.Entity("Step.Accounts.ClientAccount", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("ClientId");

                    b.Property<string>("UserGlobalId");

                    b.HasKey("Id");

                    b.ToTable("ClientAccounts");
                });

            modelBuilder.Entity("Step.Accounts.AccountToken", b =>
                {
                    b.HasOne("Step.Accounts.Account", "User")
                        .WithMany("Tokens")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });
        }
    }
}
