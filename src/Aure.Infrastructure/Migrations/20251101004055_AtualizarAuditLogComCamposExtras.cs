using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Aure.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AtualizarAuditLogComCamposExtras : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Guid>(
                name: "performed_by",
                table: "auditlogs",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<string>(
                name: "ip_address",
                table: "auditlogs",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<Guid>(
                name: "entity_id",
                table: "auditlogs",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<double>(
                name: "duration",
                table: "auditlogs",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<string>(
                name: "http_method",
                table: "auditlogs",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "path",
                table: "auditlogs",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "performed_by_email",
                table: "auditlogs",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "status_code",
                table: "auditlogs",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "success",
                table: "auditlogs",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "user_agent",
                table: "auditlogs",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "duration",
                table: "auditlogs");

            migrationBuilder.DropColumn(
                name: "http_method",
                table: "auditlogs");

            migrationBuilder.DropColumn(
                name: "path",
                table: "auditlogs");

            migrationBuilder.DropColumn(
                name: "performed_by_email",
                table: "auditlogs");

            migrationBuilder.DropColumn(
                name: "status_code",
                table: "auditlogs");

            migrationBuilder.DropColumn(
                name: "success",
                table: "auditlogs");

            migrationBuilder.DropColumn(
                name: "user_agent",
                table: "auditlogs");

            migrationBuilder.AlterColumn<Guid>(
                name: "performed_by",
                table: "auditlogs",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ip_address",
                table: "auditlogs",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "entity_id",
                table: "auditlogs",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);
        }
    }
}
