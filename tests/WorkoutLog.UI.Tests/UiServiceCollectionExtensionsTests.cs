using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Shields.UI.Lifecycle;
using Shields.UI.Navigation;
using Shields.UI.Theme;
using WorkoutLog.UI.Import;
using WorkoutLog.UI.Models.Workout;

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
            Assert.That(scope.ServiceProvider.GetRequiredService<TimeProvider>(), Is.SameAs(TimeProvider.System));
        });
    }

    [Test]
    public void AddUi_GivesEachScopeTheKitsBackButtonAndOneAppLifecycle()
    {
        using var provider = new ServiceCollection()
            .AddSingleton(typeof(ILogger<>), typeof(NullLogger<>))
            .AddUi()
            .BuildServiceProvider(validateScopes: true);
        using var scope = provider.CreateScope();
        using var other = provider.CreateScope();

        var lifecycle = scope.ServiceProvider.GetRequiredService<AppLifecycle>();
        var backButton = scope.ServiceProvider.GetRequiredService<IBackButton>();

        Assert.Multiple(() =>
        {
            Assert.That(backButton, Is.TypeOf<BackButton>());
            Assert.That(scope.ServiceProvider.GetRequiredService<IBackButton>(), Is.SameAs(backButton));
            Assert.That(other.ServiceProvider.GetRequiredService<IBackButton>(), Is.Not.SameAs(backButton));
            Assert.That(scope.ServiceProvider.GetRequiredService<IAppStart>(), Is.SameAs(lifecycle));
            Assert.That(scope.ServiceProvider.GetRequiredService<IAppLifecycle>(), Is.SameAs(lifecycle));
            Assert.That(() => provider.GetRequiredService<IBackButton>(), Throws.InvalidOperationException);
        });
    }
}
