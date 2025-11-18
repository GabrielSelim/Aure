using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Aure.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarSistemaTemplatesContratos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_notificationpreferences_users_user_id",
                table: "notificationpreferences");

            migrationBuilder.DropForeignKey(
                name: "FK_notificationpreferences_users_user_id1",
                table: "notificationpreferences");

            migrationBuilder.DropIndex(
                name: "IX_notificationpreferences_user_id1",
                table: "notificationpreferences");

            migrationBuilder.DropColumn(
                name: "user_id1",
                table: "notificationpreferences");

            migrationBuilder.CreateTable(
                name: "contracttemplates",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    nome = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    descricao = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    tipo = table.Column<string>(type: "text", nullable: false),
                    conteudo_html = table.Column<string>(type: "text", nullable: false),
                    conteudo_docx = table.Column<string>(type: "text", nullable: true),
                    eh_padrao = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    ativo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    company_id = table.Column<Guid>(type: "uuid", nullable: false),
                    variaveis_disponiveis = table.Column<string>(type: "jsonb", nullable: false),
                    data_desativacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    motivo_desativacao = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_contracttemplates", x => x.id);
                    table.ForeignKey(
                        name: "FK_contracttemplates_companies_company_id",
                        column: x => x.company_id,
                        principalTable: "companies",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "contractdocuments",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    contract_id = table.Column<Guid>(type: "uuid", nullable: false),
                    template_id = table.Column<Guid>(type: "uuid", nullable: true),
                    conteudo_html = table.Column<string>(type: "text", nullable: false),
                    conteudo_pdf = table.Column<string>(type: "text", nullable: true),
                    conteudo_docx = table.Column<string>(type: "text", nullable: true),
                    versao_major = table.Column<int>(type: "integer", nullable: false),
                    versao_minor = table.Column<int>(type: "integer", nullable: false),
                    data_geracao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    gerado_por_usuario_id = table.Column<Guid>(type: "uuid", nullable: true),
                    dados_preenchidos = table.Column<string>(type: "jsonb", nullable: false),
                    eh_versao_final = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    hash_documento = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_contractdocuments", x => x.id);
                    table.ForeignKey(
                        name: "FK_contractdocuments_contracts_contract_id",
                        column: x => x.contract_id,
                        principalTable: "contracts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_contractdocuments_contracttemplates_template_id",
                        column: x => x.template_id,
                        principalTable: "contracttemplates",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_contractdocuments_users_gerado_por_usuario_id",
                        column: x => x.gerado_por_usuario_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_contractdocuments_contract_id",
                table: "contractdocuments",
                column: "contract_id");

            migrationBuilder.CreateIndex(
                name: "IX_contractdocuments_contract_id_eh_versao_final",
                table: "contractdocuments",
                columns: new[] { "contract_id", "eh_versao_final" });

            migrationBuilder.CreateIndex(
                name: "IX_contractdocuments_eh_versao_final",
                table: "contractdocuments",
                column: "eh_versao_final");

            migrationBuilder.CreateIndex(
                name: "IX_contractdocuments_gerado_por_usuario_id",
                table: "contractdocuments",
                column: "gerado_por_usuario_id");

            migrationBuilder.CreateIndex(
                name: "IX_contractdocuments_template_id",
                table: "contractdocuments",
                column: "template_id");

            migrationBuilder.CreateIndex(
                name: "IX_contracttemplates_ativo",
                table: "contracttemplates",
                column: "ativo");

            migrationBuilder.CreateIndex(
                name: "IX_contracttemplates_company_id",
                table: "contracttemplates",
                column: "company_id");

            migrationBuilder.CreateIndex(
                name: "IX_contracttemplates_company_id_tipo_eh_padrao",
                table: "contracttemplates",
                columns: new[] { "company_id", "tipo", "eh_padrao" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "contractdocuments");

            migrationBuilder.DropTable(
                name: "contracttemplates");

            migrationBuilder.AddColumn<Guid>(
                name: "user_id1",
                table: "notificationpreferences",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_notificationpreferences_user_id1",
                table: "notificationpreferences",
                column: "user_id1");

            migrationBuilder.AddForeignKey(
                name: "FK_notificationpreferences_users_user_id",
                table: "notificationpreferences",
                column: "user_id",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_notificationpreferences_users_user_id1",
                table: "notificationpreferences",
                column: "user_id1",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
