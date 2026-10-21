using Dela.Toolkit.Application;
using MyLeitner.Application.Stats.Queries.Models;

namespace MyLeitner.Application.Stats.Queries;

public record GetStatsQuery(long UserId, StatsPeriod Period) : IQuery;

public class GetStatsQueryHandler : IQueryHandler<GetStatsQuery, StatsDto>
{
    private readonly IStatsReadService _service;

    public GetStatsQueryHandler(IStatsReadService service)
    {
        _service = service;
    }

    public Task<StatsDto> Handle(GetStatsQuery query, CancellationToken cancellationToken)
    {
        return _service.GetStatsAsync(query.UserId, query.Period, DateTime.UtcNow, cancellationToken);
    }
}
