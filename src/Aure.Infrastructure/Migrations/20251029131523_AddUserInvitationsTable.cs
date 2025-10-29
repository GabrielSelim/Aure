using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Aure.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUserInvitationsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "idx_users_email",
                table: "users");

            migrationBuilder.AddColumn<bool>(
                name: "aceitou_politica_privacidade",
                table: "users",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "aceitou_termos_uso",
                table: "users",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "avatar_url",
                table: "users",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "c_p_f_encrypted",
                table: "users",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "cargo",
                table: "users",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "data_aceite_politica_privacidade",
                table: "users",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "data_aceite_termos_uso",
                table: "users",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "data_nascimento",
                table: "users",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "endereco_bairro",
                table: "users",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "endereco_cep",
                table: "users",
                type: "character varying(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "endereco_cidade",
                table: "users",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "endereco_complemento",
                table: "users",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "endereco_estado",
                table: "users",
                type: "character varying(2)",
                maxLength: 2,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "endereco_numero",
                table: "users",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "endereco_pais",
                table: "users",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "endereco_rua",
                table: "users",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "r_g_encrypted",
                table: "users",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "telefone_celular",
                table: "users",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "telefone_fixo",
                table: "users",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "versao_politica_privacidade_aceita",
                table: "users",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "versao_termos_uso_aceita",
                table: "users",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "notificationpreferences",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id1 = table.Column<Guid>(type: "uuid", nullable: false),
                    receber_email_novo_contrato = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    receber_email_contrato_assinado = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    receber_email_contrato_vencendo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    receber_email_pagamento_processado = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    receber_email_pagamento_recebido = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    receber_email_novo_funcionario = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    receber_email_alertas_financeiros = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    receber_email_atualizacoes_sistema = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_notificationpreferences", x => x.id);
                    table.ForeignKey(
                        name: "FK_notificationpreferences_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_notificationpreferences_users_user_id1",
                        column: x => x.user_id1,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "userinvitations",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    email = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    role = table.Column<string>(type: "text", nullable: false),
                    cargo = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    company_id = table.Column<Guid>(type: "uuid", nullable: false),
                    invited_by_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    invitation_token = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    expires_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    accepted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    accepted_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_userinvitations", x => x.id);
                    table.ForeignKey(
                        name: "FK_userinvitations_companies_company_id",
                        column: x => x.company_id,
                        principalTable: "companies",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_userinvitations_users_accepted_by_user_id",
                        column: x => x.accepted_by_user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_userinvitations_users_invited_by_user_id",
                        column: x => x.invited_by_user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "idx_users_email",
                table: "users",
                column: "email",
                unique: true,
                filter: "is_deleted = false");

            migrationBuilder.CreateIndex(
                name: "IX_users_c_p_f_encrypted",
                table: "users",
                column: "c_p_f_encrypted",
                unique: true,
                filter: "c_p_f_encrypted IS NOT NULL AND is_deleted = false");

            migrationBuilder.CreateIndex(
                name: "IX_users_data_nascimento",
                table: "users",
                column: "data_nascimento");

            migrationBuilder.CreateIndex(
                name: "IX_users_is_deleted",
                table: "users",
                column: "is_deleted");

            migrationBuilder.CreateIndex(
                name: "IX_notificationpreferences_is_deleted",
                table: "notificationpreferences",
                column: "is_deleted");

            migrationBuilder.CreateIndex(
                name: "IX_notificationpreferences_user_id",
                table: "notificationpreferences",
                column: "user_id",
                unique: true,
                filter: "is_deleted = false");

            migrationBuilder.CreateIndex(
                name: "IX_notificationpreferences_user_id1",
                table: "notificationpreferences",
                column: "user_id1");

            migrationBuilder.CreateIndex(
                name: "IX_userinvitations_accepted_by_user_id",
                table: "userinvitations",
                column: "accepted_by_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_userinvitations_company_id_status",
                table: "userinvitations",
                columns: new[] { "company_id", "status" });

            migrationBuilder.CreateIndex(
                name: "IX_userinvitations_email",
                table: "userinvitations",
                column: "email");

            migrationBuilder.CreateIndex(
                name: "IX_userinvitations_invitation_token",
                table: "userinvitations",
                column: "invitation_token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_userinvitations_invited_by_user_id",
                table: "userinvitations",
                column: "invited_by_user_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "notificationpreferences");

            migrationBuilder.DropTable(
                name: "userinvitations");

            migrationBuilder.DropIndex(
                name: "idx_users_email",
                table: "users");

            migrationBuilder.DropIndex(
                name: "IX_users_c_p_f_encrypted",
                table: "users");

            migrationBuilder.DropIndex(
                name: "IX_users_data_nascimento",
                table: "users");

            migrationBuilder.DropIndex(
                name: "IX_users_is_deleted",
                table: "users");

            migrationBuilder.DropColumn(
                name: "aceitou_politica_privacidade",
                table: "users");

            migrationBuilder.DropColumn(
                name: "aceitou_termos_uso",
                table: "users");

            migrationBuilder.DropColumn(
                name: "avatar_url",
                table: "users");

            migrationBuilder.DropColumn(
                name: "c_p_f_encrypted",
                table: "users");

            migrationBuilder.DropColumn(
                name: "cargo",
                table: "users");

            migrationBuilder.DropColumn(
                name: "data_aceite_politica_privacidade",
                table: "users");

            migrationBuilder.DropColumn(
                name: "data_aceite_termos_uso",
                table: "users");

            migrationBuilder.DropColumn(
                name: "data_nascimento",
                table: "users");

            migrationBuilder.DropColumn(
                name: "endereco_bairro",
                table: "users");

            migrationBuilder.DropColumn(
                name: "endereco_cep",
                table: "users");

            migrationBuilder.DropColumn(
                name: "endereco_cidade",
                table: "users");

            migrationBuilder.DropColumn(
                name: "endereco_complemento",
                table: "users");

            migrationBuilder.DropColumn(
                name: "endereco_estado",
                table: "users");

            migrationBuilder.DropColumn(
                name: "endereco_numero",
                table: "users");

            migrationBuilder.DropColumn(
                name: "endereco_pais",
                table: "users");

            migrationBuilder.DropColumn(
                name: "endereco_rua",
                table: "users");

            migrationBuilder.DropColumn(
                name: "r_g_encrypted",
                table: "users");

            migrationBuilder.DropColumn(
                name: "telefone_celular",
                table: "users");

            migrationBuilder.DropColumn(
                name: "telefone_fixo",
                table: "users");

            migrationBuilder.DropColumn(
                name: "versao_politica_privacidade_aceita",
                table: "users");

            migrationBuilder.DropColumn(
                name: "versao_termos_uso_aceita",
                table: "users");

            migrationBuilder.CreateIndex(
                name: "idx_users_email",
                table: "users",
                column: "email",
                unique: true);
        }
    }
}
