using Dela.Toolkit.Application;
using Dela.Toolkit.ServiceHost;
using Microsoft.AspNetCore.Mvc; 
using MyLeitner.Application.Categories.Queries;
using MyLeitner.Application.Contract.Commands;

namespace MyLeitner.Api.Controllers;

public class CategoryController(IQueryDispatcher queryDispatcher, ICommandDispatcher commandDispatcher)
    : LeitnerController(queryDispatcher, commandDispatcher)
{    
    #region Categories

    [HttpGet]
    public async Task<ActionResult> GetCategoriesAsync(CancellationToken cancellationToken)
    {
        var result = await QueryDispatcher.Dispatch<GetCategoriesQuery, List<CategoryListItemDto>>(new(_userId), cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult> GetCategoryAsync(long id, CancellationToken cancellationToken)
    {
        if (await NotOwnsCategoryAsync(id, cancellationToken)) return NotFound();
        var result = await QueryDispatcher.Dispatch<GetCategoryQuery, CategoryForEditDto>(new(id), cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult> AddCategoryAsync(AddCategoryCommand request, CancellationToken cancellationToken)
    {
        if (await NotOwnsBoxesAsync(request.BoxIds, cancellationToken)) return NotFound();
        request = request with { UserId = _userId };
        await CommandDispatcher.Dispatch(request, cancellationToken);
        return Ok();
    }

    [HttpPut]
    public async Task<ActionResult> UpdateCategoryAsync(UpdateCategoryCommand request, CancellationToken cancellationToken)
    {
        if (await NotOwnsCategoryAsync(request.Id, cancellationToken)) return NotFound();
        if (await NotOwnsBoxesAsync(request.BoxIds, cancellationToken)) return NotFound();
        request = request with { UserId = _userId };
        await CommandDispatcher.Dispatch(request, cancellationToken);
        return Ok();
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteCategoryAsync(long id, CancellationToken cancellationToken)
    {
        if (await NotOwnsCategoryAsync(id, cancellationToken)) return NotFound();
        await CommandDispatcher.Dispatch(new DeleteCategoryCommand(id), cancellationToken);
        return Ok();
    }

    #endregion
}