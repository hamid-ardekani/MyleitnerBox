using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyLeitner.Persistence.WriteModel.Migrations
{
    /// <inheritdoc />
    public partial class AddBoxDailyNewCardLimit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<short>(
                name: "DailyNewCardLimit",
                schema: "dbo",
                table: "Boxes",
                type: "smallint",
                nullable: false,
                defaultValue: (short)10); // existing boxes get Box.DefaultDailyNewCardLimit
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DailyNewCardLimit",
                schema: "dbo",
                table: "Boxes");
        }
    }
}
