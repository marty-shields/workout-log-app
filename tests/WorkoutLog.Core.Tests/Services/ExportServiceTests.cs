using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using Microsoft.Extensions.Time.Testing;
using Moq;
using WorkoutLog.Core.Data;
using WorkoutLog.Core.Domain.Exercises;
using WorkoutLog.Core.Domain.Workouts;
using WorkoutLog.Core.Export;
using WorkoutLog.Core.Logging;
using WorkoutLog.Core.Services;

namespace WorkoutLog.Core.Tests.Services;

public class ExportServiceTests
{
    /// <summary>Monday 26 Oct 2026, 18:05:12.3456789 UTC.</summary>
    private static readonly DateTimeOffset Now = new DateTimeOffset(2026, 10, 26, 18, 5, 12, TimeSpan.Zero).AddTicks(3456789);
    private static readonly DateTimeOffset Start = new DateTimeOffset(2026, 10, 21, 17, 5, 0, TimeSpan.Zero).AddTicks(1234567);
    private static readonly DateTimeOffset Finish = new DateTimeOffset(2026, 10, 21, 18, 2, 41, TimeSpan.Zero).AddTicks(7654321);
    private static readonly ExerciseCatalog Catalog = new();

    private const string Header = """{"formatVersion":1,"exportedAt":"2026-10-26T18:05:12Z","appVersion":"1.4.0","workouts":""";
    private const string FileName = "workout-log-2026-10-26-1805.json";

    private Mock<IWorkoutRepository> repository = null!;
    private Mock<IExportEnvironment> environment = null!;
    private FakeTimeProvider time = null!;
    private FakeLogger<ExportService> logger = null!;
    private ExportService service = null!;
    private string folder = "";

    [SetUp]
    public void SetUp()
    {
        folder = Path.Combine(Path.GetTempPath(), $"workoutlog-export-{Guid.NewGuid():N}");
        Directory.CreateDirectory(folder);
        repository = new Mock<IWorkoutRepository>(MockBehavior.Strict);
        environment = new Mock<IExportEnvironment>();
        environment.SetupGet(e => e.AppVersion).Returns("1.4.0");
        environment.SetupGet(e => e.CacheFolder).Returns(folder);
        time = new FakeTimeProvider(Now);
        time.SetLocalTimeZone(TimeZoneInfo.Utc);
        logger = new FakeLogger<ExportService>();
        service = new ExportService(repository.Object, Catalog, environment.Object, time, logger);
    }

    [TearDown]
    public void TearDown() => Directory.Delete(folder, recursive: true);

    [Test]
    public async Task ExportAsync_WritesStrengthAndCardioWithoutIndentation()
    {
        Workouts(new Workout(1, Start, Finish,
        [
            new StrengthExercise(8, [new StrengthSet(8, 60m, null, null), new StrengthSet(6, 62.5m, null, null)]),
            new CardioExercise(311, new CardioEntry(TimeSpan.FromMinutes(30), 5.2m, null, null, null, null, null, new TimeSpan(0, 5, 46))),
        ]));

        var result = await service.ExportAsync();

        Assert.That(result, Is.EqualTo(new ExportResult(ExportStatus.Exported, Path.Combine(folder, FileName))));
        Assert.That(await File.ReadAllTextAsync(result.FilePath!), Is.EqualTo(Header +
            """[{"startedAt":"2026-10-21T17:05:00.1234567Z","finishedAt":"2026-10-21T18:02:41.7654321Z","exercises":[""" +
            """{"exercise":{"id":8,"name":"Barbell Bench Press","type":"Strength"},"sets":[{"reps":8,"weightKg":60},{"reps":6,"weightKg":62.5}]},""" +
            """{"exercise":{"id":311,"name":"Outdoor Run","type":"Cardio","distanceUnit":"Km","paceUnit":"PerKm"},"entry":{"duration":"00:30:00","distance":5.2,"pace":"00:05:46"}}]}]}"""));
    }

    [Test]
    public async Task ExportAsync_WritesEveryMetricWithAValueAndTheUnitsTheExerciseHas()
    {
        Workouts(new Workout(1, Start, Finish,
        [
            new StrengthExercise(187, [new StrengthSet(null, 32m, TimeSpan.FromSeconds(45), 20m)]),
            new StrengthExercise(258, [new StrengthSet(null, null, TimeSpan.FromSeconds(90), null)]),
            new CardioExercise(300, new CardioEntry(new TimeSpan(1, 2, 3), 10m, 4, 5, 1.5m, 310, 9.5m, TimeSpan.FromMinutes(6))),
            new CardioExercise(309, new CardioEntry(TimeSpan.FromMinutes(20), null, 40, 6, null, 200, 70m, null)),
        ]));

        Assert.That(await ExerciseJson(), Is.EqualTo(
            """{"exercise":{"id":187,"name":"Farmer's Walk","type":"Strength","distanceUnit":"Metres"},"sets":[{"weightKg":32,"duration":"00:00:45","distance":20}]},""" +
            """{"exercise":{"id":258,"name":"Plank","type":"Strength"},"sets":[{"duration":"00:01:30"}]},""" +
            """{"exercise":{"id":300,"name":"Treadmill","type":"Cardio","distanceUnit":"Km","rateUnit":"KmPerHour"},"entry":""" +
            """{"duration":"01:02:03","distance":10,"floors":4,"level":5,"incline":1.5,"calories":310,"rate":9.5,"pace":"00:06:00"}},""" +
            """{"exercise":{"id":309,"name":"Stair Climber","type":"Cardio","rateUnit":"StepsPerMin"},"entry":{"duration":"00:20:00","floors":40,"level":6,"calories":200,"rate":70}}"""));
    }

    [Test]
    public async Task ExportAsync_DecimalsRoundedTo2PlacesWithoutTrailingZeros()
    {
        Workouts(new Workout(1, Start, Finish,
            [new StrengthExercise(8, [new StrengthSet(1, 62.4999999999m, null, null), new StrengthSet(1, 22.505m, null, null), new StrengthSet(1, 60.00m, null, null)])]));

        Assert.That(await ExerciseJson(), Is.EqualTo(
            """{"exercise":{"id":8,"name":"Barbell Bench Press","type":"Strength"},"sets":[{"reps":1,"weightKg":62.5},{"reps":1,"weightKg":22.51},{"reps":1,"weightKg":60}]}"""));
    }

    [Test]
    public async Task ExportAsync_StrengthExerciseWithNoSets_WritesAnEmptySetList()
    {
        Workouts(new Workout(1, Start, Finish, [new StrengthExercise(8, [])]));

        Assert.That(await ExerciseJson(), Is.EqualTo("""{"exercise":{"id":8,"name":"Barbell Bench Press","type":"Strength"},"sets":[]}"""));
    }

    [Test]
    public async Task ExportAsync_UnknownExerciseId_WritesTheIdOnlyAndLogsAWarningOncePerId()
    {
        Workouts(
            new Workout(1, Start, Finish, [new StrengthExercise(999, [new StrengthSet(5, null, null, null)]), new CardioExercise(998, new CardioEntry(TimeSpan.FromMinutes(1), null, null, null, null, null, null, null))]),
            new Workout(2, Start.AddDays(1), Finish.AddDays(1), [new StrengthExercise(999, [new StrengthSet(6, null, null, null)])]));

        var result = await service.ExportAsync();

        Assert.That(await File.ReadAllTextAsync(result.FilePath!), Does.Contain(
            """exercises":[{"exercise":{"id":999},"sets":[{"reps":5}]},{"exercise":{"id":998},"entry":{"duration":"00:01:00"}}]}"""));
        Assert.That(LoggedEvents.Of(logger), Is.EqualTo(new[]
        {
            (CoreEvent.ExportUnknownExercise, LogLevel.Warning, "Exercise 999 is not in the exercise list; exported with its id only"),
            (CoreEvent.ExportUnknownExercise, LogLevel.Warning, "Exercise 998 is not in the exercise list; exported with its id only"),
            (CoreEvent.ExportWritten, LogLevel.Information, "Export written with 2 workouts"),
        }));
    }

    [Test]
    public async Task ExportAsync_WritesWorkoutsAndExercisesInTheRepositorysOrder()
    {
        Workouts(
            new Workout(5, Start, Finish, [new StrengthExercise(13, [new StrengthSet(1, null, null, null)]), new StrengthExercise(8, [new StrengthSet(2, null, null, null)])]),
            new Workout(2, Start.AddDays(1), Finish.AddDays(1), []));

        var json = await File.ReadAllTextAsync((await service.ExportAsync()).FilePath!);

        Assert.That(json, Is.EqualTo(Header +
            """[{"startedAt":"2026-10-21T17:05:00.1234567Z","finishedAt":"2026-10-21T18:02:41.7654321Z","exercises":[""" +
            """{"exercise":{"id":13,"name":"Barbell Close-Grip Bench Press","type":"Strength"},"sets":[{"reps":1}]},""" +
            """{"exercise":{"id":8,"name":"Barbell Bench Press","type":"Strength"},"sets":[{"reps":2}]}]},""" +
            """{"startedAt":"2026-10-22T17:05:00.1234567Z","finishedAt":"2026-10-22T18:02:41.7654321Z","exercises":[]}]}"""));
    }

    [Test]
    public async Task ExportAsync_NoWorkouts_WritesTheHeaderAndAnEmptyList()
    {
        Workouts();

        Assert.That(await File.ReadAllTextAsync((await service.ExportAsync()).FilePath!), Is.EqualTo(Header + "[]}"));
    }

    [Test]
    public async Task ExportAsync_TimesWithAnOffset_WrittenAsUtc()
    {
        var offset = TimeSpan.FromHours(2);
        time.SetUtcNow(Now.ToOffset(offset));
        Workouts(new Workout(1, Start.ToOffset(offset), Finish.ToOffset(offset), []));

        var json = await File.ReadAllTextAsync((await service.ExportAsync()).FilePath!);

        Assert.That(json, Does.StartWith(Header + """[{"startedAt":"2026-10-21T17:05:00.1234567Z","finishedAt":"2026-10-21T18:02:41.7654321Z","""));
    }

    [Test]
    public async Task ExportAsync_FileNameIsInThePhonesTimeZone()
    {
        time.SetLocalTimeZone(TimeZoneInfo.CreateCustomTimeZone("Plus10", TimeSpan.FromHours(10), "Plus10", "Plus10"));
        Workouts();

        var result = await service.ExportAsync();

        Assert.That(result.FilePath, Is.EqualTo(Path.Combine(folder, "workout-log-2026-10-27-0405.json")));
        Assert.That(File.Exists(result.FilePath), Is.True);
    }

    [Test]
    public async Task ExportAsync_FileOfTheSameName_IsReplaced()
    {
        await File.WriteAllTextAsync(Path.Combine(folder, FileName), new string('x', 10_000));
        Workouts();

        var result = await service.ExportAsync();

        Assert.That(await File.ReadAllTextAsync(result.FilePath!), Is.EqualTo(Header + "[]}"));
    }

    [Test]
    public async Task ExportAsync_ExactlyTheLimit_IsWritten()
    {
        Workouts();
        PadAppVersionTo(ExportFormat.MaxBytes);

        var result = await service.ExportAsync();

        Assert.That(result.Status, Is.EqualTo(ExportStatus.Exported));
        Assert.That(new FileInfo(result.FilePath!).Length, Is.EqualTo(5_242_880));
    }

    [Test]
    public async Task ExportAsync_OneByteOverTheLimit_ReturnsTooLargeWritesNothingAndLogsAWarning()
    {
        Workouts();
        PadAppVersionTo(ExportFormat.MaxBytes + 1);

        var result = await service.ExportAsync();

        Assert.That(result, Is.EqualTo(new ExportResult(ExportStatus.TooLarge, null)));
        Assert.That(Directory.GetFiles(folder), Is.Empty);
        Assert.That(LoggedEvents.Of(logger), Is.EqualTo(new[]
        {
            (CoreEvent.ExportTooLarge, LogLevel.Warning, "Export of 0 workouts is 5242881 bytes, over the limit; not written"),
        }));
    }

    [Test]
    public async Task ExportAsync_Written_LogsTheCountAtInformation()
    {
        Workouts(new Workout(1, Start, Finish, []));

        await service.ExportAsync();

        Assert.That(LoggedEvents.Of(logger), Is.EqualTo(new[] { (CoreEvent.ExportWritten, LogLevel.Information, "Export written with 1 workouts") }));
        Assert.That(logger.LatestRecord.Exception, Is.Null);
    }

    [Test]
    public async Task ExportAsync_RepositoryThrows_ReturnsFailedAndLogsTheError()
    {
        var exception = new InvalidOperationException("disk");
        repository.Setup(r => r.GetAllAsync()).ThrowsAsync(exception);

        var result = await service.ExportAsync();

        Assert.That(result, Is.EqualTo(new ExportResult(ExportStatus.Failed, null)));
        Assert.That(LoggedEvents.Of(logger), Is.EqualTo(new[] { (CoreEvent.ExportFailed, LogLevel.Error, "Exporting the workouts failed") }));
        Assert.That(logger.LatestRecord.Exception, Is.SameAs(exception));
    }

    [Test]
    public async Task ExportAsync_WriteFails_ReturnsFailedAndLogsTheError()
    {
        environment.SetupGet(e => e.CacheFolder).Returns(Path.Combine(folder, "missing"));
        Workouts();

        var result = await service.ExportAsync();

        Assert.That(result, Is.EqualTo(new ExportResult(ExportStatus.Failed, null)));
        Assert.That(LoggedEvents.Of(logger), Is.EqualTo(new[] { (CoreEvent.ExportFailed, LogLevel.Error, "Exporting the workouts failed") }));
        Assert.That(logger.LatestRecord.Exception, Is.InstanceOf<DirectoryNotFoundException>());
    }

    [TestCase(0)]
    [TestCase(3)]
    public async Task CountAsync_ReturnsTheNumberOfSavedWorkouts(int count)
    {
        repository.Setup(r => r.ListAsync(null, null, null))
            .ReturnsAsync(Enumerable.Range(1, count).Select(id => new WorkoutSummary(id, Start.AddDays(id))).ToList());

        Assert.That(await service.CountAsync(), Is.EqualTo(new ExportCount(count, Failed: false)));
        Assert.That(LoggedEvents.Of(logger), Is.Empty);
    }

    [Test]
    public async Task CountAsync_RepositoryThrows_ReturnsFailedAndLogsTheError()
    {
        var exception = new InvalidOperationException("disk");
        repository.Setup(r => r.ListAsync(null, null, null)).ThrowsAsync(exception);

        Assert.That(await service.CountAsync(), Is.EqualTo(new ExportCount(0, Failed: true)));
        Assert.That(LoggedEvents.Of(logger), Is.EqualTo(new[] { (CoreEvent.ExportCountFailed, LogLevel.Error, "Counting the workouts to export failed") }));
        Assert.That(logger.LatestRecord.Exception, Is.SameAs(exception));
    }

    private void Workouts(params Workout[] workouts) => repository.Setup(r => r.GetAllAsync()).ReturnsAsync(workouts);

    /// <summary>The exercises of the only workout in the exported file, without the surrounding brackets.</summary>
    private async Task<string> ExerciseJson()
    {
        var json = await File.ReadAllTextAsync((await service.ExportAsync()).FilePath!);
        const string before = "\"exercises\":[";
        var start = json.IndexOf(before, StringComparison.Ordinal) + before.Length;
        return json[start..^"]}]}".Length];
    }

    /// <summary>Lengthens the app version so the file is <paramref name="bytes"/> long.</summary>
    private void PadAppVersionTo(int bytes)
    {
        var unpadded = Encoding.UTF8.GetByteCount(Header.Replace("1.4.0", "") + "[]}");
        environment.SetupGet(e => e.AppVersion).Returns(new string('9', bytes - unpadded));
    }
}
