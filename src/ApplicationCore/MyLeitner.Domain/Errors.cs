namespace MyLeitner.Domain;

public static class Errors
{
    public const string UserCanNotAddFlashcardToBoxException = "User Can Not Add Flashcard To Box";
    public const string BoxNotFoundException = "Box With Id: {0} could not be found.";
    public const string CategoryNotFoundException = "Category With Id: {0} could not be found.";
    
    public const string FlashcardNotFoundException = "Flashcard With Id: {0} could not be found.";
    public const string LearningScheduleNotFoundException = "Learning schedule for flashcard with Id: {0} could not be found.";
    public const string FrontNullException = "Front in Flashcard is empty.";
    public const string BackNullException = "Back in Flashcard is empty.";
    public const string TagsIsEmptyException = "Tags in Flashcard is empty.";
    
    public const string StageWeighNullException = "Stage Weight state is null";
    public const string FinalLevelUnValidException = "Final Level is unvalid";
    public const string OrderUnValidException = "Order is unvalid";
    public const string DailyNewFlashcardCountUnValidException = "Daily New Flashcard Count must be between 1 and 99";
    public const string DailyBacklogFloorUnValidException = "Daily backlog floor must be between 1 and 99";
    public const string ReviewGoalUnValidException = "Review goals must be between 1 and 99999";
    public const string DuplicateBoxException = "There is a box with this name for you";
}