using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Wms.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueIndexToCompanyCode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Companies_Code",
                table: "Companies",
                column: "Code",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Companies_Code",
                table: "Companies");
        }

    }
}
