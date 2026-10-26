using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using Moq;
using WorkoutLog.Core.Data;
using WorkoutLog.Core.Domain.Exercises;
using WorkoutLog.Core.Domain.Workouts;
using WorkoutLog.Core.Logging;
using WorkoutLog.Core.Services;

namespace WorkoutLog.Core.Tests.Services;

public class WeightHistoryServiceTests
{
    /// <summary>Barbell Back Squat: heaviest is best.</summary>
    private const int Squat = 6;

    /// <summary>Machine Assisted Pull-up: weight labelled Assist, lowest is best.</summary>
    private const int AssistedPullUp = 218;

    private static readonly IExerciseCatalog Catalog = new ExerciseCatalog();

    private Mock<IWorkoutRepository> repository = null!;
    private FakeLogger<WeightHistoryService> logger = null!;
    private WeightHistoryService service = null!;

    [SetUp]
    public void SetUp()
    {
        repository = new Mock<IWorkoutRepository>(MockBehavior.Strict);
        logger = new FakeLogger<WeightHistoryService>();
        service = new WeightHistoryService(repository.Object, Catalog, logger);
    }

    [Test]
    public void Exercises_AreTheOnesNamed()
    {
        Assert.That(Catalog.All.Single(e => e.Id == Squat).Name, Is.EqualTo("Barbell Back Squat"));
        Assert.That(Catalog.All.Single(e => e.Id == AssistedPullUp).Name, Is.EqualTo("Machine Assisted Pull-up"));
    }

    [Test]
    public async Task GetAsync_NoSavedWorkout_ReturnsNoWeights()
    {
        Setup(Squat);

        Assert.That(await service.GetAsync(Squat), Is.EqualTo(new WeightHistoryResult(null, null, Failed: false)));
    }

    [Test]
    public async Task GetAsync_OneWorkout_LastAndBestAreItsHeaviest()
    {
        Setup(Squat, [60m, 62.5m, 57.5m]);

        Assert.That(await service.GetAsync(Squat), Is.EqualTo(new WeightHistoryResult(62.5m, 62.5m, Failed: false)));
    }

    [Test]
    public async Task GetAsync_ManyWorkouts_LastIsTheFirstsHeaviestAndBestTheHeaviestOfAll()
    {
        Setup(Squat, [60m, 62.5m], [70m], [65m]);

        Assert.That(await service.GetAsync(Squat), Is.EqualTo(new WeightHistoryResult(62.5m, 70m, Failed: false)));
    }

    [TestCase(new[] { 75, 60, 62.5 }, new[] { 70.0 }, new[] { 65.0 }, 75, 75, TestName = "GetAsync_PbInTheLastWorkoutsFirstSet")]
    [TestCase(new[] { 60, 62.5, 75 }, new[] { 70.0 }, new[] { 65.0 }, 75, 75, TestName = "GetAsync_PbInTheLastWorkoutsLastSet")]
    [TestCase(new[] { 60, 62.5, 57.5 }, new[] { 65.0 }, new[] { 70.0 }, 62.5, 70, TestName = "GetAsync_PbInTheOldestWorkout_Regressing")]
    [TestCase(new[] { 55.0 }, new[] { 70, 72.5, 60 }, new[] { 65.0 }, 55, 72.5, TestName = "GetAsync_PbInAMiddleWorkoutsMiddleSet")]
    public async Task GetAsync_PbAnywhere_LastIsTheLatestsHeaviestAndBestTheHeaviestOfAll(
        double[] latest, double[] middle, double[] oldest, double expectedLast, double expectedBest)
    {
        Setup(Squat, [.. latest.Select(w => (decimal)w)], [.. middle.Select(w => (decimal)w)], [.. oldest.Select(w => (decimal)w)]);

        Assert.That(await service.GetAsync(Squat), Is.EqualTo(new WeightHistoryResult((decimal)expectedLast, (decimal)expectedBest, Failed: false)));
    }

    [TestCase(new[] { 15, 25, 20.0 }, new[] { 10.0 }, new[] { 30.0 }, 15, 10, TestName = "GetAsync_Assisted_BestInTheLastWorkoutsFirstSet_PbInTheMiddle")]
    [TestCase(new[] { 30, 25, 20.0 }, new[] { 25.0 }, new[] { 10.0 }, 20, 10, TestName = "GetAsync_Assisted_PbInTheOldestWorkout_Regressing")]
    [TestCase(new[] { 25, 20, 5.0 }, new[] { 15.0 }, new[] { 10.0 }, 5, 5, TestName = "GetAsync_Assisted_PbInTheLastWorkoutsLastSet")]
    public async Task GetAsync_AssistedPbAnywhere_TakesTheLowest(
        double[] latest, double[] middle, double[] oldest, double expectedLast, double expectedBest)
    {
        Setup(AssistedPullUp, [.. latest.Select(w => (decimal)w)], [.. middle.Select(w => (decimal)w)], [.. oldest.Select(w => (decimal)w)]);

        Assert.That(await service.GetAsync(AssistedPullUp), Is.EqualTo(new WeightHistoryResult((decimal)expectedLast, (decimal)expectedBest, Failed: false)));
    }

    [Test]
    public async Task GetAsync_LastWorkoutHasNoWeight_ReturnsTheBestOnly()
    {
        Setup(Squat, [], [70m]);

        Assert.That(await service.GetAsync(Squat), Is.EqualTo(new WeightHistoryResult(null, 70m, Failed: false)));
    }

    [Test]
    public async Task GetAsync_NoWorkoutHasAWeight_ReturnsNoWeights()
    {
        Setup(Squat, [], []);

        Assert.That(await service.GetAsync(Squat), Is.EqualTo(new WeightHistoryResult(null, null, Failed: false)));
    }

    [Test]
    public async Task GetAsync_ZeroKg_CountsAsAWeight()
    {
        Setup(Squat, [0m]);

        Assert.That(await service.GetAsync(Squat), Is.EqualTo(new WeightHistoryResult(0m, 0m, Failed: false)));
    }

    [Test]
    public async Task GetAsync_AssistedMachine_TakesTheLowest()
    {
        Setup(AssistedPullUp, [25m, 20m], [15m], [30m]);

        Assert.That(await service.GetAsync(AssistedPullUp), Is.EqualTo(new WeightHistoryResult(20m, 15m, Failed: false)));
    }

    [Test]
    public async Task GetAsync_AssistedMachineWithZeroKg_ZeroIsTheBest()
    {
        Setup(AssistedPullUp, [10m], [0m, 5m]);

        Assert.That(await service.GetAsync(AssistedPullUp), Is.EqualTo(new WeightHistoryResult(10m, 0m, Failed: false)));
    }

    [Test]
    public async Task GetAsync_UnknownExercise_TakesTheHeaviest()
    {
        Setup(9999, [10m, 20m]);

        Assert.That(await service.GetAsync(9999), Is.EqualTo(new WeightHistoryResult(20m, 20m, Failed: false)));
    }

    [Test]
    public async Task GetAsync_Loaded_LogsNothing()
    {
        Setup(Squat, [60m]);

        await service.GetAsync(Squat);

        Assert.That(LoggedEvents.Of(logger), Is.Empty);
    }

    [Test]
    public async Task GetAsync_RepositoryThrows_ReturnsFailedWithNoWeights()
    {
        repository.Setup(r => r.GetWeightsAsync(Squat)).ThrowsAsync(new InvalidOperationException("disk"));

        Assert.That(await service.GetAsync(Squat), Is.EqualTo(new WeightHistoryResult(null, null, Failed: true)));
    }

    [Test]
    public async Task GetAsync_RepositoryThrows_LogsTheErrorWithTheExerciseIdAndException()
    {
        var exception = new InvalidOperationException("disk");
        repository.Setup(r => r.GetWeightsAsync(Squat)).ThrowsAsync(exception);

        await service.GetAsync(Squat);

        Assert.That(LoggedEvents.Of(logger), Is.EqualTo(new[]
        {
            (CoreEvent.WeightHistoryLoadFailed, LogLevel.Error, $"Loading the weight history of exercise {Squat} failed"),
        }));
        Assert.That(logger.LatestRecord.Exception, Is.SameAs(exception));
    }

    /// <summary>The repository returns these weights for the exercise, one array per workout, newest first.</summary>
    private void Setup(int exerciseId, params decimal[][] workouts) =>
        repository.Setup(r => r.GetWeightsAsync(exerciseId))
            .ReturnsAsync(workouts.Select((weights, i) => new WorkoutWeights(workouts.Length - i, weights)).ToList());
}
