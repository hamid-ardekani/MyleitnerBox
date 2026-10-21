using System.Threading.RateLimiting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.RateLimiting;

namespace MyLeitner.Api;

/// <summary>سقف درخواست به endpointهای ورود/ثبت‌نام برای هر IP (جلوی ساخت انبوه کاربر و حدس رمز).</summary>
public static class AuthRateLimiting
{
    private static readonly (string Path, string Key, int Permits, TimeSpan Window)[] Rules =
    [
        ("/api/Auth/Register", "register", 5, TimeSpan.FromHours(1)),
        ("/api/Auth/Login", "login", 10, TimeSpan.FromMinutes(1)),
        ("/api/Auth/Refresh", "refresh", 30, TimeSpan.FromMinutes(1)),
    ];

    public static IServiceCollection AddAuthRateLimiting(this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.OnRejected = async (context, ct) =>
            {
                if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
                    context.HttpContext.Response.Headers.RetryAfter = ((int)retryAfter.TotalSeconds).ToString();
                await context.HttpContext.Response.WriteAsJsonAsync(new { error = "too_many_requests" }, ct);
            };

            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(http =>
            {
                var rule = Rules.FirstOrDefault(r => http.Request.Path.Equals(r.Path, StringComparison.OrdinalIgnoreCase));
                if (rule.Path == null || !HttpMethods.IsPost(http.Request.Method))
                    return RateLimitPartition.GetNoLimiter("none");

                var ip = http.Connection.RemoteIpAddress?.ToString() ?? "unknown";
                return RateLimitPartition.GetFixedWindowLimiter($"{rule.Key}:{ip}", _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = rule.Permits,
                    Window = rule.Window,
                    QueueLimit = 0,
                });
            });
        });

        return services;
    }
}

/// <summary>IP واقعی کلاینت از X-Forwarded-For خوانده می‌شود، فقط از پروکسی مورد اعتماد (پیش‌فرض: همین ماشین).
/// قبل از <c>UseRouting</c> قرار می‌گیرد.</summary>
public class AuthRateLimitingStartupFilter : IStartupFilter
{
    public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next) => app =>
    {
        app.UseForwardedHeaders(new ForwardedHeadersOptions { ForwardedHeaders = ForwardedHeaders.XForwardedFor });
        app.UseRateLimiter();
        next(app);
    };
}
