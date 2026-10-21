using Dela.Toolkit.Application;
using MyLeitner.Application.Stats.Queries.Models;

namespace MyLeitner.Application.Stats.Queries;

/// <summary>«امروز چطور بود» برای صفحهٔ پایان مرور - همهٔ جعبه‌های کاربر.</summary>
public record GetRecapQuery(long UserId) : IQuery;

public class GetRecapQueryHandler(IStatsReadService service) : IQueryHandler<GetRecapQuery, RecapDto>
{
    public Task<RecapDto> Handle(GetRecapQuery query, CancellationToken cancellationToken)
    {
        return service.GetRecapAsync(query.UserId, DateTime.UtcNow, cancellationToken);
    }
}
