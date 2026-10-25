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
    /// Registers the exercise catalog, database initializer, workout repository, system time, workout validator,
    /// workout service, history service and dashboard service as singletons.
    /// The initializer and repository are registered only as their gated decorators, which run each call through one <see cref="DatabaseGate"/>.
    /// The database connection and exercise seeder are not registered; only the initializer and repository use them.
    /// Services log through <c>ILogger&lt;T&gt;</c>, so the host must register logging.
    /// </summary>
    /// <param name="services">Service collection.</param>
    /// <param name="databasePath">Full path of the SQLite database file.</param>
    public static IServiceCollection AddCore(this IServiceCollection services, string databasePath)
    {
        var connection = new SQLiteAsyncConnection(databasePath);
        return services
            .AddSingleton<DatabaseGate>()
            .AddSingleton<IExerciseCatalog, ExerciseCatalog>()
            .AddDecoratedSingleton<IDatabaseInitializer, GatedDatabaseInitializer>(provider =>
                ActivatorUtilities.CreateInstance<DatabaseInitializer>(provider, connection,
                    ActivatorUtilities.CreateInstance<ExerciseSeeder>(provider, connection)))
            .AddDecoratedSingleton<IWorkoutRepository, GatedWorkoutRepository>(provider =>
                ActivatorUtilities.CreateInstance<WorkoutRepository>(provider, connection))
            .AddSingleton(TimeProvider.System)
            .AddSingleton<IWorkoutValidator, WorkoutValidator>()
            .AddSingleton<IWorkoutService, WorkoutService>()
            .AddSingleton<IHistoryService, HistoryService>()
            .AddSingleton<IDashboardService, DashboardService>();
    }
}
