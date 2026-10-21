using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyLeitner.Persistence.WriteModel.Migrations
{
    /// <inheritdoc />
    public partial class AddReviewLogs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ReviewLogs",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    FlashcardId = table.Column<long>(type: "bigint", nullable: false),
                    BoxId = table.Column<long>(type: "bigint", nullable: false),
                    CategoryId = table.Column<long>(type: "bigint", nullable: true),
                    AnswerTypeId = table.Column<short>(type: "smallint", nullable: false),
                    ResponseTimeInSeconds = table.Column<int>(type: "int", nullable: false),
                    AnsweredAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReviewDate = table.Column<DateOnly>(type: "date", nullable: false),
                    PreviousAnsweredAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    StabilityBefore = table.Column<double>(type: "float", nullable: false),
                    DifficultyBefore = table.Column<double>(type: "float", nullable: false),
                    IntervalBefore = table.Column<double>(type: "float", nullable: false),
                    StabilityAfter = table.Column<double>(type: "float", nullable: false),
                    DifficultyAfter = table.Column<double>(type: "float", nullable: false),
                    IntervalAfter = table.Column<double>(type: "float", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReviewLogs", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ReviewLogs_FlashcardId_AnsweredAt",
                schema: "dbo",
                table: "ReviewLogs",
                columns: new[] { "FlashcardId", "AnsweredAt" });

            migrationBuilder.CreateIndex(
                name: "IX_ReviewLogs_UserId_ReviewDate",
                schema: "dbo",
                table: "ReviewLogs",
                columns: new[] { "UserId", "ReviewDate" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ReviewLogs",
                schema: "dbo");
        }
    }
}
