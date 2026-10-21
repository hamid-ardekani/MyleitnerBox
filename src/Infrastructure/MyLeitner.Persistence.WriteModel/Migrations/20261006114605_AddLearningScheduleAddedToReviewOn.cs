using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyLeitner.Persistence.WriteModel.Migrations
{
    /// <inheritdoc />
    public partial class AddLearningScheduleAddedToReviewOn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "AddedToReviewOn",
                schema: "dbo",
                table: "LearningSchedules",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AddedToReviewOn",
                schema: "dbo",
                table: "LearningSchedules");
        }
    }
}
