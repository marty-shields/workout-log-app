using Microsoft.Extensions.DependencyInjection;
using SQLite;
using WorkoutLog.Core.Data;
using WorkoutLog.Core.Domain.Exercises;

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
            (typeof(IExerciseCatalog), typeof(ExerciseCatalog), ServiceLifetime.Singleton),
            (typeof(IExerciseSeeder), typeof(ExerciseSeeder), ServiceLifetime.Singleton),
        }));
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
}
