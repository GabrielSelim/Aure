using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Aure.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CreateNotificationPreferences : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "NotificationPreferences",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReceberEmailNovoContrato = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    ReceberEmailContratoAssinado = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    ReceberEmailContratoVencendo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    ReceberEmailPagamentoProcessado = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    ReceberEmailPagamentoRecebido = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    ReceberEmailNovoFuncionario = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    ReceberEmailAlertasFinanceiros = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    ReceberEmailAtualizacoesSistema = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotificationPreferences", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NotificationPreferences_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_NotificationPreferences_UserId",
                table: "NotificationPreferences",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_NotificationPreferences_IsDeleted",
                table: "NotificationPreferences",
                column: "IsDeleted");

            // Criar preferências padrão para usuários existentes
            migrationBuilder.Sql(@"
                INSERT INTO ""NotificationPreferences"" (""Id"", ""UserId"", ""CreatedAt"", ""UpdatedAt"", ""IsDeleted"")
                SELECT gen_random_uuid(), ""Id"", NOW(), NOW(), false
                FROM ""Users""
                WHERE ""IsDeleted"" = false
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "NotificationPreferences");
        }
    }
}
