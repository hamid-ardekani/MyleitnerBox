using Dela.Toolkit.Application;
using Microsoft.AspNetCore.Mvc;
using MyLeitner.Api.Models;
using MyLeitner.Application.Contract.Dtos;
using MyLeitner.Application.Learnings.Commands;
using MyLeitner.Application.Learnings.Queries;
using MyLeitner.Common.Results;

namespace MyLeitner.Api.Controllers;

public class LearningController(IQueryDispatcher queryDispatcher, ICommandDispatcher commandDispatcher)
    : LeitnerController(queryDispatcher, commandDispatcher)
{
   
    [HttpGet("/Category/{categoryId:long}/Next")]
    public async Task<ActionResult> GetNextPlayFlashcardInCategoryProcess(long categoryId, CancellationToken cancellationToken)
    {
        if (await NotOwnsCategoryAsync(categoryId, cancellationToken)) return NotFound();
        var query = new GetNextPlayFlashcardInCategoryQuery(categoryId, _userId);
        var result = await QueryDispatcher.Dispatch<GetNextPlayFlashcardInCategoryQuery, Result<StudySessionResult>>(query, cancellationToken);

        if (result.IsFailure)
            return result.Error is NotFoundError ? NotFound(result.Error) : BadRequest(result.Error);

        return Ok(result.Value);
    }
    
    [HttpGet("/Box/{boxId:long}/Next")]
    public async Task<ActionResult> GetNextPlayFlashcardInBoxProcess(long boxId, CancellationToken cancellationToken)
    {
        if (await NotOwnsBoxAsync(boxId, cancellationToken)) return NotFound();
        var query = new GetNextPlayFlashcardInBoxQuery(boxId, _userId);
        var result = await QueryDispatcher.Dispatch<GetNextPlayFlashcardInBoxQuery, CardDto>(query, cancellationToken);
        return Ok(result);
    }
    
    /// <summary>وضعیت امروزِ مرور یک جعبه یا دسته‌بندی به تفکیک گام (صفحهٔ پیش از مرور و نوارهای حین مرور).</summary>
    [HttpGet("Overview")]
    public async Task<ActionResult> GetStudyOverview([FromQuery] long? boxId, [FromQuery] long? categoryId, CancellationToken cancellationToken)
    {
        if (boxId is null == categoryId is null)
            return BadRequest("exactly one of boxId / categoryId is required");
        if (await NotOwnsBoxOrCategoryAsync(boxId, categoryId, cancellationToken)) return NotFound();

        var query = new GetStudyOverviewQuery(boxId, categoryId, _userId);
        var result = await QueryDispatcher.Dispatch<GetStudyOverviewQuery, StudyOverviewDto>(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>«دور دوم»: کارت‌هایی که امروز «بلد نبودم»/«شانسی» گرفتند (فقط خواندنی).</summary>
    [HttpGet("Practice")]
    public async Task<ActionResult> GetPractice([FromQuery] long? boxId, [FromQuery] long? categoryId, CancellationToken cancellationToken)
    {
        if (boxId is null == categoryId is null)
            return BadRequest("exactly one of boxId / categoryId is required");
        if (await NotOwnsBoxOrCategoryAsync(boxId, categoryId, cancellationToken)) return NotFound();

        var result = await QueryDispatcher.Dispatch<GetPracticeQuery, PracticeDto>(new GetPracticeQuery(boxId, categoryId, _userId), cancellationToken);
        return Ok(result);
    }

    /// <summary>برای دکمهٔ «دور دوم · X» روی جعبه‌ها و دسته‌ها.</summary>
    [HttpGet("PracticeCounts")]
    public async Task<ActionResult> GetPracticeCounts(CancellationToken cancellationToken)
    {
        var result = await QueryDispatcher.Dispatch<GetPracticeCountsQuery, PracticeCountsDto>(new GetPracticeCountsQuery(_userId), cancellationToken);
        return Ok(result);
    }

    [HttpGet("Preview")]
    public async Task<ActionResult> GetAnswerPreview([FromQuery] long flashcardId, [FromQuery] int responseTimeInSeconds, CancellationToken cancellationToken)
    {
        if (await NotOwnsFlashcardAsync(flashcardId, cancellationToken)) return NotFound();
        var query = new GetAnswerPreviewQuery(flashcardId, responseTimeInSeconds, _userId);
        var result = await QueryDispatcher.Dispatch<GetAnswerPreviewQuery, List<AnswerIntervalPreviewDto>>(query, cancellationToken);
        return Ok(result);
    }

    [HttpPost("AnswerProcess")]
    public async Task<ActionResult> AnswerProcess([FromBody] AnswerProcessRequest request, CancellationToken cancellationToken)
    {
        if (await NotOwnsFlashcardAsync(request.FlashcardId, cancellationToken)) return NotFound();
        if (await NotOwnsBoxOrCategoryAsync(null, request.CategoryId, cancellationToken)) return NotFound();
        var command = new AnswerProcessCommand(request.FlashcardId,request.AnswerRateTimeType,request.ResponseTimeInSeconds, request.CategoryId, _userId);
        await CommandDispatcher.Dispatch(command, cancellationToken);
        return Ok();
    }
    
    [HttpPost("ShowLaterProcess")]
    public async Task<ActionResult> ShowLaterProcess([FromBody] ShowLaterProcessRequest request, CancellationToken cancellationToken)
    {
        if (await NotOwnsFlashcardAsync(request.FlashcardId, cancellationToken)) return NotFound();
        if (await NotOwnsBoxOrCategoryAsync(null, request.CategoryId, cancellationToken)) return NotFound();
        var command = new ShowLaterProcessCommand(request.FlashcardId,request.CategoryId, _userId);
        await CommandDispatcher.Dispatch(command, cancellationToken);
        return Ok();
    }

    /// <summary>«بلدمش»: کارت نمایش‌داده‌شده یادگرفته‌شده می‌شود (برگشت: Box/flashcard/{id}/restore).</summary>
    [HttpPost("MarkKnown")]
    public async Task<ActionResult> MarkKnown([FromBody] MarkKnownRequest request, CancellationToken cancellationToken)
    {
        if (await NotOwnsFlashcardAsync(request.FlashcardId, cancellationToken)) return NotFound();
        if (await NotOwnsBoxOrCategoryAsync(null, request.CategoryId, cancellationToken)) return NotFound();
        await CommandDispatcher.Dispatch(new MarkKnownCommand(request.FlashcardId, request.CategoryId, _userId), cancellationToken);
        return Ok();
    }

    /// <summary>«N کارت عقب‌افتادهٔ دیگر هم بخوانم»: به سهم بدهی امروز جعبه اضافه می‌شود.</summary>
    [HttpPost("BacklogMore")]
    public async Task<ActionResult> BacklogMore([FromBody] BacklogMoreRequest request, CancellationToken cancellationToken)
    {
        if (await NotOwnsBoxAsync(request.BoxId, cancellationToken)) return NotFound();
        await CommandDispatcher.Dispatch(new AddBacklogBonusCommand(request.BoxId, request.Amount), cancellationToken);
        return Ok();
    }
    
}