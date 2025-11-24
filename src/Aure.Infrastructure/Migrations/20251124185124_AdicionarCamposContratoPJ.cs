using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Aure.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarCamposContratoPJ : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "dia_pagamento",
                table: "contracts",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "dia_vencimento_n_f",
                table: "contracts",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "type",
                table: "contracts",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "dia_pagamento",
                table: "contracts");

            migrationBuilder.DropColumn(
                name: "dia_vencimento_n_f",
                table: "contracts");

            migrationBuilder.DropColumn(
                name: "type",
                table: "contracts");
        }
    }
}
