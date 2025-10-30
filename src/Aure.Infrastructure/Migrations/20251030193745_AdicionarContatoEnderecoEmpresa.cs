using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Aure.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarContatoEnderecoEmpresa : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "address_city",
                table: "companies",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "address_complement",
                table: "companies",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "address_country",
                table: "companies",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "address_neighborhood",
                table: "companies",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "address_number",
                table: "companies",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "address_state",
                table: "companies",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "address_street",
                table: "companies",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "address_zip_code",
                table: "companies",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "phone_landline",
                table: "companies",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "phone_mobile",
                table: "companies",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "address_city",
                table: "companies");

            migrationBuilder.DropColumn(
                name: "address_complement",
                table: "companies");

            migrationBuilder.DropColumn(
                name: "address_country",
                table: "companies");

            migrationBuilder.DropColumn(
                name: "address_neighborhood",
                table: "companies");

            migrationBuilder.DropColumn(
                name: "address_number",
                table: "companies");

            migrationBuilder.DropColumn(
                name: "address_state",
                table: "companies");

            migrationBuilder.DropColumn(
                name: "address_street",
                table: "companies");

            migrationBuilder.DropColumn(
                name: "address_zip_code",
                table: "companies");

            migrationBuilder.DropColumn(
                name: "phone_landline",
                table: "companies");

            migrationBuilder.DropColumn(
                name: "phone_mobile",
                table: "companies");
        }
    }
}
