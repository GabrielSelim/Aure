using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Aure.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddBusinessModelToCompany : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "business_model",
                table: "companies",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "business_model",
                table: "companies");
        }
    }
}
