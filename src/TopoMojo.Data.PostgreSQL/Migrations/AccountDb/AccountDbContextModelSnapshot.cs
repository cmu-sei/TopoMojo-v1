﻿// <auto-generated />
using Jam.Accounts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using System;

namespace TopoMojo.Data.PostgreSQL.Migrations.AccountDb
{
    [DbContext(typeof(AccountDbContext))]
    partial class AccountDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn)
                .HasAnnotation("ProductVersion", "2.0.2-rtm-10011");

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
#pragma warning restore 612, 618
        }
    }
}