using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyLeitner.Persistence.WriteModel.Migrations
{
    /// <inheritdoc />
    public partial class AddBacklogFloorAndReviewDueAt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DueAt",
                schema: "dbo",
                table: "ReviewLogs",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<short>(
                name: "DailyBacklogFloor",
                schema: "dbo",
                table: "Boxes",
                type: "smallint",
                nullable: false,
                defaultValue: (short)20); // existing boxes get BacklogQuota.DefaultFloor
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DueAt",
                schema: "dbo",
                table: "ReviewLogs");

            migrationBuilder.DropColumn(
                name: "DailyBacklogFloor",
                schema: "dbo",
                table: "Boxes");
        }
    }
}
