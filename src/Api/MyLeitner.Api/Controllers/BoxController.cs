using Dela.Toolkit.Application;
using Dela.Toolkit.ServiceHost;
using Microsoft.AspNetCore.Mvc; 
using MyLeitner.Application.Boxes.Queries;
using MyLeitner.Application.Boxes.Queries.Models;
using MyLeitner.Application.Contract.Commands;
using MyLeitner.Application.Flashcards.Queries;

namespace MyLeitner.Api.Controllers;

public class BoxController(IQueryDispatcher queryDispatcher, ICommandDispatcher commandDispatcher)
    : LeitnerController(queryDispatcher, commandDispatcher)
{
    #region Boxes

    [HttpGet]
    public async Task<ActionResult> GetBoxesAsync(CancellationToken cancellationToken)
    {
        var result = await QueryDispatcher.Dispatch<GetBoxesQuery, List<BoxListItemDto>>(new(_userId), cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult> GetBoxAsync(long id, CancellationToken cancellationToken)
    {
        if (await NotOwnsBoxAsync(id, cancellationToken)) return NotFound();
        var result = await QueryDispatcher.Dispatch<GetBoxQuery, BoxForEditDto>(new(id), cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult> AddBoxAsync(AddBoxCommand request, CancellationToken cancellationToken)
    {
        request = request with { UserId = _userId };
        await CommandDispatcher.Dispatch(request, cancellationToken);
        return Ok();
    }

    [HttpPut]
    public async Task<ActionResult> UpdateBoxAsync(UpdateBoxCommand request, CancellationToken cancellationToken)
    {
        if (await NotOwnsBoxAsync(request.Id, cancellationToken)) return NotFound();
        await CommandDispatcher.Dispatch(request, cancellationToken);
        return Ok();
    }
    
    [HttpPut("Order")]
    public async Task<ActionResult> UpdateBoxAsync(UpdateOrderBoxCommand request, CancellationToken cancellationToken)
    {
        if (await NotOwnsBoxAsync(request.Id, cancellationToken)) return NotFound();
        request = request with { UserId = _userId };
        await CommandDispatcher.Dispatch(request, cancellationToken);
        return Ok();
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteBoxAsync(long id, CancellationToken cancellationToken)
    {
        if (await NotOwnsBoxAsync(id, cancellationToken)) return NotFound();
        await CommandDispatcher.Dispatch(new DeleteBoxCommand(id), cancellationToken);
        return Ok();
    }

    #endregion

    #region Flashcards

    [HttpGet("{boxId}/flashcard")]
    public async Task<ActionResult> GetFlashcardsAsync(long boxId, [FromQuery] int page, [FromQuery] int limit, CancellationToken cancellationToken)
    {
        if (await NotOwnsBoxAsync(boxId, cancellationToken)) return NotFound();
        var query = new GetFlashcardsQuery(boxId, page, limit);
        var result = await QueryDispatcher.Dispatch<GetFlashcardsQuery, List<FlashcardListItemDto>>(query, cancellationToken);
        return Ok(result);
    }
    
    [HttpGet("{boxId}/flashcard/{flashcardId}")]
    public async Task<ActionResult> GetFlashcardAsync(long flashcardId, CancellationToken cancellationToken)
    {
        if (await NotOwnsFlashcardAsync(flashcardId, cancellationToken)) return NotFound();
        var query = new GetFlashcardQuery(flashcardId);
        var result = await QueryDispatcher.Dispatch<GetFlashcardQuery, FlashcardForSaveDto>(query, cancellationToken);
        return Ok(result);
    }

    [HttpPost("{boxId}/flashcard")]
    public async Task<ActionResult> AddFlashcardAsync(long boxId, [FromBody] FlashcardForSaveDto request, CancellationToken cancellationToken)
    {
        if (await NotOwnsBoxAsync(boxId, cancellationToken)) return NotFound();
        var command = new AddFlashcardCommand(boxId, request.Front, request.Back, request.FrontSearchText, request.BackSearchText, request.Tags, _userId);
        await CommandDispatcher.Dispatch(command, cancellationToken);
        return Ok();
    }

    [HttpPut("{boxId}/flashcard")]
    public async Task<ActionResult> UpdateFlashcardAsync(long boxId, FlashcardForSaveDto request, CancellationToken cancellationToken)
    {
        if (await NotOwnsFlashcardAsync(request.FlashcardId, cancellationToken)) return NotFound();
        if (request.MoveToBoxId is { } moveTo && await NotOwnsBoxAsync(moveTo, cancellationToken)) return NotFound();
        var command = new UpdateFlashcardCommand(request.FlashcardId, request.Front, request.Back, request.FrontSearchText, request.BackSearchText, request.Tags, _userId, request.MoveToBoxId);
        await CommandDispatcher.Dispatch(command, cancellationToken);
        return Ok();
    }

    [HttpDelete("{boxId}/flashcard/{flashcardId}")]
    public async Task<ActionResult> DeleteFlashcardAsync(long boxId, long flashcardId, CancellationToken cancellationToken)
    {
        if (await NotOwnsFlashcardAsync(flashcardId, cancellationToken)) return NotFound();
        await CommandDispatcher.Dispatch(new DeleteFlashcardCommand(flashcardId), cancellationToken);
        return Ok();
    }

    /// <summary>کارت را دستی از مرور بیرون می‌گذارد (تاریخچه‌اش می‌ماند).</summary>
    [HttpPost("{boxId}/flashcard/{flashcardId}/archive")]
    public async Task<ActionResult> ArchiveFlashcardAsync(long boxId, long flashcardId, CancellationToken cancellationToken)
    {
        if (await NotOwnsFlashcardAsync(flashcardId, cancellationToken)) return NotFound();
        await CommandDispatcher.Dispatch(new ArchiveFlashcardCommand(flashcardId), cancellationToken);
        return Ok();
    }

    /// <summary>کارت بایگانی‌شده یا یادگرفته‌شده را به مرور برمی‌گرداند (همین امروز موعدش می‌رسد).</summary>
    [HttpPost("{boxId}/flashcard/{flashcardId}/restore")]
    public async Task<ActionResult> RestoreFlashcardAsync(long boxId, long flashcardId, CancellationToken cancellationToken)
    {
        if (await NotOwnsFlashcardAsync(flashcardId, cancellationToken)) return NotFound();
        await CommandDispatcher.Dispatch(new RestoreFlashcardCommand(flashcardId), cancellationToken);
        return Ok();
    }

    /// <summary>کارت جدید را همین امروز به مرور این جعبه اضافه می‌کند (از سهم کارت جدید امروز کم می‌شود).</summary>
    [HttpPost("{boxId}/flashcard/{flashcardId}/add-to-review")]
    public async Task<ActionResult> AddFlashcardToReviewAsync(long boxId, long flashcardId, CancellationToken cancellationToken)
    {
        if (await NotOwnsFlashcardAsync(flashcardId, cancellationToken)) return NotFound();
        await CommandDispatcher.Dispatch(new AddFlashcardToReviewCommand(flashcardId), cancellationToken);
        return Ok();
    }

    #endregion
}