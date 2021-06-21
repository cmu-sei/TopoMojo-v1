﻿using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace TopoMojo.Web.Data.Migrations.SqlServer.TopoMojoDb
{
    public partial class AddCompositeKeys : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ApiKey_Users_UserId",
                table: "ApiKey");

            migrationBuilder.DropForeignKey(
                name: "FK_Gamespaces_Workspaces_WorkspaceId",
                table: "Gamespaces");

            migrationBuilder.DropForeignKey(
                name: "FK_Players_Gamespaces_GamespaceId",
                table: "Players");

            migrationBuilder.DropForeignKey(
                name: "FK_Workers_Workspaces_WorkspaceId",
                table: "Workers");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Workers",
                table: "Workers");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Players",
                table: "Players");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ApiKey",
                table: "ApiKey");

            migrationBuilder.DropColumn(
                name: "DocumentUrl",
                table: "Workspaces");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "Workers");

            migrationBuilder.DropColumn(
                name: "CallbackUrl",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "Players");

            migrationBuilder.RenameTable(
                name: "ApiKey",
                newName: "ApiKeys");

            migrationBuilder.RenameColumn(
                name: "SessionLimit",
                table: "Users",
                newName: "GamespaceCleanupGraceMinutes");

            migrationBuilder.RenameColumn(
                name: "StopTime",
                table: "Gamespaces",
                newName: "EndTime");

            migrationBuilder.RenameColumn(
                name: "ClientId",
                table: "Gamespaces",
                newName: "ManagerId");

            migrationBuilder.RenameIndex(
                name: "IX_ApiKey_UserId",
                table: "ApiKeys",
                newName: "IX_ApiKeys_UserId");

            migrationBuilder.AddColumn<bool>(
                name: "HostAffinity",
                table: "Workspaces",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<string>(
                name: "WorkspaceId",
                table: "Workers",
                type: "nchar(36)",
                fixedLength: true,
                maxLength: 36,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nchar(36)",
                oldFixedLength: true,
                oldMaxLength: 36,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "SubjectName",
                table: "Workers",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "SubjectId",
                table: "Workers",
                type: "nchar(36)",
                fixedLength: true,
                maxLength: 36,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "SubjectName",
                table: "Players",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "SubjectId",
                table: "Players",
                type: "nchar(36)",
                fixedLength: true,
                maxLength: 36,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "GamespaceId",
                table: "Players",
                type: "nchar(36)",
                fixedLength: true,
                maxLength: 36,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nchar(36)",
                oldFixedLength: true,
                oldMaxLength: 36,
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CleanupGraceMinutes",
                table: "Gamespaces",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Hash",
                table: "ApiKeys",
                type: "nchar(64)",
                fixedLength: true,
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "ApiKeys",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "WhenCreated",
                table: "ApiKeys",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddPrimaryKey(
                name: "PK_Workers",
                table: "Workers",
                columns: new[] { "SubjectId", "WorkspaceId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_Players",
                table: "Players",
                columns: new[] { "SubjectId", "GamespaceId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_ApiKeys",
                table: "ApiKeys",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_ApiKeys_Hash",
                table: "ApiKeys",
                column: "Hash");

            migrationBuilder.AddForeignKey(
                name: "FK_ApiKeys_Users_UserId",
                table: "ApiKeys",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Gamespaces_Workspaces_WorkspaceId",
                table: "Gamespaces",
                column: "WorkspaceId",
                principalTable: "Workspaces",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Players_Gamespaces_GamespaceId",
                table: "Players",
                column: "GamespaceId",
                principalTable: "Gamespaces",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Workers_Workspaces_WorkspaceId",
                table: "Workers",
                column: "WorkspaceId",
                principalTable: "Workspaces",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ApiKeys_Users_UserId",
                table: "ApiKeys");

            migrationBuilder.DropForeignKey(
                name: "FK_Gamespaces_Workspaces_WorkspaceId",
                table: "Gamespaces");

            migrationBuilder.DropForeignKey(
                name: "FK_Players_Gamespaces_GamespaceId",
                table: "Players");

            migrationBuilder.DropForeignKey(
                name: "FK_Workers_Workspaces_WorkspaceId",
                table: "Workers");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Workers",
                table: "Workers");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Players",
                table: "Players");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ApiKeys",
                table: "ApiKeys");

            migrationBuilder.DropIndex(
                name: "IX_ApiKeys_Hash",
                table: "ApiKeys");

            migrationBuilder.DropColumn(
                name: "HostAffinity",
                table: "Workspaces");

            migrationBuilder.DropColumn(
                name: "CleanupGraceMinutes",
                table: "Gamespaces");

            migrationBuilder.DropColumn(
                name: "Hash",
                table: "ApiKeys");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "ApiKeys");

            migrationBuilder.DropColumn(
                name: "WhenCreated",
                table: "ApiKeys");

            migrationBuilder.RenameTable(
                name: "ApiKeys",
                newName: "ApiKey");

            migrationBuilder.RenameColumn(
                name: "GamespaceCleanupGraceMinutes",
                table: "Users",
                newName: "SessionLimit");

            migrationBuilder.RenameColumn(
                name: "ManagerId",
                table: "Gamespaces",
                newName: "ClientId");

            migrationBuilder.RenameColumn(
                name: "EndTime",
                table: "Gamespaces",
                newName: "StopTime");

            migrationBuilder.RenameIndex(
                name: "IX_ApiKeys_UserId",
                table: "ApiKey",
                newName: "IX_ApiKey_UserId");

            migrationBuilder.AddColumn<string>(
                name: "DocumentUrl",
                table: "Workspaces",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "SubjectName",
                table: "Workers",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(64)",
                oldMaxLength: 64,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "WorkspaceId",
                table: "Workers",
                type: "nchar(36)",
                fixedLength: true,
                maxLength: 36,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nchar(36)",
                oldFixedLength: true,
                oldMaxLength: 36);

            migrationBuilder.AlterColumn<string>(
                name: "SubjectId",
                table: "Workers",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nchar(36)",
                oldFixedLength: true,
                oldMaxLength: 36);

            migrationBuilder.AddColumn<int>(
                name: "Id",
                table: "Workers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "CallbackUrl",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "SubjectName",
                table: "Players",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(64)",
                oldMaxLength: 64,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "GamespaceId",
                table: "Players",
                type: "nchar(36)",
                fixedLength: true,
                maxLength: 36,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nchar(36)",
                oldFixedLength: true,
                oldMaxLength: 36);

            migrationBuilder.AlterColumn<string>(
                name: "SubjectId",
                table: "Players",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nchar(36)",
                oldFixedLength: true,
                oldMaxLength: 36);

            migrationBuilder.AddColumn<int>(
                name: "Id",
                table: "Players",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Workers",
                table: "Workers",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Players",
                table: "Players",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ApiKey",
                table: "ApiKey",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ApiKey_Users_UserId",
                table: "ApiKey",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Gamespaces_Workspaces_WorkspaceId",
                table: "Gamespaces",
                column: "WorkspaceId",
                principalTable: "Workspaces",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Players_Gamespaces_GamespaceId",
                table: "Players",
                column: "GamespaceId",
                principalTable: "Gamespaces",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Workers_Workspaces_WorkspaceId",
                table: "Workers",
                column: "WorkspaceId",
                principalTable: "Workspaces",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
