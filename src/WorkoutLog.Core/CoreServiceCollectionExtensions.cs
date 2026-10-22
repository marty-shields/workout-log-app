using Microsoft.Extensions.DependencyInjection;
using SQLite;
using WorkoutLog.Core.Data;
using WorkoutLog.Core.Domain.Exercises;

namespace WorkoutLog.Core;

/// <summary>Registers Core's services.</summary>
public static class CoreServiceCollectionExtensions
{
    /// <summary>Registers the database connection, the exercise catalog and the exercise seeder as singletons.</summary>
    /// <param name="services">Service collection.</param>
    /// <param name="databasePath">Full path of the SQLite database file.</param>
    public static IServiceCollection AddCore(this IServiceCollection services, string databasePath) =>
        services
            .AddSingleton(_ => new SQLiteAsyncConnection(databasePath))
            .AddSingleton<IExerciseCatalog, ExerciseCatalog>()
            .AddSingleton<IExerciseSeeder, ExerciseSeeder>();
}
