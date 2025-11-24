using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Aure.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarContractTemplateConfig : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "contract_template_configs",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    company_id = table.Column<Guid>(type: "uuid", nullable: false),
                    titulo_servico = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    descricao_servico = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    local_prestacao_servico = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    detalhamento_servicos = table.Column<string>(type: "jsonb", nullable: false),
                    clausula_ajuda_custo = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    obrigacoes_contratado = table.Column<string>(type: "jsonb", nullable: false),
                    obrigacoes_contratante = table.Column<string>(type: "jsonb", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_contract_template_configs", x => x.id);
                    table.ForeignKey(
                        name: "FK_contract_template_configs_companies_company_id",
                        column: x => x.company_id,
                        principalTable: "companies",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_contract_template_configs_company_id",
                table: "contract_template_configs",
                column: "company_id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "contract_template_configs");
        }
    }
}
