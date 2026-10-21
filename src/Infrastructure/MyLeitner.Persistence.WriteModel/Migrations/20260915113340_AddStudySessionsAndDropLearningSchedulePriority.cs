using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyLeitner.Persistence.WriteModel.Migrations
{
    /// <inheritdoc />
    public partial class AddStudySessionsAndDropLearningSchedulePriority : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Priority",
                schema: "dbo",
                table: "LearningSchedules");

            migrationBuilder.CreateTable(
                name: "StudySessions",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    ScopeType = table.Column<int>(type: "int", nullable: false),
                    ScopeId = table.Column<long>(type: "bigint", nullable: false),
                    SessionDate = table.Column<DateOnly>(type: "date", nullable: false),
                    CurrentFlashcardId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudySessions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "StudySessionCards",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StudySessionId = table.Column<long>(type: "bigint", nullable: false),
                    FlashcardId = table.Column<long>(type: "bigint", nullable: false),
                    BoxId = table.Column<long>(type: "bigint", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    AnswerTypeId = table.Column<short>(type: "smallint", nullable: true),
                    ShownAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ResolvedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudySessionCards", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StudySessionCards_StudySessions_StudySessionId",
                        column: x => x.StudySessionId,
                        principalSchema: "dbo",
                        principalTable: "StudySessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_StudySessionCards_StudySessionId",
                schema: "dbo",
                table: "StudySessionCards",
                column: "StudySessionId");

            migrationBuilder.CreateIndex(
                name: "IX_StudySessions_UserId_ScopeType_ScopeId_SessionDate",
                schema: "dbo",
                table: "StudySessions",
                columns: new[] { "UserId", "ScopeType", "ScopeId", "SessionDate" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StudySessionCards",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "StudySessions",
                schema: "dbo");

            migrationBuilder.AddColumn<double>(
                name: "Priority",
                schema: "dbo",
                table: "LearningSchedules",
                type: "float",
                nullable: false,
                defaultValue: 0.0);
        }
    }
}
