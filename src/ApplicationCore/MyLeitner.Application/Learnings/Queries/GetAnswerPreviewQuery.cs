using Dela.Toolkit.Application;
using MyLeitner.Application.Contract.Dtos;
using MyLeitner.Application.Learnings.Enums;
using MyLeitner.Domain.Flashcards;
using MyLeitner.Domain.LearningSchedules;
using MyLeitner.Domain.LearningSchedules.Exceptions;

namespace MyLeitner.Application.Learnings.Queries;

public record GetAnswerPreviewQuery(long FlashcardId, int ResponseTimeInSeconds, long UserId) : IQuery;

/// <summary>
/// برای یک فلش‌کارت: فاصلهٔ مرور بعدی (به روز) به ازای هر پاسخ ممکن. محاسبه همان <c>AnswerProcess</c> است، ولی چیزی تغییر یا ذخیره نمی‌کند.
/// </summary>
public class GetAnswerPreviewQueryHandler(ILearningScheduleRepository schedules)
    : IQueryHandler<GetAnswerPreviewQuery, List<AnswerIntervalPreviewDto>>
{
    public async Task<List<AnswerIntervalPreviewDto>> Handle(GetAnswerPreviewQuery query, CancellationToken cancellationToken)
    {
        var schedule = await schedules.GetByFlashcardIdAsync(query.FlashcardId, cancellationToken)
                       ?? throw new LearningScheduleNotFoundException(query.FlashcardId);

        return Enum.GetValues<AnswerRateTimeType>()
            .Select(type => AnswerTypeEnum.Create((short)type))
            .Select(answer =>
            {
                var days = schedule.PreviewIntervalDays(answer, query.ResponseTimeInSeconds, LearningSchedule.DefaultUserGlobalMedianSeconds);
                var masters = schedule.WouldMaster(answer, query.ResponseTimeInSeconds, LearningSchedule.DefaultUserGlobalMedianSeconds);
                return new AnswerIntervalPreviewDto(answer.Id, days, masters, masters ? LearningStep.Mastered : LearningStep.ForInterval(days));
            })
            .ToList();
    }
}
