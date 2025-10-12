using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Aure.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUserInvitesTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "userinvites",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    inviter_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    invitee_email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    invitee_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    role = table.Column<int>(type: "integer", nullable: false),
                    business_model = table.Column<int>(type: "integer", nullable: true),
                    company_name = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    cnpj = table.Column<string>(type: "character varying(18)", maxLength: 18, nullable: true),
                    company_type = table.Column<int>(type: "integer", nullable: true),
                    company_id = table.Column<Guid>(type: "uuid", nullable: false),
                    invited_by_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    token = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    expires_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    is_accepted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    invite_type = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_userinvites", x => x.id);
                    table.ForeignKey(
                        name: "FK_userinvites_companies_company_id",
                        column: x => x.company_id,
                        principalTable: "companies",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_userinvites_users_invited_by_user_id",
                        column: x => x.invited_by_user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_userinvites_company_id_is_accepted_is_deleted",
                table: "userinvites",
                columns: new[] { "company_id", "is_accepted", "is_deleted" });

            migrationBuilder.CreateIndex(
                name: "IX_userinvites_invited_by_user_id",
                table: "userinvites",
                column: "invited_by_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_userinvites_invitee_email",
                table: "userinvites",
                column: "invitee_email");

            migrationBuilder.CreateIndex(
                name: "IX_userinvites_token",
                table: "userinvites",
                column: "token",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "userinvites");
        }
    }
}
