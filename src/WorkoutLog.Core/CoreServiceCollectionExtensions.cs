using Microsoft.Extensions.DependencyInjection;
using SQLite;
using WorkoutLog.Core.Data;
using WorkoutLog.Core.Domain.Exercises;
using WorkoutLog.Core.Domain.Workouts.Validation;
using WorkoutLog.Core.Services;

namespace WorkoutLog.Core;

/// <summary>Registers Core's services.</summary>
public static class CoreServiceCollectionExtensions
{
    /// <summary>
    /// Registers the database connection, exercise catalog, exercise seeder, database initializer, workout repository,
    /// system time, workout validator, workout service and history service as singletons.
    /// The initializer and repository run through one <see cref="DatabaseGate"/>.
    /// Services log through <c>ILogger&lt;T&gt;</c>, so the host must register logging.
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
            .AddSingleton<IWorkoutRepository, GatedWorkoutRepository>()
            .AddSingleton(TimeProvider.System)
            .AddSingleton<IWorkoutValidator, WorkoutValidator>()
            .AddSingleton<IWorkoutService, WorkoutService>()
            .AddSingleton<IHistoryService, HistoryService>();
}
