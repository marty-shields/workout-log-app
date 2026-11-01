using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
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

public class WorkoutSummaryServiceTests
{
    private static readonly DateTimeOffset Start = new(2026, 10, 30, 18, 0, 0, TimeSpan.Zero);

    private Mock<IWorkoutRepository> repository = null!;
    private FakeLogger<WorkoutSummaryService> logger = null!;
    private WorkoutSummaryService service = null!;
    private readonly List<Workout> workouts = [];
    private int nextId;

    [SetUp]
    public void SetUp()
    {
        repository = new Mock<IWorkoutRepository>(MockBehavior.Strict);
        logger = new FakeLogger<WorkoutSummaryService>();
        IBestRule[] rules =
        [
            new LiftBestRule(new TopSetRule()), new BodyweightBestRule(new BodyweightRule(new TopSetRule(), new E1rmRule())), new AssistedBestRule(new LeastAssistRule()),
            new HoldBestRule(new HoldRule()), new CarryBestRule(new CarryRule()), new CardioBestRule(new CardioSessionRule()),
        ];
        service = new WorkoutSummaryService(repository.Object, Catalog, new RecordCheck(rules), new BestChangeRule(rules), logger);
        nextId = 0;
        workouts.Clear();
    }

    [Test]
    public async Task GetAsync_ReturnsTheTotals()
    {
        var saved = Workout(Start, Sets(Squat, Set(5, 60m), Set(5, 60m)), Sets(Curl, Set(10, 10m)), Entry(Run, TimeSpan.FromMinutes(30), 5m));
        Setup();

        var summary = (await service.GetAsync(saved.Id)).Summary!;

        Assert.Multiple(() =>
        {
            Assert.That(summary.Duration, Is.EqualTo(TimeSpan.FromHours(1)));
            Assert.That(summary.ExerciseCount, Is.EqualTo(3));
            Assert.That(summary.SetCount, Is.EqualTo(3));
        });
    }

    [Test]
    public async Task GetAsync_FirstWorkout_HasNoEarlierWorkoutAndEveryExerciseIsFirstTime()
    {
        var saved = Workout(Start, Sets(Squat, Set(5, 60m)));
        Setup();

        var summary = (await service.GetAsync(saved.Id)).Summary!;

        Assert.Multiple(() =>
        {
            Assert.That(summary.HasEarlierWorkout, Is.False);
            Assert.That(summary.Exercises, Is.EqualTo(new[] { new SummaryExercise(Squat, new ExerciseBest(60m, 5, null, null), false, new BestChange(BestChangeKind.FirstTime, null)) }));
        });
    }

    [Test]
    public async Task GetAsync_BeatsEveryEarlierSession_IsARecordAndUpOnTheLastSession()
    {
        Workout(Start.AddDays(-14), Sets(Squat, Set(5, 100m)));
        Workout(Start.AddDays(-7), Sets(Squat, Set(5, 90m)));
        var saved = Workout(Start, Sets(Squat, Set(3, 102.5m)));
        Setup();

        var exercise = (await service.GetAsync(saved.Id)).Summary!.Exercises.Single();

        Assert.Multiple(() =>
        {
            Assert.That(exercise.IsRecord, Is.True);
            Assert.That(exercise.Change, Is.EqualTo(new BestChange(BestChangeKind.Up, new ExerciseBest(12.5m, null, null, null))));
        });
    }

    [Test]
    public async Task GetAsync_BeatsTheLastSessionButNotAnOlderOne_IsUpButNotARecord()
    {
        Workout(Start.AddDays(-14), Sets(Squat, Set(5, 100m)));
        Workout(Start.AddDays(-7), Sets(Squat, Set(5, 90m)));
        var saved = Workout(Start, Sets(Squat, Set(5, 95m)));
        Setup();

        var exercise = (await service.GetAsync(saved.Id)).Summary!.Exercises.Single();

        Assert.Multiple(() =>
        {
            Assert.That(exercise.IsRecord, Is.False);
            Assert.That(exercise.Change.Kind, Is.EqualTo(BestChangeKind.Up));
        });
    }

    [Test]
    public async Task GetAsync_Tie_IsEqualAndNotARecord()
    {
        Workout(Start.AddDays(-7), Sets(Squat, Set(5, 100m)));
        var saved = Workout(Start, Sets(Squat, Set(5, 100m)));
        Setup();

        var exercise = (await service.GetAsync(saved.Id)).Summary!.Exercises.Single();

        Assert.Multiple(() =>
        {
            Assert.That(exercise.IsRecord, Is.False);
            Assert.That(exercise.Change, Is.EqualTo(new BestChange(BestChangeKind.Equal, null)));
        });
    }

    [Test]
    public async Task GetAsync_WorseThanLastTime_IsDown()
    {
        Workout(Start.AddDays(-7), Sets(Squat, Set(5, 100m)));
        var saved = Workout(Start, Sets(Squat, Set(5, 90m)));
        Setup();

        Assert.That((await service.GetAsync(saved.Id)).Summary!.Exercises.Single().Change, Is.EqualTo(new BestChange(BestChangeKind.Down, new ExerciseBest(10m, null, null, null))));
    }

    [Test]
    public async Task GetAsync_Assisted_LessAssistIsUpAndARecord()
    {
        Workout(Start.AddDays(-7), Sets(AssistedPullUp, Set(10, 25m)));
        var saved = Workout(Start, Sets(AssistedPullUp, Set(8, 20m)));
        Setup();

        var exercise = (await service.GetAsync(saved.Id)).Summary!.Exercises.Single();

        Assert.Multiple(() =>
        {
            Assert.That(exercise.IsRecord, Is.True);
            Assert.That(exercise.Change, Is.EqualTo(new BestChange(BestChangeKind.Up, new ExerciseBest(5m, null, null, null))));
        });
    }

    [Test]
    public async Task GetAsync_CardioDuration_ComparesTheDuration()
    {
        Workout(Start.AddDays(-7), Entry(StairClimber, TimeSpan.FromMinutes(10), null));
        var saved = Workout(Start, Entry(StairClimber, TimeSpan.FromMinutes(15), null));
        Setup();

        Assert.That((await service.GetAsync(saved.Id)).Summary!.Exercises.Single().Change,
            Is.EqualTo(new BestChange(BestChangeKind.Up, new ExerciseBest(null, null, TimeSpan.FromMinutes(5), null))));
    }

    [Test]
    public async Task GetAsync_ExerciseNotDoneBefore_IsFirstTimeEvenWithEarlierWorkouts()
    {
        Workout(Start.AddDays(-7), Sets(Squat, Set(5, 100m)));
        var saved = Workout(Start, Sets(Squat, Set(5, 100m)), Sets(Curl, Set(10, 10m)));
        Setup();

        var summary = (await service.GetAsync(saved.Id)).Summary!;

        Assert.Multiple(() =>
        {
            Assert.That(summary.HasEarlierWorkout, Is.True);
            Assert.That(summary.Exercises.Select(e => (e.ExerciseId, e.Change.Kind)), Is.EqualTo(new[] { (Squat, BestChangeKind.Equal), (Curl, BestChangeKind.FirstTime) }));
        });
    }

    [Test]
    public async Task GetAsync_LaterWorkouts_AreIgnored()
    {
        var saved = Workout(Start, Sets(Squat, Set(5, 100m)));
        Workout(Start.AddDays(7), Sets(Squat, Set(5, 200m)));
        Setup();

        var summary = (await service.GetAsync(saved.Id)).Summary!;

        Assert.Multiple(() =>
        {
            Assert.That(summary.HasEarlierWorkout, Is.False);
            Assert.That(summary.Exercises.Single().Change.Kind, Is.EqualTo(BestChangeKind.FirstTime));
        });
    }

    [Test]
    public async Task GetAsync_UnknownExercise_IsLeftOutAndLoggedWithItsId()
    {
        var saved = Workout(Start, Sets(Squat, Set(5, 60m)), Sets(999, Set(5, 60m)));
        Setup();

        var summary = (await service.GetAsync(saved.Id)).Summary!;

        Assert.That(summary.Exercises.Select(e => e.ExerciseId), Is.EqualTo(new[] { Squat }));
        Assert.That(LoggedEvents.Of(logger), Is.EqualTo(new[]
        {
            (CoreEvent.WorkoutSummaryUnknownExercise, LogLevel.Warning, "Exercise 999 is not in the exercise list; left out"),
            (CoreEvent.WorkoutSummaryLoaded, LogLevel.Information, $"Loaded the summary of workout {saved.Id}"),
        }));
    }

    [Test]
    public async Task GetAsync_ExerciseWithNoBest_IsLeftOutButCounted()
    {
        Workout(Start.AddDays(-7), Sets(Squat, Set(5, 100m)));
        var saved = Workout(Start, Sets(Squat, Set(5, 100m)), Sets(Curl));
        Setup();

        var summary = (await service.GetAsync(saved.Id)).Summary!;

        Assert.Multiple(() =>
        {
            Assert.That(summary.ExerciseCount, Is.EqualTo(2));
            Assert.That(summary.Exercises.Select(e => e.ExerciseId), Is.EqualTo(new[] { Squat }));
        });
    }

    [Test]
    public async Task GetAsync_SummarisingAnExerciseFails_LeavesItOutAndLogsAWarningWithTheException()
    {
        var error = new InvalidOperationException("bad set");
        var recordCheck = new Mock<IRecordCheck>();
        recordCheck.Setup(c => c.Records(It.IsAny<ExerciseDefinition>(), It.IsAny<IEnumerable<WorkoutSession>>())).Returns([]);
        var changeRule = new Mock<IBestChangeRule>();
        changeRule.Setup(r => r.Change(It.Is<ExerciseDefinition>(d => d.Id == Squat), It.IsAny<WorkoutExercise>(), It.IsAny<IEnumerable<WorkoutExercise>>())).Throws(error);
        changeRule.Setup(r => r.Change(It.Is<ExerciseDefinition>(d => d.Id == Curl), It.IsAny<WorkoutExercise>(), It.IsAny<IEnumerable<WorkoutExercise>>()))
            .Returns(new SessionChange(new ExerciseBest(10m, 8, null, null), new BestChange(BestChangeKind.FirstTime, null)));
        service = new WorkoutSummaryService(repository.Object, Catalog, recordCheck.Object, changeRule.Object, logger);
        var saved = Workout(Start, Sets(Squat, Set(5, 100m)), Sets(Curl, Set(8, 10m)));
        Setup();

        var result = await service.GetAsync(saved.Id);

        Assert.Multiple(() =>
        {
            Assert.That(result.Failed, Is.False);
            Assert.That(result.Summary!.Exercises.Select(e => e.ExerciseId), Is.EqualTo(new[] { Curl }));
            Assert.That(LoggedEvents.Of(logger).Select(e => (e.Id, e.Level)), Does.Contain((CoreEvent.WorkoutSummaryExerciseFailed, LogLevel.Warning)));
            Assert.That(logger.Collector.GetSnapshot().Single(r => r.Id.Id == (int)CoreEvent.WorkoutSummaryExerciseFailed).Exception, Is.SameAs(error));
        });
    }

    [Test]
    public async Task GetAsync_Loaded_LogsAtInformationWithoutTheWorkoutsContents()
    {
        var saved = Workout(Start, Sets(Squat, Set(5, 60m)));
        Setup();

        await service.GetAsync(saved.Id);

        Assert.That(LoggedEvents.Of(logger), Is.EqualTo(new[] { (CoreEvent.WorkoutSummaryLoaded, LogLevel.Information, $"Loaded the summary of workout {saved.Id}") }));
    }

    [Test]
    public async Task GetAsync_ReadFails_ReturnsFailedAndLogsAWarningWithTheException()
    {
        var error = new InvalidOperationException("disk");
        repository.Setup(r => r.GetAllAsync()).ThrowsAsync(error);

        var result = await service.GetAsync(1);

        Assert.That(result, Is.EqualTo(new WorkoutSummaryResult(null, Failed: true)));
        Assert.Multiple(() =>
        {
            Assert.That(LoggedEvents.Of(logger), Is.EqualTo(new[] { (CoreEvent.WorkoutSummaryLoadFailed, LogLevel.Warning, "Loading the workout summary failed") }));
            Assert.That(logger.Collector.LatestRecord.Exception, Is.SameAs(error));
        });
    }

    [Test]
    public async Task GetAsync_MissingWorkout_ReturnsFailed()
    {
        Workout(Start, Sets(Squat, Set(5, 60m)));
        Setup();

        Assert.That((await service.GetAsync(99)).Failed, Is.True);
    }

    private Workout Workout(DateTimeOffset startedAt, params WorkoutExercise[] exercises)
    {
        var workout = new Workout(++nextId, startedAt, startedAt.AddHours(1), exercises);
        workouts.Add(workout);
        return workout;
    }

    private void Setup() => repository.Setup(r => r.GetAllAsync()).ReturnsAsync(workouts.OrderBy(w => w.StartedAt).ToList());
}
