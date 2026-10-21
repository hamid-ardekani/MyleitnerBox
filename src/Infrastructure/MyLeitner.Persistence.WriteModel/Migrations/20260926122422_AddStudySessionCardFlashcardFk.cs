using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyLeitner.Persistence.WriteModel.Migrations
{
    /// <inheritdoc />
    public partial class AddStudySessionCardFlashcardFk : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_StudySessionCards_FlashcardId",
                schema: "dbo",
                table: "StudySessionCards",
                column: "FlashcardId");

            migrationBuilder.AddForeignKey(
                name: "FK_StudySessionCards_Flashcards_FlashcardId",
                schema: "dbo",
                table: "StudySessionCards",
                column: "FlashcardId",
                principalSchema: "dbo",
                principalTable: "Flashcards",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StudySessionCards_Flashcards_FlashcardId",
                schema: "dbo",
                table: "StudySessionCards");

            migrationBuilder.DropIndex(
                name: "IX_StudySessionCards_FlashcardId",
                schema: "dbo",
                table: "StudySessionCards");
        }
    }
}
