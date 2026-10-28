using Microsoft.Extensions.DependencyInjection;
using SQLite;
using WorkoutLog.Core.Data;
using WorkoutLog.Core.Domain.Exercises;
using WorkoutLog.Core.Domain.Workouts;
using WorkoutLog.Core.Domain.Workouts.Validation;
using WorkoutLog.Core.Import;
using WorkoutLog.Core.Import.Checks;
using WorkoutLog.Core.Services;

namespace WorkoutLog.Core;

/// <summary>Registers Core's services.</summary>
public static class CoreServiceCollectionExtensions
{
    /// <summary>
    /// Registers the exercise catalog, database initializer, workout repository, unit of work, workout writer factory, system time, workout validator,
    /// workout service, workouts service, dashboard service, weight history service, export service, import service, the import reader, checks,
    /// checker and workout comparer, and schema upgrade steps as singletons.
    /// The initializer, repository and unit of work are registered only as their gated decorators, which run each call through one <see cref="DatabaseGate"/>.
    /// The database connection and exercise seeder are not registered; only the initializer, repository and unit of work use them.
    /// Services log through <c>ILogger&lt;T&gt;</c>, so the host must register logging. The host must also register <see cref="IExportEnvironment"/>.
    /// </summary>
    /// <param name="services">Service collection.</param>
    /// <param name="databasePath">Full path of the SQLite database file.</param>
    public static IServiceCollection AddCore(this IServiceCollection services, string databasePath) =>
        services
            .AddDatabase(databasePath)
            .AddSchemaUpgrades()
            .AddImport()
            .AddServices();

    /// <summary>
    /// Registers the gate, the writer factory, and the gated initializer, repository and unit of work, which share one connection to <paramref name="databasePath"/>.
    /// </summary>
    private static IServiceCollection AddDatabase(this IServiceCollection services, string databasePath)
    {
        var connection = new SQLiteAsyncConnection(databasePath);
        return services
            .AddSingleton<DatabaseGate>()
            .AddSingleton<IWorkoutWriterFactory, WorkoutWriterFactory>()
            .AddDecoratedSingleton<IDatabaseInitializer, GatedDatabaseInitializer>(provider =>
                ActivatorUtilities.CreateInstance<DatabaseInitializer>(provider, connection,
                    ActivatorUtilities.CreateInstance<ExerciseSeeder>(provider, connection)))
            .AddDecoratedSingleton<IWorkoutRepository, GatedWorkoutRepository>(provider =>
                ActivatorUtilities.CreateInstance<WorkoutRepository>(provider, connection))
            .AddDecoratedSingleton<IUnitOfWork, GatedUnitOfWork>(provider =>
                ActivatorUtilities.CreateInstance<UnitOfWork>(provider, connection));
    }

    /// <summary>Registers the schema upgrade steps.</summary>
    private static IServiceCollection AddSchemaUpgrades(this IServiceCollection services) =>
        services.AddSingleton<ISchemaUpgrade, WorkoutStartedAtUniqueIndex>();

    /// <summary>Registers the import file reader, each import check, the checker and the workout comparer. There are no format upgrade steps yet.</summary>
    private static IServiceCollection AddImport(this IServiceCollection services) =>
        services
            .AddSingleton<IImportFileReader, ImportFileReader>()
            .AddImportChecks()
            .AddSingleton<IImportChecker, ImportChecker>()
            .AddSingleton<IWorkoutContentComparer, WorkoutContentComparer>();

    /// <summary>Registers each import check.</summary>
    private static IServiceCollection AddImportChecks(this IServiceCollection services) =>
        services
            .AddSingleton<IImportCheck, StartCheck>()
            .AddSingleton<IImportCheck, RepeatedStartCheck>()
            .AddSingleton<IImportCheck, FinishCheck>()
            .AddSingleton<IImportCheck, UnknownExerciseCheck>()
            .AddSingleton<IImportCheck, ExerciseDefinitionCheck>()
            .AddSingleton<IImportCheck, SaveRulesCheck>();

    /// <summary>Registers the exercise catalog, system time, workout validator and the services.</summary>
    private static IServiceCollection AddServices(this IServiceCollection services) =>
        services
            .AddSingleton<IExerciseCatalog, ExerciseCatalog>()
            .AddSingleton(TimeProvider.System)
            .AddSingleton<IWorkoutValidator, WorkoutValidator>()
            .AddSingleton<IWorkoutService, WorkoutService>()
            .AddSingleton<IWorkoutsService, WorkoutsService>()
            .AddSingleton<IDashboardService, DashboardService>()
            .AddSingleton<IWeightHistoryService, WeightHistoryService>()
            .AddSingleton<IExportService, ExportService>()
            .AddSingleton<IImportService, ImportService>();
}
