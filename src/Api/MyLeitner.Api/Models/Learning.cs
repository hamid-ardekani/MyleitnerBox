using MyLeitner.Application.Learnings.Enums;

namespace MyLeitner.Api.Models;

public record AnswerProcessRequest(long FlashcardId, AnswerRateTimeType AnswerRateTimeType, int ResponseTimeInSeconds, long? CategoryId);
public record ShowLaterProcessRequest(long FlashcardId, long? CategoryId);
public record MarkKnownRequest(long FlashcardId, long? CategoryId);
public record BacklogMoreRequest(long BoxId, int Amount);