using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

namespace TopoMojo.Web.Data.Migrations.TopoMojo
{
    public partial class topouseuplinkswitch : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "UseUplinkSwitch",
                table: "Topologies",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UseUplinkSwitch",
                table: "Topologies");
        }
    }
}
