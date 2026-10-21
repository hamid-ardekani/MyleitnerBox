using Dela.Toolkit.Application;
using MyLeitner.Application.Stats.Queries.Models;

namespace MyLeitner.Application.Stats.Queries;

/// <summary>صفحهٔ «خانه»: پیشرفت، زنجیرهٔ روزها، دستاوردها و جعبه‌هایی که امروز کار دارند.</summary>
public record GetHomeQuery(long UserId) : IQuery;

public class GetHomeQueryHandler(IStatsReadService service) : IQueryHandler<GetHomeQuery, HomeDto>
{
    public Task<HomeDto> Handle(GetHomeQuery query, CancellationToken cancellationToken)
    {
        return service.GetHomeAsync(query.UserId, DateTime.UtcNow, cancellationToken);
    }
}
