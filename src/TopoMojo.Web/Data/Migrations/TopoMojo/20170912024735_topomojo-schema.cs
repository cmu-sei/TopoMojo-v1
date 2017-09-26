using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace TopoMojo.Web.Data.Migrations.TopoMojo
{
    public partial class topomojoschema : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Profiles",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn)
                        .Annotation("Sqlite:Autoincrement", true),
                    GlobalId = table.Column<string>(nullable: true),
                    IsAdmin = table.Column<bool>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    WhenCreated = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Profiles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Topologies",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn)
                        .Annotation("Sqlite:Autoincrement", true),
                    Description = table.Column<string>(nullable: true),
                    DocumentUrl = table.Column<string>(nullable: true),
                    GlobalId = table.Column<string>(nullable: true),
                    IsPublished = table.Column<bool>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    ShareCode = table.Column<string>(nullable: true),
                    WhenCreated = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Topologies", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Gamespaces",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn)
                        .Annotation("Sqlite:Autoincrement", true),
                    GlobalId = table.Column<string>(nullable: true),
                    Name = table.Column<string>(nullable: true),
                    ShareCode = table.Column<string>(nullable: true),
                    TopologyId = table.Column<int>(nullable: false),
                    WhenCreated = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Gamespaces", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Gamespaces_Topologies_TopologyId",
                        column: x => x.TopologyId,
                        principalTable: "Topologies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Templates",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn)
                        .Annotation("Sqlite:Autoincrement", true),
                    Description = table.Column<string>(nullable: true),
                    Detail = table.Column<string>(nullable: true),
                    GlobalId = table.Column<string>(nullable: true),
                    IsHidden = table.Column<bool>(nullable: false),
                    IsPublished = table.Column<bool>(nullable: false),
                    Iso = table.Column<string>(nullable: true),
                    Name = table.Column<string>(nullable: true),
                    Networks = table.Column<string>(nullable: true),
                    ParentId = table.Column<int>(nullable: true),
                    TopologyId = table.Column<int>(nullable: true),
                    WhenCreated = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Templates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Templates_Templates_ParentId",
                        column: x => x.ParentId,
                        principalTable: "Templates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Templates_Topologies_TopologyId",
                        column: x => x.TopologyId,
                        principalTable: "Topologies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Workers",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn)
                        .Annotation("Sqlite:Autoincrement", true),
                    Permission = table.Column<int>(nullable: false),
                    PersonId = table.Column<int>(nullable: false),
                    TopologyId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Workers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Workers_Profiles_PersonId",
                        column: x => x.PersonId,
                        principalTable: "Profiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Workers_Topologies_TopologyId",
                        column: x => x.TopologyId,
                        principalTable: "Topologies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Players",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn)
                        .Annotation("Sqlite:Autoincrement", true),
                    GamespaceId = table.Column<int>(nullable: false),
                    Permission = table.Column<int>(nullable: false),
                    PersonId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Players", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Players_Gamespaces_GamespaceId",
                        column: x => x.GamespaceId,
                        principalTable: "Gamespaces",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Players_Profiles_PersonId",
                        column: x => x.PersonId,
                        principalTable: "Profiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Gamespaces_TopologyId",
                table: "Gamespaces",
                column: "TopologyId");

            migrationBuilder.CreateIndex(
                name: "IX_Players_GamespaceId",
                table: "Players",
                column: "GamespaceId");

            migrationBuilder.CreateIndex(
                name: "IX_Players_PersonId",
                table: "Players",
                column: "PersonId");

            migrationBuilder.CreateIndex(
                name: "IX_Templates_ParentId",
                table: "Templates",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_Templates_TopologyId",
                table: "Templates",
                column: "TopologyId");

            migrationBuilder.CreateIndex(
                name: "IX_Workers_PersonId",
                table: "Workers",
                column: "PersonId");

            migrationBuilder.CreateIndex(
                name: "IX_Workers_TopologyId",
                table: "Workers",
                column: "TopologyId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Players");

            migrationBuilder.DropTable(
                name: "Templates");

            migrationBuilder.DropTable(
                name: "Workers");

            migrationBuilder.DropTable(
                name: "Gamespaces");

            migrationBuilder.DropTable(
                name: "Profiles");

            migrationBuilder.DropTable(
                name: "Topologies");
        }
    }
}
