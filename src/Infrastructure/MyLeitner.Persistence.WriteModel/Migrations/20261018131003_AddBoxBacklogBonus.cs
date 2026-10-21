using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyLeitner.Persistence.WriteModel.Migrations
{
    /// <inheritdoc />
    public partial class AddBoxBacklogBonus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<short>(
                name: "BacklogBonus",
                schema: "dbo",
                table: "Boxes",
                type: "smallint",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.AddColumn<DateOnly>(
                name: "BacklogBonusDate",
                schema: "dbo",
                table: "Boxes",
                type: "date",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BacklogBonus",
                schema: "dbo",
                table: "Boxes");

            migrationBuilder.DropColumn(
                name: "BacklogBonusDate",
                schema: "dbo",
                table: "Boxes");
        }
    }
}
