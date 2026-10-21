using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyLeitner.Persistence.WriteModel.Migrations
{
    /// <inheritdoc />
    public partial class AddLearningScheduleStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Status",
                schema: "dbo",
                table: "LearningSchedules",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "StatusChangedAt",
                schema: "dbo",
                table: "LearningSchedules",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                schema: "dbo",
                table: "LearningSchedules");

            migrationBuilder.DropColumn(
                name: "StatusChangedAt",
                schema: "dbo",
                table: "LearningSchedules");
        }
    }
}
