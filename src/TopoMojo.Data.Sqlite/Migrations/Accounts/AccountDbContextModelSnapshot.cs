using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Jam.Accounts;

namespace TopoMojo.Data.Sqlite.Migrations.Accounts
{
    [DbContext(typeof(AccountDbContext))]
    partial class AccountDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
            modelBuilder
                .HasAnnotation("ProductVersion", "1.1.2");

            modelBuilder.Entity("Jam.Accounts.Account", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("AuthenticationFailures");

                    b.Property<string>("GlobalId");

                    b.Property<int>("LockedMinutes");

                    b.Property<int>("RoleFlags");

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

            modelBuilder.Entity("Jam.Accounts.AccountCode", b =>
                {
                    b.Property<string>("Hash")
                        .ValueGeneratedOnAdd()
                        .HasMaxLength(40);

                    b.Property<int>("Code");

                    b.Property<DateTime>("WhenCreated");

                    b.HasKey("Hash");

                    b.ToTable("AccountCodes");
                });

            modelBuilder.Entity("Jam.Accounts.AccountToken", b =>
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

            modelBuilder.Entity("Jam.Accounts.AccountToken", b =>
                {
                    b.HasOne("Jam.Accounts.Account", "User")
                        .WithMany("Tokens")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });
        }
    }
}
