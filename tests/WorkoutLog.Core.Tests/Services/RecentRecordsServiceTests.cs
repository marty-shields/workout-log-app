using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using Microsoft.Extensions.Time.Testing;
using Moq;
using WorkoutLog.Core.Data;
using WorkoutLog.Core.Domain.Exercises;
using WorkoutLog.Core.Domain.Exercises.Bests;
using WorkoutLog.Core.Domain.Exercises.Progress;
using WorkoutLog.Core.Domain.Workouts;
using WorkoutLog.Core.Logging;
using WorkoutLog.Core.Services;
using static WorkoutLog.Core.Tests.Domain.Exercises.Bests.BestRuleCases;

namespace WorkoutLog.Core.Tests.Services;

public class RecentRecordsServiceTests
{
    /// <summary>Fri 30 Oct 2026, 12:00 UTC; 13:00 in the phone's UTC+1.</summary>
    private static readonly DateTimeOffset Now = new(2026, 10, 30, 12, 0, 0, TimeSpan.Zero);

    /// <summary>00:00 on Thu 1 Oct in UTC+1: the window's first instant.</summary>
    private static readonly DateTimeOffset WindowStart = new(2026, 10, 1, 0, 0, 0, TimeSpan.FromHours(1));

    private static readonly DateTimeOffset LongAgo = Now.AddDays(-60);

    private Mock<IWorkoutRepository> repository = null!;
    private FakeLogger<RecentRecordsService> logger = null!;
    private RecentRecordsService service = null!;
    private int nextId;

    [SetUp]
    public void SetUp()
    {
        repository = new Mock<IWorkoutRepository>(MockBehavior.Strict);
        logger = new FakeLogger<RecentRecordsService>();
        var clock = new FakeTimeProvider(Now);
        clock.SetLocalTimeZone(TimeZoneInfo.CreateCustomTimeZone("UTC+1", TimeSpan.FromHours(1), "UTC+1", "UTC+1"));
        IBestRule[] rules =
        [
            new LiftBestRule(new TopSetRule()), new BodyweightBestRule(new BodyweightRule(new TopSetRule(), new E1rmRule())), new AssistedBestRule(new LeastAssistRule()),
            new HoldBestRule(new HoldRule()), new CarryBestRule(new CarryRule()), new CardioBestRule(new CardioSessionRule()),
        ];
        service = new RecentRecordsService(repository.Object, Catalog, new RecordCheck(rules), clock, logger);
        nextId = 0;
    }

    [Test]
    public async Task GetAsync_NothingSaved_ReturnsNoRecords()
    {
        Setup();

        Assert.That(await service.GetAsync(), Is.EqualTo(new RecentRecordsResult([], Now, Failed: false)).UsingPropertiesComparer());
    }

    [Test]
    public async Task GetAsync_ReturnsARecordWithItsStartAndBest()
    {
        Setup(
            Workout(LongAgo, Sets(Squat, Set(5, 60m))),
            Workout(Now.AddDays(-2), Sets(Squat, Set(5, 60m), Set(3, 70m))));

        var result = await service.GetAsync();

        Assert.That(result, Is.EqualTo(new RecentRecordsResult([new ExerciseRecord(Squat, Now.AddDays(-2), new ExerciseBest(70m, 3, null, null))], Now, Failed: false))
            .UsingPropertiesComparer());
    }

    [Test]
    public async Task GetAsync_FirstSession_IsNotARecord()
    {
        Setup(Workout(Now.AddDays(-1), Sets(Squat, Set(5, 100m))));

        Assert.That((await service.GetAsync()).Records, Is.Empty);
    }

    [Test]
    public async Task GetAsync_Tie_IsNotARecord()
    {
        Setup(Workout(LongAgo, Sets(Squat, Set(5, 60m))), Workout(Now.AddDays(-1), Sets(Squat, Set(5, 60m))));

        Assert.That((await service.GetAsync()).Records, Is.Empty);
    }

    [Test]
    public async Task GetAsync_Assisted_LessAssistIsTheRecord()
    {
        Setup(
            Workout(LongAgo, Sets(AssistedPullUp, Set(10, 25m))),
            Workout(Now.AddDays(-3), Sets(AssistedPullUp, Set(10, 30m))),
            Workout(Now.AddDays(-1), Sets(AssistedPullUp, Set(8, 20m))));

        Assert.That((await service.GetAsync()).Records, Is.EqualTo(new[] { new ExerciseRecord(AssistedPullUp, Now.AddDays(-1), new ExerciseBest(20m, 8, null, null)) }));
    }

    [Test]
    public async Task GetAsync_StartOfTheWindowInThePhonesTimeZone_IsIn()
    {
        Setup(Workout(LongAgo, Sets(Squat, Set(5, 60m))), Workout(WindowStart, Sets(Squat, Set(5, 70m))));

        Assert.That((await service.GetAsync()).Records.Select(r => r.StartedAt), Is.EqualTo(new[] { WindowStart }));
    }

    [Test]
    public async Task GetAsync_JustBeforeTheWindow_IsOut()
    {
        Setup(Workout(LongAgo, Sets(Squat, Set(5, 60m))), Workout(WindowStart.AddMinutes(-1), Sets(Squat, Set(5, 70m))));

        Assert.That((await service.GetAsync()).Records, Is.Empty);
    }

    [Test]
    public async Task GetAsync_AfterNow_IsOut()
    {
        Setup(Workout(LongAgo, Sets(Squat, Set(5, 60m))), Workout(Now.AddMinutes(1), Sets(Squat, Set(5, 70m))));

        Assert.That((await service.GetAsync()).Records, Is.Empty);
    }

    [Test]
    public async Task GetAsync_RecordBeforeTheWindow_IsOut()
    {
        Setup(
            Workout(LongAgo, Sets(Squat, Set(5, 60m))),
            Workout(LongAgo.AddDays(1), Sets(Squat, Set(5, 70m))),
            Workout(Now.AddDays(-1), Sets(Squat, Set(5, 65m))));

        Assert.That((await service.GetAsync()).Records, Is.Empty);
    }

    [Test]
    public async Task GetAsync_ManyRecordsOfAnExercise_TakesTheLatest()
    {
        Setup(
            Workout(LongAgo, Sets(Squat, Set(5, 60m))),
            Workout(Now.AddDays(-5), Sets(Squat, Set(5, 70m))),
            Workout(Now.AddDays(-2), Sets(Squat, Set(5, 75m))));

        Assert.That((await service.GetAsync()).Records, Is.EqualTo(new[] { new ExerciseRecord(Squat, Now.AddDays(-2), new ExerciseBest(75m, 5, null, null)) }));
    }

    [Test]
    public async Task GetAsync_MoreThanThree_TakesTheNewestThreeNewestFirstSameWorkoutAToZ()
    {
        Setup(
            Workout(LongAgo,
                Sets(Squat, Set(5, 60m)), Sets(Curl, Set(10, 10m)), Sets(PushUp, Set(20, 0m)), Entry(Run, TimeSpan.FromMinutes(30), 5m)),
            Workout(Now.AddDays(-9), Entry(Run, TimeSpan.FromMinutes(30), 6m)),
            Workout(Now.AddDays(-5), Sets(Squat, Set(5, 70m))),
            Workout(Now.AddDays(-2), Sets(PushUp, Set(25, 0m)), Sets(Curl, Set(10, 12m))));

        Assert.That((await service.GetAsync()).Records.Select(r => r.ExerciseId), Is.EqualTo(new[] { Curl, PushUp, Squat }));
    }

    [Test]
    public async Task GetAsync_LogsTheCountAtInformation()
    {
        Setup(Workout(LongAgo, Sets(Squat, Set(5, 60m))), Workout(Now.AddDays(-1), Sets(Squat, Set(5, 70m))));

        await service.GetAsync();

        Assert.That(LoggedEvents.Of(logger), Is.EqualTo(new[] { (CoreEvent.RecentRecordsLoaded, LogLevel.Information, "Loaded 1 recent records") }));
    }

    [Test]
    public async Task GetAsync_UnknownExercise_IsLeftOutAndLoggedWithItsId()
    {
        Setup(
            Workout(LongAgo, Sets(Squat, Set(5, 60m)), Sets(999, Set(5, 60m))),
            Workout(Now.AddDays(-1), Sets(Squat, Set(5, 70m)), Sets(999, Set(5, 70m))));

        var result = await service.GetAsync();

        Assert.That(result.Records.Select(r => r.ExerciseId), Is.EqualTo(new[] { Squat }));
        Assert.That(LoggedEvents.Of(logger), Is.EqualTo(new[]
        {
            (CoreEvent.RecentRecordsUnknownExercise, LogLevel.Warning, "Exercise 999 is not in the exercise list; left out"),
            (CoreEvent.RecentRecordsLoaded, LogLevel.Information, "Loaded 1 recent records"),
        }));
    }

    [Test]
    public async Task GetAsync_ReadFails_ReturnsFailedAndLogsAWarningWithTheException()
    {
        var error = new InvalidOperationException("disk");
        repository.Setup(r => r.GetAllAsync()).ThrowsAsync(error);

        var result = await service.GetAsync();

        Assert.That(result, Is.EqualTo(new RecentRecordsResult([], Now, Failed: true)).UsingPropertiesComparer());
        Assert.Multiple(() =>
        {
            Assert.That(LoggedEvents.Of(logger), Is.EqualTo(new[] { (CoreEvent.RecentRecordsLoadFailed, LogLevel.Warning, "Loading the recent records failed") }));
            Assert.That(logger.Collector.LatestRecord.Exception, Is.SameAs(error));
        });
    }

    [Test]
    public async Task GetAsync_FindingRecordsFails_ReturnsFailedAndLogsAWarningWithTheException()
    {
        var error = new InvalidOperationException("bad set");
        var recordCheck = new Mock<IRecordCheck>();
        recordCheck.Setup(c => c.Records(It.IsAny<ExerciseDefinition>(), It.IsAny<IEnumerable<WorkoutSession>>())).Throws(error);
        service = new RecentRecordsService(repository.Object, Catalog, recordCheck.Object, new FakeTimeProvider(Now), logger);
        Setup(Workout(Now.AddDays(-1), Sets(Squat, Set(5, 100m))));

        var result = await service.GetAsync();

        Assert.That(result, Is.EqualTo(new RecentRecordsResult([], Now, Failed: true)).UsingPropertiesComparer());
        Assert.Multiple(() =>
        {
            Assert.That(LoggedEvents.Of(logger), Is.EqualTo(new[] { (CoreEvent.RecentRecordsLoadFailed, LogLevel.Warning, "Loading the recent records failed") }));
            Assert.That(logger.Collector.LatestRecord.Exception, Is.SameAs(error));
        });
    }

    private Workout Workout(DateTimeOffset startedAt, params WorkoutExercise[] exercises) =>
        new(++nextId, startedAt, startedAt.AddHours(1), exercises);

    private void Setup(params Workout[] workouts) =>
        repository.Setup(r => r.GetAllAsync()).ReturnsAsync(workouts);
}
