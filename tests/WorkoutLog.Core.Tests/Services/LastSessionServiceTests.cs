using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using Moq;
using WorkoutLog.Core.Data;
using WorkoutLog.Core.Domain.Exercises.Progress;
using WorkoutLog.Core.Domain.Workouts;
using WorkoutLog.Core.Logging;
using WorkoutLog.Core.Services;

namespace WorkoutLog.Core.Tests.Services;

public class LastSessionServiceTests
{
    /// <summary>Barbell Back Squat.</summary>
    private const int Squat = 6;

    /// <summary>Plank, a hold.</summary>
    private const int Plank = 258;

    /// <summary>Treadmill, cardio.</summary>
    private const int Treadmill = 300;

    private static readonly DateTimeOffset Started = new(2026, 10, 20, 18, 5, 0, TimeSpan.Zero);

    private Mock<IWorkoutRepository> repository = null!;
    private FakeLogger<LastSessionService> logger = null!;
    private LastSessionService service = null!;

    [SetUp]
    public void SetUp()
    {
        repository = new Mock<IWorkoutRepository>(MockBehavior.Strict);
        logger = new FakeLogger<LastSessionService>();
        service = new LastSessionService(repository.Object, new CardioSessionRule(), logger);
    }

    [Test]
    public async Task GetAsync_NoEarlierWorkout_ReturnsNoExercise()
    {
        Setup(Squat, null, null);

        Assert.That(await service.GetAsync(Squat), Is.EqualTo(new LastSessionResult(null, null, Failed: false)));
    }

    [Test]
    public async Task GetAsync_Strength_ReturnsTheSetsInOrder()
    {
        var sets = new[] { new StrengthSet(8, 60m, null, null), new StrengthSet(6, 62.5m, null, null) };
        Setup(Squat, null, new StrengthExercise(Squat, sets));

        var result = await service.GetAsync(Squat);

        Assert.Multiple(() =>
        {
            Assert.That(((StrengthExercise)result.Exercise!).Sets, Is.EqualTo(sets));
            Assert.That(result.Best, Is.Null);
            Assert.That(result.Failed, Is.False);
        });
    }

    [Test]
    public async Task GetAsync_WeightedHold_ReturnsTheWeightWithTheDuration()
    {
        var sets = new[] { new StrengthSet(null, 20m, TimeSpan.FromSeconds(90), null) };
        Setup(Plank, null, new StrengthExercise(Plank, sets));

        Assert.That(((StrengthExercise)(await service.GetAsync(Plank)).Exercise!).Sets, Is.EqualTo(sets));
    }

    [Test]
    public async Task GetAsync_UnweightedHold_ReturnsTheDurationAlone()
    {
        var sets = new[] { new StrengthSet(null, 0m, TimeSpan.FromSeconds(90), null) };
        Setup(Plank, null, new StrengthExercise(Plank, sets));

        Assert.That(((StrengthExercise)(await service.GetAsync(Plank)).Exercise!).Sets, Is.EqualTo(sets));
    }

    [Test]
    public async Task GetAsync_Cardio_ReturnsTheEntryWithItsPaceAndSpeedWorkedOut()
    {
        var entry = new CardioEntry(new TimeSpan(0, 30, 0), 5m, null, null, null, 310, null);
        Setup(Treadmill, null, new CardioExercise(Treadmill, entry));

        var result = await service.GetAsync(Treadmill);

        Assert.Multiple(() =>
        {
            Assert.That(((CardioExercise)result.Exercise!).Entry, Is.EqualTo(entry));
            Assert.That(result.Best, Is.EqualTo(new CardioBest(5m, new TimeSpan(0, 30, 0), new TimeSpan(0, 6, 0), 10m, null, 310)));
            Assert.That(result.Failed, Is.False);
        });
    }

    [Test]
    public async Task GetAsync_Before_AsksForTheWorkoutsStartedBeforeIt()
    {
        var exercise = new StrengthExercise(Squat, [new StrengthSet(8, 55m, null, null)]);
        Setup(Squat, Started, exercise);

        Assert.That((await service.GetAsync(Squat, Started)).Exercise, Is.SameAs(exercise));
    }

    [Test]
    public async Task GetAsync_Loaded_LogsNothing()
    {
        Setup(Squat, null, new StrengthExercise(Squat, [new StrengthSet(8, 60m, null, null)]));

        await service.GetAsync(Squat);

        Assert.That(LoggedEvents.Of(logger), Is.Empty);
    }

    [Test]
    public async Task GetAsync_RepositoryThrows_ReturnsFailedWithNoExercise()
    {
        repository.Setup(r => r.GetLastSessionOfAsync(Squat, null)).ThrowsAsync(new InvalidOperationException("disk"));

        Assert.That(await service.GetAsync(Squat), Is.EqualTo(new LastSessionResult(null, null, Failed: true)));
    }

    [Test]
    public async Task GetAsync_RepositoryThrows_LogsAWarningWithTheExerciseIdAndException()
    {
        var exception = new InvalidOperationException("disk");
        repository.Setup(r => r.GetLastSessionOfAsync(Squat, null)).ThrowsAsync(exception);

        await service.GetAsync(Squat);

        Assert.That(LoggedEvents.Of(logger), Is.EqualTo(new[]
        {
            (CoreEvent.LastSessionLoadFailed, LogLevel.Warning, $"Loading the last session of exercise {Squat} failed"),
        }));
        Assert.That(logger.LatestRecord.Exception, Is.SameAs(exception));
    }

    /// <summary>The repository returns <paramref name="exercise"/> as the last session of the exercise before <paramref name="before"/>; none when null.</summary>
    private void Setup(int exerciseId, DateTimeOffset? before, WorkoutExercise? exercise) =>
        repository.Setup(r => r.GetLastSessionOfAsync(exerciseId, before))
            .ReturnsAsync(exercise is null ? null : new LastSession(exerciseId, Started.AddDays(-3), exercise));
}
