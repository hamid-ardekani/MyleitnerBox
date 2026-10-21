using Autofac;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MyLeitner.Config;
using Serilog;

namespace CommandHandler_Test.Autofac;

public class AutofacTestFixture : IDisposable
{
    public IContainer Container { get; }

    public AutofacTestFixture()
    {
        var configuration = new ConfigurationBuilder().Build();

        var builder = new ContainerBuilder();

        builder.RegisterModule(new MyLeitnerModule(
            configuration,
            configureWriteDb: options => options.UseInMemoryDatabase("Leitner_Test"),
            configureReadDb: options => options.UseInMemoryDatabase("Leitner_Test"),
            // لاگینگ جدای از AddDependencies است - فقط برای همین تست‌ها لازم است
            // (AutofacCommandDispatcher به ILogger<T> نیاز دارد). باید در همان
            // ServiceCollection که AddDependencies استفاده می‌کند جمع شود، نه در
            // یک Populate() جدا - وگرنه EF Core فکر می‌کند چند provider هم‌زمان ثبت شده.
            configureServices: services => services.AddLogging(logging => logging.AddSerilog())));

        Container = builder.Build();
    }

    public void Dispose()
    {
        Container.Dispose();
    }
}
