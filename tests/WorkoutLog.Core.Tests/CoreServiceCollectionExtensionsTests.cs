using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using SQLite;
using WorkoutLog.Core.Data;
using WorkoutLog.Core.Domain.Exercises;
using WorkoutLog.Core.Domain.Workouts.Validation;
using WorkoutLog.Core.Services;

namespace WorkoutLog.Core.Tests;

public class CoreServiceCollectionExtensionsTests
{
    [Test]
    public void AddCore_RegistersSingletons()
    {
        var services = new ServiceCollection().AddCore("workoutlog.db3");

        Assert.That(services.Select(s => (s.ServiceType, s.ImplementationType, s.Lifetime)), Is.EquivalentTo(new[]
        {
            (typeof(SQLiteAsyncConnection), (Type?)null, ServiceLifetime.Singleton),
            (typeof(DatabaseGate), typeof(DatabaseGate), ServiceLifetime.Singleton),
            (typeof(IExerciseCatalog), typeof(ExerciseCatalog), ServiceLifetime.Singleton),
            (typeof(IExerciseSeeder), typeof(ExerciseSeeder), ServiceLifetime.Singleton),
            (typeof(DatabaseInitializer), typeof(DatabaseInitializer), ServiceLifetime.Singleton),
            (typeof(IDatabaseInitializer), typeof(GatedDatabaseInitializer), ServiceLifetime.Singleton),
            (typeof(WorkoutRepository), typeof(WorkoutRepository), ServiceLifetime.Singleton),
            (typeof(IWorkoutRepository), typeof(GatedWorkoutRepository), ServiceLifetime.Singleton),
            (typeof(TimeProvider), (Type?)null, ServiceLifetime.Singleton),
            (typeof(IWorkoutValidator), typeof(WorkoutValidator), ServiceLifetime.Singleton),
            (typeof(IWorkoutService), typeof(WorkoutService), ServiceLifetime.Singleton),
            (typeof(IHistoryService), typeof(HistoryService), ServiceLifetime.Singleton),
            (typeof(IDashboardService), typeof(DashboardService), ServiceLifetime.Singleton),
        }));
    }

    [Test]
    public void AddCore_WithLogging_ResolvesThePublicServices()
    {
        using var provider = new ServiceCollection()
            .AddSingleton(typeof(ILogger<>), typeof(NullLogger<>))
            .AddCore(TempDatabase.NewPath())
            .BuildServiceProvider();

        Assert.Multiple(() =>
        {
            Assert.That(provider.GetRequiredService<IDatabaseInitializer>(), Is.InstanceOf<GatedDatabaseInitializer>());
            Assert.That(provider.GetRequiredService<IWorkoutService>(), Is.InstanceOf<WorkoutService>());
            Assert.That(provider.GetRequiredService<IHistoryService>(), Is.InstanceOf<HistoryService>());
            Assert.That(provider.GetRequiredService<IDashboardService>(), Is.InstanceOf<DashboardService>());
        });
    }

    [Test]
    public void AddCore_ConnectionUsesTheDatabasePath()
    {
        var path = TempDatabase.NewPath();
        var factory = new ServiceCollection().AddCore(path)
            .Single(s => s.ServiceType == typeof(SQLiteAsyncConnection)).ImplementationFactory!;

        var connection = (SQLiteAsyncConnection)factory(null!);

        Assert.That(connection.DatabasePath, Is.EqualTo(path));
    }

    [Test]
    public void AddCore_TimeIsTheSystemClock()
    {
        var time = new ServiceCollection().AddCore("workoutlog.db3")
            .Single(s => s.ServiceType == typeof(TimeProvider)).ImplementationInstance;

        Assert.That(time, Is.SameAs(TimeProvider.System));
    }
}
