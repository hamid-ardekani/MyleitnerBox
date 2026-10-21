using Dela.Toolkit.Domain.Exceptions;

namespace MyLeitner.Domain.LearningSchedules.Exceptions;

public class LearningScheduleNotFoundException : ExceptionManager
{
    public LearningScheduleNotFoundException(long flashcardId) : base(string.Format(Errors.LearningScheduleNotFoundException, flashcardId))
    {
    }
}
