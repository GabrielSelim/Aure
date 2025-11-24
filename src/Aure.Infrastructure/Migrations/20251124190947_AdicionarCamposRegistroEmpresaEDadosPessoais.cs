using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Aure.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarCamposRegistroEmpresaEDadosPessoais : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "estado_civil",
                table: "users",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "nacionalidade",
                table: "users",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "orgao_expedidor_r_g",
                table: "users",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "nire",
                table: "companies",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "state_registration",
                table: "companies",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "estado_civil",
                table: "users");

            migrationBuilder.DropColumn(
                name: "nacionalidade",
                table: "users");

            migrationBuilder.DropColumn(
                name: "orgao_expedidor_r_g",
                table: "users");

            migrationBuilder.DropColumn(
                name: "nire",
                table: "companies");

            migrationBuilder.DropColumn(
                name: "state_registration",
                table: "companies");
        }
    }
}
