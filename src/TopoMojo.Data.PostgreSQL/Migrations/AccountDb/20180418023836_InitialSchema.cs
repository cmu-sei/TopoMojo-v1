using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace TopoMojo.Data.PostgreSQL.Migrations.AccountDb
{
    public partial class InitialSchema : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AccountCodes",
                columns: table => new
                {
                    Hash = table.Column<string>(maxLength: 40, nullable: false),
                    Code = table.Column<int>(nullable: false),
                    WhenCreated = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccountCodes", x => x.Hash);
                });

            migrationBuilder.CreateTable(
                name: "Accounts",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    AuthenticationFailures = table.Column<int>(nullable: false),
                    GlobalId = table.Column<string>(nullable: true),
                    LockedMinutes = table.Column<int>(nullable: false),
                    RoleFlags = table.Column<int>(nullable: false),
                    Status = table.Column<int>(nullable: false),
                    WhenAuthenticated = table.Column<DateTime>(nullable: false),
                    WhenCreated = table.Column<DateTime>(nullable: false),
                    WhenLastAuthenticated = table.Column<DateTime>(nullable: false),
                    WhenLocked = table.Column<DateTime>(nullable: false),
                    WhereAuthenticated = table.Column<string>(nullable: true),
                    WhereLastAuthenticated = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Accounts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AccountTokens",
                columns: table => new
                {
                    Hash = table.Column<string>(maxLength: 40, nullable: false),
                    Type = table.Column<int>(nullable: false),
                    UserId = table.Column<int>(nullable: false),
                    WhenCreated = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccountTokens", x => x.Hash);
                    table.ForeignKey(
                        name: "FK_AccountTokens_Accounts_UserId",
                        column: x => x.UserId,
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_GlobalId",
                table: "Accounts",
                column: "GlobalId");

            migrationBuilder.CreateIndex(
                name: "IX_AccountTokens_UserId",
                table: "AccountTokens",
                column: "UserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AccountCodes");

            migrationBuilder.DropTable(
                name: "AccountTokens");

            migrationBuilder.DropTable(
                name: "Accounts");
        }
    }
}
