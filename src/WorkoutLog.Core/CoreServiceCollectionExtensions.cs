using Microsoft.Extensions.DependencyInjection;
using SQLite;
using WorkoutLog.Core.Data;
using WorkoutLog.Core.Domain.Exercises;
using WorkoutLog.Core.Domain.Exercises.Bests;
using WorkoutLog.Core.Domain.Exercises.Progress;
using WorkoutLog.Core.Domain.Workouts;
using WorkoutLog.Core.Domain.Workouts.Consistency;
using WorkoutLog.Core.Domain.Workouts.Validation;
using WorkoutLog.Core.Import;
using WorkoutLog.Core.Import.Checks;
using WorkoutLog.Core.Import.Upgrades;
using WorkoutLog.Core.Services;

namespace WorkoutLog.Core;

/// <summary>Registers Core's services.</summary>
public static class CoreServiceCollectionExtensions
{
    /// <summary>
    /// Registers Core's services as singletons. The initializer, repository and unit of work are registered only as gated decorators
    /// that run each call through one <see cref="DatabaseGate"/>; the connection and exercise seeder are not registered.
    /// The host must register logging, <see cref="IExportEnvironment"/> and <see cref="IElapsedClock"/>.
    /// </summary>
    /// <param name="services">Service collection.</param>
    /// <param name="databasePath">Full path of the SQLite database file.</param>
    public static IServiceCollection AddCore(this IServiceCollection services, string databasePath) =>
        services
            .AddDatabase(databasePath)
            .AddSchemaUpgrades()
            .AddImport()
            .AddServices();

    /// <summary>The gated initializer, repository and unit of work share one connection to <paramref name="databasePath"/>.</summary>
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

    private static IServiceCollection AddSchemaUpgrades(this IServiceCollection services) =>
        services
            .AddSingleton<ISchemaUpgrade, WorkoutStartedAtUniqueIndex>()
            .AddSingleton<ISchemaUpgrade, BlankAssistToZero>()
            .AddSingleton<ISchemaUpgrade, ValidationRulesSchema>();

    private static IServiceCollection AddImport(this IServiceCollection services) =>
        services
            .AddSingleton<IFormatUpgrade, ValidationRulesFormatUpgrade>()
            .AddSingleton<IImportFileReader, ImportFileReader>()
            .AddImportChecks()
            .AddSingleton<IImportChecker, ImportChecker>()
            .AddSingleton<IWorkoutContentComparer, WorkoutContentComparer>();

    private static IServiceCollection AddImportChecks(this IServiceCollection services) =>
        services
            .AddSingleton<IImportCheck, StartCheck>()
            .AddSingleton<IImportCheck, RepeatedStartCheck>()
            .AddSingleton<IImportCheck, FinishCheck>()
            .AddSingleton<IImportCheck, UnknownExerciseCheck>()
            .AddSingleton<IImportCheck, ExerciseDefinitionCheck>()
            .AddSingleton<IImportCheck, RepeatedExerciseCheck>()
            .AddSingleton<IImportCheck, SaveRulesCheck>();

    private static IServiceCollection AddBestRules(this IServiceCollection services) =>
        services
            .AddSingleton<IBestRule, LiftBestRule>()
            .AddSingleton<IBestRule, BodyweightBestRule>()
            .AddSingleton<IBestRule, AssistedBestRule>()
            .AddSingleton<IBestRule, HoldBestRule>()
            .AddSingleton<IBestRule, CarryBestRule>()
            .AddSingleton<IBestRule, CardioBestRule>()
            .AddSingleton<IRecordCheck, RecordCheck>();

    /// <summary>Registers the Exercise page's range filter and each category's rules.</summary>
    private static IServiceCollection AddProgressRules(this IServiceCollection services) =>
        services
            .AddSingleton<IRangeFilter, RangeFilter>()
            .AddSingleton<ITopSetRule, TopSetRule>()
            .AddSingleton<IE1rmRule, E1rmRule>()
            .AddSingleton<ILiftRecordsRule, LiftRecordsRule>()
            .AddSingleton<IBodyweightRule, BodyweightRule>()
            .AddSingleton<IBodyweightRecordsRule, BodyweightRecordsRule>()
            .AddSingleton<ILeastAssistRule, LeastAssistRule>()
            .AddSingleton<IAssistedRecordsRule, AssistedRecordsRule>()
            .AddSingleton<IHoldRule, HoldRule>()
            .AddSingleton<IHoldRecordsRule, HoldRecordsRule>()
            .AddSingleton<ICarryRule, CarryRule>()
            .AddSingleton<ICarryRecordsRule, CarryRecordsRule>()
            .AddSingleton<ICardioSessionRule, CardioSessionRule>()
            .AddSingleton<ICardioRecordsRule, CardioRecordsRule>();

    /// <summary>Registers the Consistency card's rules.</summary>
    private static IServiceCollection AddConsistencyRules(this IServiceCollection services) =>
        services
            .AddSingleton<IWeekComparisonRule, WeekComparisonRule>()
            .AddSingleton<IWeekBarsRule, WeekBarsRule>()
            .AddSingleton<IStreakRule, StreakRule>();

    private static IServiceCollection AddServices(this IServiceCollection services) =>
        services
            .AddSingleton<IExerciseCatalog, ExerciseCatalog>()
            .AddSingleton(TimeProvider.System)
            .AddSingleton<IWorkoutValidator, WorkoutValidator>()
            .AddBestRules()
            .AddProgressRules()
            .AddSingleton<IWorkoutService, WorkoutService>()
            .AddSingleton<IWorkoutsService, WorkoutsService>()
            .AddSingleton<IDashboardService, DashboardService>()
            .AddSingleton<ILastWorkoutService, LastWorkoutService>()
            .AddSingleton<IRecentRecordsService, RecentRecordsService>()
            .AddConsistencyRules()
            .AddSingleton<IConsistencyService, ConsistencyService>()
            .AddSingleton<IMuscleGroupMap, MuscleGroupMap>()
            .AddSingleton<IMuscleSetsService, MuscleSetsService>()
            .AddSingleton<IWeightHistoryService, WeightHistoryService>()
            .AddSingleton<ILoggedExercisesService, LoggedExercisesService>()
            .AddSingleton<IExerciseProgressService, ExerciseProgressService>()
            .AddSingleton<ICardioProgressService, CardioProgressService>()
            .AddSingleton<IExportService, ExportService>()
            .AddSingleton<IImportService, ImportService>();
}
