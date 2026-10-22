using Microsoft.Extensions.DependencyInjection;
using SQLite;
using WorkoutLog.Core.Data;
using WorkoutLog.Core.Domain.Exercises;

namespace WorkoutLog.Core;

/// <summary>Registers Core's services.</summary>
public static class CoreServiceCollectionExtensions
{
    /// <summary>
    /// Registers the database connection, exercise catalog, exercise seeder, database initializer and workout repository as singletons.
    /// The initializer and repository run through one <see cref="DatabaseGate"/>.
    /// </summary>
    /// <param name="services">Service collection.</param>
    /// <param name="databasePath">Full path of the SQLite database file.</param>
    public static IServiceCollection AddCore(this IServiceCollection services, string databasePath) =>
        services
            .AddSingleton(_ => new SQLiteAsyncConnection(databasePath))
            .AddSingleton<DatabaseGate>()
            .AddSingleton<IExerciseCatalog, ExerciseCatalog>()
            .AddSingleton<IExerciseSeeder, ExerciseSeeder>()
            .AddSingleton<DatabaseInitializer>()
            .AddSingleton<IDatabaseInitializer, GatedDatabaseInitializer>()
            .AddSingleton<WorkoutRepository>()
            .AddSingleton<IWorkoutRepository, GatedWorkoutRepository>();
}
