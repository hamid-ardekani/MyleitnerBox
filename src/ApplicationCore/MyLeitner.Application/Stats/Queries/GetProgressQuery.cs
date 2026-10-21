using Dela.Toolkit.Application;
using MyLeitner.Application.Stats.Queries.Models;

namespace MyLeitner.Application.Stats.Queries;

/// <summary>«پیشرفت من»: امروز / این هفته / این ماه در برابر هدف و گذشتهٔ خود کاربر - همهٔ جعبه‌ها.</summary>
public record GetProgressQuery(long UserId) : IQuery;

public class GetProgressQueryHandler(IStatsReadService service) : IQueryHandler<GetProgressQuery, ProgressDto>
{
    public Task<ProgressDto> Handle(GetProgressQuery query, CancellationToken cancellationToken)
    {
        return service.GetProgressAsync(query.UserId, DateTime.UtcNow, cancellationToken);
    }
}
