using MyLeitner.Application.Stats.Queries.Models;

namespace MyLeitner.Application.Stats.Queries;

public interface IStatsReadService
{
    Task<StatsDto> GetStatsAsync(long userId, StatsPeriod period, DateTime utcNow, CancellationToken cancellationToken);
    Task<RecapDto> GetRecapAsync(long userId, DateTime utcNow, CancellationToken cancellationToken);
    Task<HomeDto> GetHomeAsync(long userId, DateTime utcNow, CancellationToken cancellationToken);
    Task<ProgressDto> GetProgressAsync(long userId, DateTime utcNow, CancellationToken cancellationToken);
}
