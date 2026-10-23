using Microsoft.Extensions.DependencyInjection;
using WorkoutLog.UI.Models.Workout;
using WorkoutLog.UI.Navigation;

namespace WorkoutLog.UI.Tests;

public class UiServiceCollectionExtensionsTests
{
    [Test]
    public void AddUi_RegistersSingletons()
    {
        var services = new ServiceCollection().AddUi();

        Assert.That(services.Select(s => (s.ServiceType, s.ImplementationType, s.Lifetime)), Is.EquivalentTo(new[]
        {
            (typeof(IBackButton), typeof(BackButton), ServiceLifetime.Singleton),
            (typeof(ISetChecker), typeof(SetChecker), ServiceLifetime.Singleton),
            (typeof(IWorkoutChecker), typeof(WorkoutChecker), ServiceLifetime.Singleton),
        }));
    }
}
