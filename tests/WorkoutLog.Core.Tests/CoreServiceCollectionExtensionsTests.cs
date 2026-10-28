using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using SQLite;
using WorkoutLog.Core.Data;
using WorkoutLog.Core.Domain.Exercises;
using WorkoutLog.Core.Domain.Exercises.Bests;
using WorkoutLog.Core.Domain.Exercises.Progress;
using WorkoutLog.Core.Domain.Workouts;
using WorkoutLog.Core.Domain.Workouts.Validation;
using WorkoutLog.Core.Import;
using WorkoutLog.Core.Import.Checks;
using WorkoutLog.Core.Import.Upgrades;
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
            (typeof(DatabaseGate), typeof(DatabaseGate), ServiceLifetime.Singleton),
            (typeof(IExerciseCatalog), typeof(ExerciseCatalog), ServiceLifetime.Singleton),
            (typeof(ISchemaUpgrade), typeof(WorkoutStartedAtUniqueIndex), ServiceLifetime.Singleton),
            (typeof(ISchemaUpgrade), typeof(BlankAssistToZero), ServiceLifetime.Singleton),
            (typeof(IDatabaseInitializer), (Type?)null, ServiceLifetime.Singleton),
            (typeof(IWorkoutRepository), (Type?)null, ServiceLifetime.Singleton),
            (typeof(TimeProvider), (Type?)null, ServiceLifetime.Singleton),
            (typeof(IWorkoutValidator), typeof(WorkoutValidator), ServiceLifetime.Singleton),
            (typeof(IWorkoutService), typeof(WorkoutService), ServiceLifetime.Singleton),
            (typeof(IWorkoutsService), typeof(WorkoutsService), ServiceLifetime.Singleton),
            (typeof(IDashboardService), typeof(DashboardService), ServiceLifetime.Singleton),
            (typeof(IWeightHistoryService), typeof(WeightHistoryService), ServiceLifetime.Singleton),
            (typeof(ILoggedExercisesService), typeof(LoggedExercisesService), ServiceLifetime.Singleton),
            (typeof(IBestRule), typeof(LiftBestRule), ServiceLifetime.Singleton),
            (typeof(IBestRule), typeof(BodyweightBestRule), ServiceLifetime.Singleton),
            (typeof(IBestRule), typeof(AssistedBestRule), ServiceLifetime.Singleton),
            (typeof(IBestRule), typeof(HoldBestRule), ServiceLifetime.Singleton),
            (typeof(IBestRule), typeof(CarryBestRule), ServiceLifetime.Singleton),
            (typeof(IBestRule), typeof(CardioBestRule), ServiceLifetime.Singleton),
            (typeof(IRangeFilter), typeof(RangeFilter), ServiceLifetime.Singleton),
            (typeof(ITopSetRule), typeof(TopSetRule), ServiceLifetime.Singleton),
            (typeof(IE1rmRule), typeof(E1rmRule), ServiceLifetime.Singleton),
            (typeof(ILiftRecordsRule), typeof(LiftRecordsRule), ServiceLifetime.Singleton),
            (typeof(IBodyweightRule), typeof(BodyweightRule), ServiceLifetime.Singleton),
            (typeof(IBodyweightRecordsRule), typeof(BodyweightRecordsRule), ServiceLifetime.Singleton),
            (typeof(ILeastAssistRule), typeof(LeastAssistRule), ServiceLifetime.Singleton),
            (typeof(IAssistedRecordsRule), typeof(AssistedRecordsRule), ServiceLifetime.Singleton),
            (typeof(IHoldRule), typeof(HoldRule), ServiceLifetime.Singleton),
            (typeof(IHoldRecordsRule), typeof(HoldRecordsRule), ServiceLifetime.Singleton),
            (typeof(ICarryRule), typeof(CarryRule), ServiceLifetime.Singleton),
            (typeof(ICarryRecordsRule), typeof(CarryRecordsRule), ServiceLifetime.Singleton),
            (typeof(ICardioSessionRule), typeof(CardioSessionRule), ServiceLifetime.Singleton),
            (typeof(ICardioRecordsRule), typeof(CardioRecordsRule), ServiceLifetime.Singleton),
            (typeof(IExerciseProgressService), typeof(ExerciseProgressService), ServiceLifetime.Singleton),
            (typeof(ICardioProgressService), typeof(CardioProgressService), ServiceLifetime.Singleton),
            (typeof(IExportService), typeof(ExportService), ServiceLifetime.Singleton),
            (typeof(IWorkoutWriterFactory), typeof(WorkoutWriterFactory), ServiceLifetime.Singleton),
            (typeof(IUnitOfWork), (Type?)null, ServiceLifetime.Singleton),
            (typeof(IImportFileReader), typeof(ImportFileReader), ServiceLifetime.Singleton),
            (typeof(IImportCheck), typeof(StartCheck), ServiceLifetime.Singleton),
            (typeof(IImportCheck), typeof(RepeatedStartCheck), ServiceLifetime.Singleton),
            (typeof(IImportCheck), typeof(FinishCheck), ServiceLifetime.Singleton),
            (typeof(IImportCheck), typeof(UnknownExerciseCheck), ServiceLifetime.Singleton),
            (typeof(IImportCheck), typeof(ExerciseDefinitionCheck), ServiceLifetime.Singleton),
            (typeof(IImportCheck), typeof(SaveRulesCheck), ServiceLifetime.Singleton),
            (typeof(IImportChecker), typeof(ImportChecker), ServiceLifetime.Singleton),
            (typeof(IWorkoutContentComparer), typeof(WorkoutContentComparer), ServiceLifetime.Singleton),
            (typeof(IImportService), typeof(ImportService), ServiceLifetime.Singleton),
        }));
    }

    [Test]
    public void AddCore_RegistersNoFormatUpgradeSteps()
    {
        using var provider = new ServiceCollection().AddCore(TempDatabase.NewPath()).BuildServiceProvider();

        Assert.That(provider.GetServices<IFormatUpgrade>(), Is.Empty);
    }

    [Test]
    public void AddCore_WithLoggingAndExportEnvironment_ResolvesThePublicServices()
    {
        using var provider = new ServiceCollection()
            .AddSingleton(typeof(ILogger<>), typeof(NullLogger<>))
            .AddSingleton(Mock.Of<IExportEnvironment>())
            .AddCore(TempDatabase.NewPath())
            .BuildServiceProvider();

        Assert.Multiple(() =>
        {
            Assert.That(provider.GetRequiredService<IDatabaseInitializer>(), Is.InstanceOf<GatedDatabaseInitializer>());
            Assert.That(provider.GetRequiredService<IWorkoutRepository>(), Is.InstanceOf<GatedWorkoutRepository>());
            Assert.That(provider.GetRequiredService<IWorkoutService>(), Is.InstanceOf<WorkoutService>());
            Assert.That(provider.GetRequiredService<IWorkoutsService>(), Is.InstanceOf<WorkoutsService>());
            Assert.That(provider.GetRequiredService<IDashboardService>(), Is.InstanceOf<DashboardService>());
            Assert.That(provider.GetRequiredService<IWeightHistoryService>(), Is.InstanceOf<WeightHistoryService>());
            Assert.That(provider.GetRequiredService<ILoggedExercisesService>(), Is.InstanceOf<LoggedExercisesService>());
            Assert.That(provider.GetRequiredService<IExerciseProgressService>(), Is.InstanceOf<ExerciseProgressService>());
            Assert.That(provider.GetRequiredService<ICardioProgressService>(), Is.InstanceOf<CardioProgressService>());
            Assert.That(provider.GetRequiredService<IExportService>(), Is.InstanceOf<ExportService>());
            Assert.That(provider.GetRequiredService<IImportService>(), Is.InstanceOf<ImportService>());
            Assert.That(provider.GetRequiredService<IUnitOfWork>(), Is.InstanceOf<GatedUnitOfWork>());
        });
    }

    [Test]
    public void AddCore_UngatedDatabaseTypesCannotBeResolved()
    {
        using var provider = new ServiceCollection().AddCore(TempDatabase.NewPath()).BuildServiceProvider();

        Assert.Multiple(() =>
        {
            Assert.That(provider.GetService<SQLiteAsyncConnection>(), Is.Null);
            Assert.That(provider.GetService<DatabaseInitializer>(), Is.Null);
            Assert.That(provider.GetService<WorkoutRepository>(), Is.Null);
            Assert.That(provider.GetService<UnitOfWork>(), Is.Null);
            Assert.That(provider.GetService<IExerciseSeeder>(), Is.Null);
            Assert.That(provider.GetService<ExerciseSeeder>(), Is.Null);
        });
    }

    [Test]
    public async Task AddCore_InitializerAndRepositoryUseTheDatabasePath()
    {
        var path = TempDatabase.NewPath();
        var start = new DateTimeOffset(2026, 10, 21, 17, 5, 0, TimeSpan.Zero);
        using var provider = new ServiceCollection()
            .AddSingleton(typeof(ILogger<>), typeof(NullLogger<>))
            .AddCore(path)
            .BuildServiceProvider();

        await provider.GetRequiredService<IDatabaseInitializer>().InitializeAsync();
        await provider.GetRequiredService<IWorkoutRepository>().AddAsync(new NewWorkout(start, start, []));

        var check = new SQLiteAsyncConnection(path);
        try
        {
            var userVersion = await check.ExecuteScalarAsync<int>("PRAGMA user_version");
            var exercises = await check.Table<Exercise>().CountAsync();
            var workouts = await check.Table<WorkoutRow>().CountAsync();
            Assert.Multiple(() =>
            {
                Assert.That(userVersion, Is.EqualTo(DatabaseInitializer.CurrentVersion));
                Assert.That(exercises, Is.EqualTo(provider.GetRequiredService<IExerciseCatalog>().All.Count()));
                Assert.That(workouts, Is.EqualTo(1));
            });
        }
        finally
        {
            await check.CloseAsync();
            File.Delete(path);
        }
    }

    [Test]
    public void AddCore_TimeIsTheSystemClock()
    {
        var time = new ServiceCollection().AddCore("workoutlog.db3")
            .Single(s => s.ServiceType == typeof(TimeProvider)).ImplementationInstance;

        Assert.That(time, Is.SameAs(TimeProvider.System));
    }
}
