using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyLeitner.Persistence.WriteModel.Migrations
{
    /// <inheritdoc />
    public partial class AddCategoryBoxUniqueIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CategoryBoxes_CategoryId",
                schema: "dbo",
                table: "CategoryBoxes");

            migrationBuilder.CreateIndex(
                name: "IX_CategoryBoxes_CategoryId_BoxId",
                schema: "dbo",
                table: "CategoryBoxes",
                columns: new[] { "CategoryId", "BoxId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CategoryBoxes_CategoryId_BoxId",
                schema: "dbo",
                table: "CategoryBoxes");

            migrationBuilder.CreateIndex(
                name: "IX_CategoryBoxes_CategoryId",
                schema: "dbo",
                table: "CategoryBoxes",
                column: "CategoryId");
        }
    }
}
