using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Shields.UI.Theme;
using WorkoutLog.UI.Import;
using WorkoutLog.UI.Lifecycle;
using WorkoutLog.UI.Models.Workout;
using WorkoutLog.UI.Navigation;

namespace WorkoutLog.UI.Tests;

public class UiServiceCollectionExtensionsTests
{
    [Test]
    public void AddUi_RegistersTheServices()
    {
        var services = new ServiceCollection().AddUi();

        // The kit's own registrations (AddShieldsUi) are the kit's to test.
        var ours = services.Where(s => s.ServiceType.Assembly == typeof(UiServiceCollectionExtensions).Assembly);

        Assert.That(ours.Select(s => (s.ServiceType, s.ImplementationType, s.Lifetime)), Is.EquivalentTo(new[]
        {
            (typeof(ImportSession), typeof(ImportSession), ServiceLifetime.Singleton),
            (typeof(IImportFileOpener), typeof(ImportFileOpener), ServiceLifetime.Singleton),
            (typeof(IBackButton), typeof(BackButton), ServiceLifetime.Singleton),
            (typeof(AppLifecycle), typeof(AppLifecycle), ServiceLifetime.Singleton),
            (typeof(IAppStart), (Type?)null, ServiceLifetime.Singleton),
            (typeof(IAppLifecycle), (Type?)null, ServiceLifetime.Singleton),
            (typeof(ISetChecker), typeof(SetChecker), ServiceLifetime.Singleton),
            (typeof(IWorkoutChecker), typeof(WorkoutChecker), ServiceLifetime.Singleton),
            (typeof(LastTimeLoader), typeof(LastTimeLoader), ServiceLifetime.Singleton),
            (typeof(WorkoutEditor), typeof(WorkoutEditor), ServiceLifetime.Transient),
        }));
    }

    [Test]
    public void AddUi_RegistersTheKitsThemeAndSystemBarServices()
    {
        using var context = new BunitContext();
        using var provider = new ServiceCollection()
            .AddSingleton(typeof(ILogger<>), typeof(NullLogger<>))
            .AddSingleton(context.JSInterop.JSRuntime)
            .AddUi()
            .BuildServiceProvider();
        using var scope = provider.CreateScope();

        Assert.Multiple(() =>
        {
            Assert.That(scope.ServiceProvider.GetRequiredService<ThemeService>(), Is.Not.Null);
            Assert.That(scope.ServiceProvider.GetRequiredService<SystemBarInsetWriter>(), Is.Not.Null);
        });
    }

    [Test]
    public void AddUi_AppStartAndAppLifecycleAreOneInstance()
    {
        using var provider = new ServiceCollection()
            .AddSingleton(typeof(ILogger<>), typeof(NullLogger<>))
            .AddUi()
            .BuildServiceProvider();

        var lifecycle = provider.GetRequiredService<AppLifecycle>();

        Assert.Multiple(() =>
        {
            Assert.That(provider.GetRequiredService<IAppStart>(), Is.SameAs(lifecycle));
            Assert.That(provider.GetRequiredService<IAppLifecycle>(), Is.SameAs(lifecycle));
        });
    }
}
