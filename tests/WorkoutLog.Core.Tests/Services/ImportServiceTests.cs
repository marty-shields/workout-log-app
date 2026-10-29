using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using Moq;
using SQLite;
using WorkoutLog.Core.Data;
using WorkoutLog.Core.Domain.Exercises;
using WorkoutLog.Core.Domain.Workouts;
using WorkoutLog.Core.Domain.Workouts.Validation;
using WorkoutLog.Core.Import;
using WorkoutLog.Core.Import.Checks;
using WorkoutLog.Core.Import.Upgrades;
using WorkoutLog.Core.Logging;
using WorkoutLog.Core.Services;
using static WorkoutLog.Core.Tests.Import.ImportJson;

namespace WorkoutLog.Core.Tests.Services;

/// <summary>Runs the service with the real reader, checks, comparer, repository and unit of work on a temp database.</summary>
public class ImportServiceTests
{
    private const string FileName = "workout-log-2026-10-26-1805.json";

    /// <summary>Machine Assisted Pull-up: weight labelled Assist.</summary>
    private const int AssistedPullUp = 218;

    /// <summary>Bench as saved on the phone: 8 × 60 kg.</summary>
    private static readonly WorkoutExercise[] BenchSaved = [new StrengthExercise(8, [new StrengthSet(8, 60m, null, null)])];

    private static readonly ExerciseCatalog Catalog = new();

    private string databasePath = "";
    private SQLiteAsyncConnection connection = null!;
    private IWorkoutRepository repository = null!;
    private UnitOfWork unitOfWork = null!;
    private FakeLogger<ImportService> logger = null!;
    private ImportService service = null!;

    [SetUp]
    public async Task SetUp()
    {
        databasePath = TempDatabase.NewPath();
        connection = new SQLiteAsyncConnection(databasePath);
        await connection.RunInTransactionAsync(WorkoutTables.CreateMissing);
        repository = new WorkoutRepository(connection, new WorkoutWriterFactory());
        unitOfWork = new UnitOfWork(connection, new WorkoutWriterFactory());
        logger = new FakeLogger<ImportService>();
        service = Service(repository, unitOfWork);
    }

    [TearDown]
    public async Task TearDown()
    {
        await connection.CloseAsync();
        System.IO.File.Delete(databasePath);
    }

    [Test]
    public async Task ReadAsync_GroupsEachWorkoutAgainstThePhonesAndWritesNothing()
    {
        await AddToPhoneAsync(0, 1);
        var review = await ReadAsync(File(Workout(0, Bench), Workout(1, Run), Workout(2, Bench), Workout(3, """{"exercise":{"id":999},"sets":[{"reps":1}]}"""), "[]"));

        Assert.That(review with { Groups = ImportGroups.Empty },
            Is.EqualTo(new ImportReview(FileName, Total: 5, New: 1, Already: 1, Different: 1, Skipped: 2, PhoneCount: 2)));
        Assert.That(await repository.ListAsync(), Has.Count.EqualTo(2));
    }

    [Test]
    public async Task ReadAsync_SkippedWorkouts_LogAWarningEachWithPositionAndEveryReason()
    {
        await ReadAsync(File(
            Workout(0, Bench),
            Workout(Time(Start.AddHours(1)), "never", """{"exercise":{"id":999},"sets":[{"reps":1}]}""", """{"exercise":{"id":999},"sets":[{"reps":1}]}"""),
            "[]"));

        Assert.That(LoggedEvents.Of(logger), Is.EqualTo(new[]
        {
            (CoreEvent.ImportWorkoutSkipped, LogLevel.Warning, "Workout 2 in the file can't be imported: InvalidFinish, UnknownExercise, RepeatedExercise"),
            (CoreEvent.ImportWorkoutSkipped, LogLevel.Warning, "Workout 3 in the file can't be imported: Malformed"),
        }));
    }

    [Test]
    public async Task ReadAsync_FailingWorkoutWithAPhonesStart_IsSkippedNotDifferent()
    {
        await AddToPhoneAsync(0);

        var review = await ReadAsync(File(Workout(0, """{"exercise":{"id":8,"type":"Cardio"},"sets":[{"reps":8}]}""")));

        Assert.That((review.Different, review.Skipped), Is.EqualTo((0, 1)));
    }

    [Test]
    public async Task ReadAsync_RepeatedStart_SkipsEveryWorkoutWithItIncludingAMalformedOne()
    {
        var malformed = Workout(0).Replace("\"exercises\":[]", "\"exercises\":null");

        var review = await ReadAsync(File(Workout(0, Bench), malformed, Workout(1, Bench)));

        Assert.That((review.New, review.Skipped), Is.EqualTo((1, 2)));
        Assert.That(LoggedEvents.Of(logger).Select(e => e.Message), Is.EqualTo(new[]
        {
            "Workout 1 in the file can't be imported: RepeatedStart",
            "Workout 2 in the file can't be imported: Malformed",
        }));
    }

    [Test]
    public async Task ReadAsync_AllCantBeImported_ReturnsZeroImportable()
    {
        var review = await ReadAsync(File("[]", Workout(0)));

        Assert.That(review with { Groups = ImportGroups.Empty }, Is.EqualTo(new ImportReview(FileName, 2, 0, 0, 0, 2, 0)));
    }

    [TestCase("not JSON", ImportError.NotAnExport, true)]
    [TestCase("""{"formatVersion":3}""", ImportError.NewerVersion, false)]
    [TestCase("""{"formatVersion":1,"exportedAt":"2026-10-26T18:05:12Z","appVersion":"1.4.0","workouts":[]}""", ImportError.NoWorkouts, false)]
    [TestCase("""{"formatVersion":1,"exportedAt":"2026-10-26T18:05:12Z","workouts":[]}""", ImportError.NotAnExport, false)]
    public async Task ReadAsync_FileCantBeImported_ReturnsTheErrorAndLogsIt(string json, ImportError error, bool hasException)
    {
        var review = await ReadAsync(json);

        var logged = hasException ? (CoreEvent.ImportFileUnreadable, LogLevel.Error) : (CoreEvent.ImportFileRejected, LogLevel.Warning);
        Assert.That(review, Is.EqualTo(ImportReview.Rejected(FileName, error)));
        Assert.That(LoggedEvents.Of(logger), Is.EqualTo(new[] { (logged.Item1, logged.Item2, $"The import file can't be imported: {error}") }));
        Assert.That(logger.LatestRecord.Exception, hasException ? Is.Not.Null : Is.Null);
    }

    [Test]
    public async Task ReadAsync_OverTheLimit_IsTooLarge()
    {
        var review = await service.ReadAsync(new MemoryStream(new byte[5_242_881]), FileName);

        Assert.That(review.Error, Is.EqualTo(ImportError.TooLarge));
        Assert.That(LoggedEvents.Of(logger), Is.EqualTo(new[] { (CoreEvent.ImportFileRejected, LogLevel.Warning, "The import file can't be imported: TooLarge") }));
    }

    [Test]
    public async Task ReadAsync_StreamThrows_IsFailedAndLogsTheException()
    {
        var exception = new IOException("unreadable");
        var stream = new Mock<Stream>();
        stream.Setup(s => s.ReadAsync(It.IsAny<Memory<byte>>(), It.IsAny<CancellationToken>())).ThrowsAsync(exception);

        var review = await service.ReadAsync(stream.Object, FileName);

        Assert.That(review, Is.EqualTo(ImportReview.Rejected(FileName, ImportError.Failed)));
        Assert.That(LoggedEvents.Of(logger), Is.EqualTo(new[] { (CoreEvent.ImportFileUnreadable, LogLevel.Error, "The import file can't be imported: Failed") }));
        Assert.That(logger.LatestRecord.Exception, Is.SameAs(exception));
    }

    [Test]
    public async Task ReadAsync_PhoneReadFails_IsFailedAndLogsTheException()
    {
        var exception = new InvalidOperationException("disk");
        var failing = new Mock<IWorkoutRepository>(MockBehavior.Strict);
        failing.Setup(r => r.GetAllAsync()).ThrowsAsync(exception);

        var review = await Service(failing.Object, unitOfWork).ReadAsync(Stream(File(Workout(0, Bench))), FileName);

        Assert.That(review, Is.EqualTo(ImportReview.Rejected(FileName, ImportError.Failed)));
        Assert.That(LoggedEvents.Of(logger), Is.EqualTo(new[] { (CoreEvent.ImportPhoneReadFailed, LogLevel.Error, "Reading the phone's workouts for an import failed") }));
        Assert.That(logger.LatestRecord.Exception, Is.SameAs(exception));
    }

    [Test]
    public async Task ImportAsync_AddKeepingThePhones_AddsTheNewOnes()
    {
        var phone = await AddToPhoneAsync(0, 1);
        var review = await ReadAsync(File(Workout(0, Bench), Workout(1, Run), Workout(2, Bench, Run), "[]"));

        var result = await service.ImportAsync(review, ImportMode.Add, DifferentChoice.KeepPhones);

        Assert.That(result, Is.EqualTo(new ImportResult(ImportStatus.Imported, Added: 1, Replaced: 0, Skipped: 1)));
        Assert.That(await repository.GetAllAsync(), Is.EqualTo(new[]
        {
            phone[0],
            phone[1],
            Saved(await IdAtAsync(2), 2, [.. BenchSaved, RunSaved()]),
        }).UsingPropertiesComparer());
    }

    [Test]
    public async Task ImportAsync_AddUsingTheFiles_ReplacesTheDifferentOnesIncludingTheFinish()
    {
        var phone = await AddToPhoneAsync(0, 1);
        var review = await ReadAsync(File(Workout(0, Bench), Workout(Time(Start.AddHours(1)), Time(Start.AddHours(5)), Run), Workout(2, Bench)));

        var result = await service.ImportAsync(review, ImportMode.Add, DifferentChoice.UseFiles);

        Assert.That(result, Is.EqualTo(new ImportResult(ImportStatus.Imported, 1, 1, 0)));
        Assert.That(await repository.GetAllAsync(), Is.EqualTo(new[]
        {
            phone[0],
            new Workout(phone[1].Id, Start.AddHours(1), Start.AddHours(5), [RunSaved()]),
            Saved(await IdAtAsync(2), 2, BenchSaved),
        }).UsingPropertiesComparer());
    }

    [Test]
    public async Task ImportAsync_ReplaceAll_DeletesEveryPhoneWorkoutAndAddsEveryOneThatCanBeImported()
    {
        await AddToPhoneAsync(0, 1, 7);
        var review = await ReadAsync(File(Workout(0, Bench), Workout(1, Run), Workout(2, Bench), "[]"));

        var result = await service.ImportAsync(review, ImportMode.ReplaceAll, DifferentChoice.UseFiles);

        Assert.That(result, Is.EqualTo(new ImportResult(ImportStatus.Imported, Added: 3, Replaced: 0, Skipped: 1)));
        Assert.That(await repository.GetAllAsync(), Is.EqualTo(new[]
        {
            Saved(await IdAtAsync(0), 0, BenchSaved),
            Saved(await IdAtAsync(1), 1, [RunSaved()]),
            Saved(await IdAtAsync(2), 2, BenchSaved),
        }).UsingPropertiesComparer());
    }

    [Test]
    public async Task ImportAsync_Imported_LogsTheModeChoiceAndCounts()
    {
        await AddToPhoneAsync(0, 1);
        var review = await ReadAsync(File(Workout(0, Bench), Workout(1, Run), Workout(2, Bench), "[]"));
        logger.Collector.Clear();

        await service.ImportAsync(review, ImportMode.Add, DifferentChoice.KeepPhones);

        Assert.That(LoggedEvents.Of(logger), Is.EqualTo(new[]
        {
            (CoreEvent.ImportDone, LogLevel.Information,
                "Imported with Add and KeepPhones: 1 new, 1 already on the phone, 1 different, 1 can't be imported"),
        }));
    }

    [Test]
    public async Task ImportAsync_WriteFails_WritesNothingReturnsFailedAndLogsTheException()
    {
        var review = await ReadAsync(File(Workout(0, Bench), Workout(1, Bench)));
        await AddToPhoneAsync(1);
        logger.Collector.Clear();

        var result = await service.ImportAsync(review, ImportMode.Add, DifferentChoice.KeepPhones);

        Assert.That(result, Is.EqualTo(new ImportResult(ImportStatus.Failed, 0, 0, 0)));
        Assert.That((await repository.ListAsync()).Select(w => w.StartedAt), Is.EqualTo(new[] { Start.AddHours(1) }));
        Assert.That(LoggedEvents.Of(logger), Is.EqualTo(new[] { (CoreEvent.ImportWriteFailed, LogLevel.Error, "Writing the import failed; nothing was written") }));
        Assert.That(logger.LatestRecord.Exception, Is.InstanceOf<SQLiteException>());
    }

    [Test]
    public async Task ImportAsync_ReplaceAll_LogsTheModeWithoutAChoice()
    {
        await AddToPhoneAsync(0);
        var review = await ReadAsync(File(Workout(1, Bench)));
        logger.Collector.Clear();

        await service.ImportAsync(review, ImportMode.ReplaceAll, DifferentChoice.UseFiles);

        Assert.That(LoggedEvents.Of(logger), Is.EqualTo(new[]
        {
            (CoreEvent.ImportDone, LogLevel.Information, "Imported with ReplaceAll: 1 new, 0 already on the phone, 0 different, 0 can't be imported"),
        }));
    }

    [Test]
    public async Task ImportAsync_ReplaceAllWithNothingThatCanBeImported_ThrowsAndDeletesNothing()
    {
        await AddToPhoneAsync(0, 1);
        var review = await ReadAsync(File("[]"));

        Assert.That(() => service.ImportAsync(review, ImportMode.ReplaceAll, DifferentChoice.KeepPhones), Throws.ArgumentException);
        Assert.That(() => service.ImportAsync(new ImportReview(FileName, 3, 3, 0, 0, 0, 2), ImportMode.ReplaceAll, DifferentChoice.KeepPhones),
            Throws.ArgumentException);
        Assert.That(await repository.ListAsync(), Has.Count.EqualTo(2));
    }

    [Test]
    public async Task ReadAsync_BlankWeightInAVersion1File_MatchesThePhonesZero()
    {
        await repository.AddAsync(new NewWorkout(Start, Start.AddHours(1), [new StrengthExercise(AssistedPullUp, [new StrengthSet(8, 0m, null, null)])]));

        var review = await ReadAsync(File(1, Workout(0, $$"""{"exercise":{"id":{{AssistedPullUp}},"name":"Machine Assisted Pull-up","type":"Strength"},"sets":[{"reps":8}]}""")));

        Assert.That(review with { Groups = ImportGroups.Empty },
            Is.EqualTo(new ImportReview(FileName, Total: 1, New: 0, Already: 1, Different: 0, Skipped: 0, PhoneCount: 1)));
    }

    [Test]
    public async Task ImportAsync_FileWrittenByExport_IsAllAlreadyOnThePhone()
    {
        var start = new DateTimeOffset(2026, 10, 21, 17, 5, 0, TimeSpan.Zero).AddTicks(1234567);
        WorkoutExercise[] every =
        [
            new StrengthExercise(8, [new StrengthSet(8, 62.5m, null, null), new StrengthSet(6, 60m, null, null)]),
            new StrengthExercise(187, [new StrengthSet(null, 32.25m, new TimeSpan(0, 0, 45), 20m)]),
            new StrengthExercise(258, [new StrengthSet(null, 0m, new TimeSpan(0, 1, 30), null)]),
            new CardioExercise(311, new CardioEntry(new TimeSpan(0, 30, 0), 5.2m, null, null, null, null, null)),
            new CardioExercise(309, new CardioEntry(new TimeSpan(0, 20, 0), null, 40, 6, null, 200, 70m)),
        ];
        var workouts = new WorkoutValidator(Catalog);
        Assert.That(workouts.Validate(every).IsValid, Is.True, "the saved workout passes Save's checks");
        await repository.AddAsync(new NewWorkout(start, start.AddMinutes(62).AddTicks(7654321), every));
        await repository.AddAsync(new NewWorkout(start.AddDays(1), start.AddDays(1).AddHours(1), [new StrengthExercise(8, [new StrengthSet(5, 100m, null, null)])]));
        var folder = Directory.CreateTempSubdirectory("workoutlog-roundtrip-");
        try
        {
            var environment = new Mock<IExportEnvironment>();
            environment.SetupGet(e => e.AppVersion).Returns("1.4.0");
            environment.SetupGet(e => e.CacheFolder).Returns(folder.FullName);
            var exported = await new ExportService(repository, Catalog, environment.Object, TimeProvider.System, new FakeLogger<ExportService>()).ExportAsync();

            await using var file = System.IO.File.OpenRead(exported.FilePath!);
            var review = await service.ReadAsync(file, FileName);

            Assert.That(review with { Groups = ImportGroups.Empty }, Is.EqualTo(new ImportReview(FileName, 2, 0, 2, 0, 0, 2)));
        }
        finally
        {
            folder.Delete(recursive: true);
        }
    }

    [Test]
    public void ImportAsync_ReviewWithAnError_Throws()
    {
        Assert.That(() => service.ImportAsync(ImportReview.Rejected(FileName, ImportError.NoWorkouts), ImportMode.Add, DifferentChoice.KeepPhones),
            Throws.ArgumentException);
    }

    [Test]
    public async Task ImportAsync_ReviewMadeOutsideTheService_WritesNothing()
    {
        var result = await service.ImportAsync(new ImportReview(FileName, 3, 3, 0, 0, 0, 0), ImportMode.Add, DifferentChoice.KeepPhones);

        Assert.That(result, Is.EqualTo(new ImportResult(ImportStatus.Imported, 0, 0, 0)));
        Assert.That(await repository.ListAsync(), Is.Empty);
    }

    private ImportService Service(IWorkoutRepository workouts, IUnitOfWork writes)
    {
        IImportCheck[] checks =
        [
            new StartCheck(), new RepeatedStartCheck(), new FinishCheck(), new UnknownExerciseCheck(Catalog), new ExerciseDefinitionCheck(Catalog),
            new RepeatedExerciseCheck(), new SaveRulesCheck(new WorkoutValidator(Catalog)),
        ];
        return new ImportService(new ImportFileReader([new ValidationRulesFormatUpgrade(Catalog)]), new ImportChecker(checks), workouts, new WorkoutContentComparer(), writes, logger);
    }

    private Task<ImportReview> ReadAsync(string json) => service.ReadAsync(Stream(json), FileName);

    /// <summary>Adds a Bench workout an hour long at each hour after <see cref="ImportJson.Start"/>. Returns them as saved.</summary>
    private async Task<List<Workout>> AddToPhoneAsync(params int[] hours)
    {
        var saved = new List<Workout>();
        foreach (var hour in hours)
        {
            saved.Add(Saved(await repository.AddAsync(new NewWorkout(Start.AddHours(hour), Start.AddHours(hour + 1), BenchSaved)), hour, BenchSaved));
        }

        return saved;
    }

    private async Task<int> IdAtAsync(int hours) => (await repository.ListAsync()).Single(w => w.StartedAt == Start.AddHours(hours)).Id;

    private static Workout Saved(int id, int hours, WorkoutExercise[] exercises) => new(id, Start.AddHours(hours), Start.AddHours(hours + 1), exercises);

    private static CardioExercise RunSaved() =>
        new(311, new CardioEntry(TimeSpan.FromMinutes(30), 5.2m, null, null, null, null, null));
}
