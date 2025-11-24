using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Aure.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class PermitirMultiplasConfigsPorEmpresa : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_contract_template_configs_company_id",
                table: "contract_template_configs");

            migrationBuilder.AddColumn<string>(
                name: "categoria",
                table: "contract_template_configs",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "nome_config",
                table: "contract_template_configs",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_contract_template_configs_company_id",
                table: "contract_template_configs",
                column: "company_id");

            migrationBuilder.CreateIndex(
                name: "IX_contract_template_configs_company_id_nome_config",
                table: "contract_template_configs",
                columns: new[] { "company_id", "nome_config" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_contract_template_configs_company_id",
                table: "contract_template_configs");

            migrationBuilder.DropIndex(
                name: "IX_contract_template_configs_company_id_nome_config",
                table: "contract_template_configs");

            migrationBuilder.DropColumn(
                name: "categoria",
                table: "contract_template_configs");

            migrationBuilder.DropColumn(
                name: "nome_config",
                table: "contract_template_configs");

            migrationBuilder.CreateIndex(
                name: "IX_contract_template_configs_company_id",
                table: "contract_template_configs",
                column: "company_id",
                unique: true);
        }
    }
}
