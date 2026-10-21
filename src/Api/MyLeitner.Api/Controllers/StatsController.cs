using Dela.Toolkit.Application;
using Microsoft.AspNetCore.Mvc;
using MyLeitner.Application.Contract.Commands;
using MyLeitner.Application.Stats.Queries;
using MyLeitner.Application.Stats.Queries.Models;

namespace MyLeitner.Api.Controllers;

public class StatsController(IQueryDispatcher queryDispatcher, ICommandDispatcher commandDispatcher)
    : LeitnerController(queryDispatcher, commandDispatcher)
{
    /// <param name="period">Today / Week / Month / Year (یا 0..3) - پیش‌فرض Week.</param>
    [HttpGet]
    public async Task<ActionResult> GetStatsAsync([FromQuery] StatsPeriod period = StatsPeriod.Week, CancellationToken cancellationToken = default)
    {
        var result = await QueryDispatcher.Dispatch<GetStatsQuery, StatsDto>(new GetStatsQuery(_userId, period), cancellationToken);
        return Ok(result);
    }

    /// <summary>صفحهٔ «خانه»: پیشرفت، زنجیرهٔ روزها، دستاوردها و جعبه‌هایی که امروز کار دارند.</summary>
    [HttpGet("Home")]
    public async Task<ActionResult> GetHomeAsync(CancellationToken cancellationToken = default)
    {
        var result = await QueryDispatcher.Dispatch<GetHomeQuery, HomeDto>(new GetHomeQuery(_userId), cancellationToken);
        return Ok(result);
    }

    /// <summary>«پیشرفت من»: امروز/هفته/ماه در برابر هدف و دورهٔ قبل (تا همان نقطه).</summary>
    [HttpGet("Progress")]
    public async Task<ActionResult> GetProgressAsync(CancellationToken cancellationToken = default)
    {
        var result = await QueryDispatcher.Dispatch<GetProgressQuery, ProgressDto>(new GetProgressQuery(_userId), cancellationToken);
        return Ok(result);
    }

    /// <summary>ذخیرهٔ هدف‌های مرور (روزانه/هفتگی/ماهانه).</summary>
    [HttpPut("Goals")]
    public async Task<ActionResult> SaveGoalsAsync(ReviewGoalsDto request, CancellationToken cancellationToken = default)
    {
        await CommandDispatcher.Dispatch(new SaveReviewGoalsCommand(_userId, request.Daily, request.Weekly, request.Monthly), cancellationToken);
        return Ok();
    }

    /// <summary>«امروز چطور بود» برای پایان مرور: امروز در برابر دیروز و میانگین هفته/ماه.</summary>
    [HttpGet("Recap")]
    public async Task<ActionResult> GetRecapAsync(CancellationToken cancellationToken = default)
    {
        var result = await QueryDispatcher.Dispatch<GetRecapQuery, RecapDto>(new GetRecapQuery(_userId), cancellationToken);
        return Ok(result);
    }
}
