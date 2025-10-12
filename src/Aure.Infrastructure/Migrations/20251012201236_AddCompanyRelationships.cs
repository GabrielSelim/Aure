using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Aure.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCompanyRelationships : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "companyrelationships",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    client_company_id = table.Column<Guid>(type: "uuid", nullable: false),
                    provider_company_id = table.Column<Guid>(type: "uuid", nullable: false),
                    type = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    start_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    end_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    notes = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_companyrelationships", x => x.id);
                    table.ForeignKey(
                        name: "FK_companyrelationships_companies_client_company_id",
                        column: x => x.client_company_id,
                        principalTable: "companies",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_companyrelationships_companies_provider_company_id",
                        column: x => x.provider_company_id,
                        principalTable: "companies",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_companyrelationships_client_company_id_provider_company_id_~",
                table: "companyrelationships",
                columns: new[] { "client_company_id", "provider_company_id", "type" },
                unique: true,
                filter: "is_deleted = false");

            migrationBuilder.CreateIndex(
                name: "IX_companyrelationships_provider_company_id",
                table: "companyrelationships",
                column: "provider_company_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "companyrelationships");
        }
    }
}
