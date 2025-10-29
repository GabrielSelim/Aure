using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Aure.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUserTermsFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "AceitouTermosUso",
                table: "Users",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "DataAceiteTermosUso",
                table: "Users",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VersaoTermosUsoAceita",
                table: "Users",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "AceitouPoliticaPrivacidade",
                table: "Users",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "DataAceitePoliticaPrivacidade",
                table: "Users",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VersaoPoliticaPrivacidadeAceita",
                table: "Users",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AceitouTermosUso",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "DataAceiteTermosUso",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "VersaoTermosUsoAceita",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "AceitouPoliticaPrivacidade",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "DataAceitePoliticaPrivacidade",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "VersaoPoliticaPrivacidadeAceita",
                table: "Users");
        }
    }
}
