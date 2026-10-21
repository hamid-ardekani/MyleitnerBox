using Autofac;
using Autofac.Extensions.DependencyInjection;
using Dela.Toolkit.Application;
using Dela.Toolkit.Autofac;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MyLeitner.Config;

/// <summary>نقطهٔ ترکیب (composition root) برای جاهایی که به <see cref="IContainer"/> Autofac نیاز دارند (مثل تست‌ها).</summary>
/// <remarks>
/// همهٔ ثبت‌ها از همان <see cref="DependencyInjection.AddDependencies"/> که API واقعی هم استفاده می‌کند گرفته می‌شود (با <c>Populate</c>)؛
/// این کلاس فقط یک wrapper نازک است.
/// </remarks>
public class MyLeitnerModule : Module
{
    private readonly IConfiguration _configuration;
    private readonly Action<DbContextOptionsBuilder>? _configureWriteDb;
    private readonly Action<DbContextOptionsBuilder>? _configureReadDb;
    private readonly Action<IServiceCollection>? _configureServices;

    public MyLeitnerModule(
        IConfiguration? configuration = null,
        Action<DbContextOptionsBuilder>? configureWriteDb = null,
        Action<DbContextOptionsBuilder>? configureReadDb = null,
        Action<IServiceCollection>? configureServices = null)
    {
        _configuration = configuration ?? new ConfigurationBuilder().Build();
        _configureWriteDb = configureWriteDb;
        _configureReadDb = configureReadDb;
        _configureServices = configureServices;
    }

    protected override void Load(ContainerBuilder builder)
    {
        builder.LoadAggregate();

        // همه‌چیز (از جمله logging در تست‌ها) باید در همین یک ServiceCollection جمع شود و فقط یک‌بار Populate شود؛
        // دو بار Populate از دو ServiceCollection جدا باعث خطای «چند provider ثبت شده» در EF Core می‌شود.
        var services = new ServiceCollection();
        _configureServices?.Invoke(services);
        services.AddDependencies(_configuration, _configureWriteDb, _configureReadDb);
        builder.Populate(services);

        // AutofacCommandDispatcher باید بعد از Populate ثبت شود تا جای نسخهٔ MS-DI را بگیرد.
        // عمداً InstancePerLifetimeScope نه SingleInstance: SingleInstance در scope ریشه ساخته می‌شود و ILifetimeScope تزریق‌شده به آن
        // به DbContext ریشه وصل می‌ماند، نه DbContext همان scope تست؛ در نتیجه داده‌ی کهنهٔ tracked خوانده می‌شود.
        builder.RegisterType<AutofacCommandDispatcher>().As<ICommandDispatcher>().InstancePerLifetimeScope();
    }
}
