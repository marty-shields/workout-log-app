using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using Moq;
using WorkoutLog.Core.Data;
using WorkoutLog.Core.Domain.Exercises;
using WorkoutLog.Core.Domain.Exercises.Bests;
using WorkoutLog.Core.Domain.Workouts;
using WorkoutLog.Core.Logging;
using WorkoutLog.Core.Services;

namespace WorkoutLog.Core.Tests.Services;

public class LoggedExercisesServiceTests
{
    /// <summary>Barbell Back Squat.</summary>
    private const int Squat = 6;

    /// <summary>Dumbbell Curl.</summary>
    private const int Curl = 132;

    /// <summary>Push-up.</summary>
    private const int PushUp = 264;

    /// <summary>Outdoor Run.</summary>
    private const int Run = 311;

    private static readonly DateTimeOffset Start = new(2026, 10, 21, 17, 5, 0, TimeSpan.Zero);
    private static readonly IExerciseCatalog Catalog = new ExerciseCatalog();

    private static readonly IBestRule[] Rules =
        [new LiftBestRule(), new BodyweightBestRule(), new AssistedBestRule(), new HoldBestRule(), new CarryBestRule(), new CardioBestRule()];

    private Mock<IWorkoutRepository> repository = null!;
    private FakeLogger<LoggedExercisesService> logger = null!;
    private LoggedExercisesService service = null!;

    [SetUp]
    public void SetUp()
    {
        repository = new Mock<IWorkoutRepository>(MockBehavior.Strict);
        logger = new FakeLogger<LoggedExercisesService>();
        service = new LoggedExercisesService(repository.Object, Catalog, Rules, logger);
    }

    [Test]
    public void Exercises_AreTheOnesNamed() =>
        Assert.That(new[] { Squat, Curl, PushUp, Run }.Select(id => Catalog.All.Single(e => e.Id == id).Name),
            Is.EqualTo(new[] { "Barbell Back Squat", "Dumbbell Curl", "Push-up", "Outdoor Run" }));

    [Test]
    public void Rules_EachExerciseInTheListMatchesOne() =>
        Assert.That(Catalog.All.Where(e => Rules.Count(r => r.AppliesTo(e)) != 1).Select(e => e.Name), Is.Empty);

    [Test]
    public async Task GetAsync_NothingLogged_ReturnsEmpty()
    {
        Setup();

        Assert.That(await service.GetAsync(), Is.EqualTo(new LoggedExercisesResult([], Failed: false)).UsingPropertiesComparer());
    }

    [Test]
    public async Task GetAsync_ReturnsEachExercisesStartAndBest()
    {
        Setup(
            new LastSession(Squat, Start, [new StrengthExercise(Squat, [new StrengthSet(5, 60m, null, null), new StrengthSet(3, 70m, null, null)])]),
            new LastSession(Run, Start.AddDays(-1), [new CardioExercise(Run, new CardioEntry(TimeSpan.FromMinutes(30), 5.2m, null, null, null, null, null, null))]));

        var result = await service.GetAsync();

        Assert.That(result, Is.EqualTo(new LoggedExercisesResult(
        [
            new LoggedExercise(Squat, Start, new ExerciseBest(70m, 3, null, null)),
            new LoggedExercise(Run, Start.AddDays(-1), new ExerciseBest(null, null, null, 5.2m)),
        ], Failed: false)).UsingPropertiesComparer());
    }

    [Test]
    public async Task GetAsync_NothingToShow_HasNoBest()
    {
        Setup(new LastSession(Squat, Start, [new StrengthExercise(Squat, [new StrengthSet(5, null, null, null)])]));

        Assert.That((await service.GetAsync()).Exercises.Single(), Is.EqualTo(new LoggedExercise(Squat, Start, null)));
    }

    [Test]
    public async Task GetAsync_OrdersByLatestStartThenSameWorkoutAToZ()
    {
        Setup(
            Session(Run, Start.AddDays(-2)),
            Session(PushUp, Start),
            Session(Squat, Start.AddDays(-1)),
            Session(Curl, Start));

        Assert.That((await service.GetAsync()).Exercises.Select(e => e.ExerciseId), Is.EqualTo(new[] { Curl, PushUp, Squat, Run }));
    }

    [Test]
    public async Task GetAsync_LogsTheCountAtInformation()
    {
        Setup(Session(Squat, Start), Session(Curl, Start));

        await service.GetAsync();

        Assert.That(LoggedEvents.Of(logger), Is.EqualTo(new[] { (CoreEvent.LoggedExercisesLoaded, LogLevel.Information, "Loaded 2 logged exercises") }));
    }

    [Test]
    public async Task GetAsync_UnknownExercise_IsLeftOutAndLoggedWithItsId()
    {
        Setup(Session(Squat, Start), Session(999, Start));

        var result = await service.GetAsync();

        Assert.That(result.Exercises.Select(e => e.ExerciseId), Is.EqualTo(new[] { Squat }));
        Assert.That(LoggedEvents.Of(logger), Is.EqualTo(new[]
        {
            (CoreEvent.LoggedExercisesUnknownExercise, LogLevel.Warning, "Exercise 999 is not in the exercise list; left out"),
            (CoreEvent.LoggedExercisesLoaded, LogLevel.Information, "Loaded 1 logged exercises"),
        }));
    }

    [Test]
    public async Task GetAsync_ReadFails_ReturnsFailedAndLogsAWarningWithTheException()
    {
        var error = new InvalidOperationException("disk");
        repository.Setup(r => r.GetLastSessionsAsync()).ThrowsAsync(error);

        var result = await service.GetAsync();

        Assert.That(result, Is.EqualTo(new LoggedExercisesResult([], Failed: true)).UsingPropertiesComparer());
        var record = logger.Collector.LatestRecord;
        Assert.Multiple(() =>
        {
            Assert.That(LoggedEvents.Of(logger), Is.EqualTo(new[] { (CoreEvent.LoggedExercisesLoadFailed, LogLevel.Warning, "Loading the logged exercises failed") }));
            Assert.That(record.Exception, Is.SameAs(error));
        });
    }

    /// <summary>A last session with one set, or a 30-minute entry for cardio.</summary>
    private static LastSession Session(int exerciseId, DateTimeOffset startedAt) =>
        new(exerciseId, startedAt,
        [
            Catalog.All.FirstOrDefault(e => e.Id == exerciseId)?.Type == ExerciseType.Cardio
                ? new CardioExercise(exerciseId, new CardioEntry(TimeSpan.FromMinutes(30), null, null, null, null, null, null, null))
                : new StrengthExercise(exerciseId, [new StrengthSet(5, 20m, null, null)]),
        ]);

    private void Setup(params LastSession[] sessions) =>
        repository.Setup(r => r.GetLastSessionsAsync()).ReturnsAsync(sessions);
}
