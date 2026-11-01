using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using Moq;
using WorkoutLog.Core.Data;
using WorkoutLog.Core.Domain.Exercises;
using WorkoutLog.Core.Domain.Workouts;
using WorkoutLog.Core.Logging;
using WorkoutLog.Core.Services;

namespace WorkoutLog.Core.Tests.Services;

public class LastWorkoutServiceTests
{
    private static readonly IExerciseCatalog Catalog = new ExerciseCatalog();
    private static readonly DateTimeOffset Start = new(2026, 10, 27, 18, 5, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset Finish = Start.AddMinutes(72);

    private Mock<IWorkoutRepository> repository = null!;
    private FakeLogger<LastWorkoutService> logger = null!;
    private LastWorkoutService service = null!;

    [SetUp]
    public void SetUp()
    {
        repository = new Mock<IWorkoutRepository>(MockBehavior.Strict);
        logger = new FakeLogger<LastWorkoutService>();
        service = new LastWorkoutService(repository.Object, Catalog, logger);
    }

    [Test]
    public async Task GetAsync_NoWorkouts_ReturnsNoneAndLogsLoaded()
    {
        repository.Setup(r => r.GetLatestAsync()).ReturnsAsync((WorkoutOutline?)null);

        Assert.That(await service.GetAsync(), Is.EqualTo(new LastWorkoutResult(null, Failed: false)));
        Assert.That(LoggedEvents.Of(logger), Is.EqualTo(new[] { (CoreEvent.LastWorkoutLoaded, LogLevel.Information, "Last workout loaded") }));
    }

    [Test]
    public async Task GetAsync_ReturnsTheWorkoutWithItsExercisesInOrder()
    {
        repository.Setup(r => r.GetLatestAsync()).ReturnsAsync(new WorkoutOutline(4, Start, Finish, [300, 1, 187]));

        var result = await service.GetAsync();

        Assert.That(result, Is.EqualTo(new LastWorkoutResult(new WorkoutOutline(4, Start, Finish, [300, 1, 187]), Failed: false))
            .UsingPropertiesComparer());
        Assert.That(LoggedEvents.Of(logger), Is.EqualTo(new[] { (CoreEvent.LastWorkoutLoaded, LogLevel.Information, "Last workout loaded") }));
    }

    [Test]
    public async Task GetAsync_UnknownExercise_IsLeftOutAndLoggedWithItsId()
    {
        repository.Setup(r => r.GetLatestAsync()).ReturnsAsync(new WorkoutOutline(4, Start, Finish, [1, 9999, 300]));

        var result = await service.GetAsync();

        Assert.That(result.Workout!.ExerciseIds, Is.EqualTo(new[] { 1, 300 }));
        Assert.That(LoggedEvents.Of(logger), Is.EqualTo(new[]
        {
            (CoreEvent.LastWorkoutUnknownExercise, LogLevel.Warning, "Exercise 9999 is not in the exercise list; left out"),
            (CoreEvent.LastWorkoutLoaded, LogLevel.Information, "Last workout loaded"),
        }));
    }

    [Test]
    public async Task GetAsync_ReadFails_ReturnsFailedAndLogsAWarningWithTheException()
    {
        var error = new InvalidOperationException("disk");
        repository.Setup(r => r.GetLatestAsync()).ThrowsAsync(error);

        var result = await service.GetAsync();

        Assert.That(result, Is.EqualTo(new LastWorkoutResult(null, Failed: true)));
        Assert.Multiple(() =>
        {
            Assert.That(LoggedEvents.Of(logger), Is.EqualTo(new[] { (CoreEvent.LastWorkoutLoadFailed, LogLevel.Warning, "Loading the last workout failed") }));
            Assert.That(logger.Collector.LatestRecord.Exception, Is.SameAs(error));
        });
    }
}
