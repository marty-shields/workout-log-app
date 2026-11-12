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

public static class CoreServiceCollectionExtensions
{
    /// <summary>
    /// Registers Core's services as singletons. The initializer, repository and unit of work are registered only as gated decorators
    /// that run each call through one <see cref="DatabaseGate"/>; the connection and exercise seeder are not registered.
    /// The host must register logging, <see cref="IExportEnvironment"/> and <see cref="IElapsedClock"/>.
    /// </summary>
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
            .AddSingleton<WorkoutWriterFactory>()
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
            .AddSingleton<ISchemaUpgrade, ValidationRulesSchema>()
            .AddSingleton<ISchemaUpgrade, NotesSchema>();

    private static IServiceCollection AddImport(this IServiceCollection services) =>
        services
            .AddSingleton<IFormatUpgrade, ValidationRulesFormatUpgrade>()
            .AddSingleton<IFormatUpgrade, NotesFormatUpgrade>()
            .AddSingleton<ImportFileReader>()
            .AddImportChecks()
            .AddSingleton<ImportChecker>()
            .AddSingleton<WorkoutContentComparer>();

    private static IServiceCollection AddImportChecks(this IServiceCollection services) =>
        services
            .AddSingleton<IImportCheck, StartCheck>()
            .AddSingleton<IImportCheck, RepeatedStartCheck>()
            .AddSingleton<IImportCheck, FinishCheck>()
            .AddSingleton<IImportCheck, FutureCheck>()
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
            .AddSingleton<IRecordCheck, RecordCheck>()
            .AddSingleton<IBestChangeRule, BestChangeRule>();

    /// <summary>Registers the Exercise page's range filter and each category's rules.</summary>
    private static IServiceCollection AddProgressRules(this IServiceCollection services) =>
        services
            .AddSingleton<RangeFilter>()
            .AddSingleton<TopSetRule>()
            .AddSingleton<E1rmRule>()
            .AddSingleton<LiftRecordsRule>()
            .AddSingleton<BodyweightRule>()
            .AddSingleton<BodyweightRecordsRule>()
            .AddSingleton<LeastAssistRule>()
            .AddSingleton<AssistedRecordsRule>()
            .AddSingleton<HoldRule>()
            .AddSingleton<HoldRecordsRule>()
            .AddSingleton<CarryRule>()
            .AddSingleton<CarryRecordsRule>()
            .AddSingleton<CardioSessionRule>()
            .AddSingleton<CardioRecordsRule>();

    /// <summary>Registers the Consistency card's rules.</summary>
    private static IServiceCollection AddConsistencyRules(this IServiceCollection services) =>
        services
            .AddSingleton<WeekComparisonRule>()
            .AddSingleton<WeekBarsRule>()
            .AddSingleton<StreakRule>();

    private static IServiceCollection AddServices(this IServiceCollection services) =>
        services
            .AddSingleton<IExerciseCatalog, ExerciseCatalog>()
            .AddSingleton(TimeProvider.System)
            .AddSingleton<NoteLengthRule>()
            .AddSingleton<IWorkoutValidator, WorkoutValidator>()
            .AddBestRules()
            .AddProgressRules()
            .AddSingleton<IWorkoutService, WorkoutService>()
            .AddSingleton<IWorkoutsService, WorkoutsService>()
            .AddSingleton<IWorkoutTotalsService, WorkoutTotalsService>()
            .AddSingleton<ILastWorkoutService, LastWorkoutService>()
            .AddSingleton<IRecentRecordsService, RecentRecordsService>()
            .AddSingleton<IWorkoutSummaryService, WorkoutSummaryService>()
            .AddConsistencyRules()
            .AddSingleton<IConsistencyService, ConsistencyService>()
            .AddSingleton<MuscleGroupMap>()
            .AddSingleton<IMuscleSetsService, MuscleSetsService>()
            .AddSingleton<IWeightHistoryService, WeightHistoryService>()
            .AddSingleton<ILastSessionService, LastSessionService>()
            .AddSingleton<ILoggedExercisesService, LoggedExercisesService>()
            .AddSingleton<IExerciseProgressService, ExerciseProgressService>()
            .AddSingleton<ICardioProgressService, CardioProgressService>()
            .AddSingleton<IExportService, ExportService>()
            .AddSingleton<IImportService, ImportService>();
}
