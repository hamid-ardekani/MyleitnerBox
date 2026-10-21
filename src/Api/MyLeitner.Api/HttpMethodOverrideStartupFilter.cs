namespace MyLeitner.Api;

/// <summary>
/// PUT/DELETE را که وب به شکل POST + هدر <c>X-HTTP-Method-Override</c> می‌فرستد، قبل از routing به متد اصلی برمی‌گرداند.
/// دلیل: WebDAV در IIS هاست production درخواست PUT/DELETE واقعی را با ۴۰۵ رد می‌کند.
/// </summary>
/// <remarks>
/// StartupFilter است نه <c>UseHttpMethodOverride()</c> در Program.cs، چون <c>CreateApp</c> در Dela.Toolkit
/// خودش <c>UseRouting()</c> را صدا می‌زند و override باید قبل از انتخاب endpoint اجرا شود.
/// </remarks>
public class HttpMethodOverrideStartupFilter : IStartupFilter
{
    public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next) => app =>
    {
        app.UseHttpMethodOverride(); // POST only, header X-HTTP-Method-Override
        next(app);
    };
}
