using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using Moq;
using WorkoutLog.Core.Data;
using WorkoutLog.Core.Domain.Workouts;
using WorkoutLog.Core.Logging;
using WorkoutLog.Core.Services;

namespace WorkoutLog.Core.Tests.Services;

public class HistoryServiceTests
{
    private static readonly DateTimeOffset Start = new(2026, 10, 21, 17, 5, 0, TimeSpan.Zero);

    private Mock<IWorkoutRepository> repository = null!;
    private FakeLogger<HistoryService> logger = null!;
    private HistoryService service = null!;

    [SetUp]
    public void SetUp()
    {
        repository = new Mock<IWorkoutRepository>(MockBehavior.Strict);
        logger = new FakeLogger<HistoryService>();
        service = new HistoryService(repository.Object, logger);
    }

    [Test]
    public async Task GetAllAsync_ReturnsTheRepositorysWorkoutsInOrder()
    {
        WorkoutSummary[] workouts = [new(2, Start), new(1, Start.AddDays(-1))];
        repository.Setup(r => r.ListAsync()).ReturnsAsync(workouts);

        var result = await service.GetAllAsync();

        Assert.That(result.Failed, Is.False);
        Assert.That(result.Workouts, Is.EqualTo(workouts));
    }

    [TestCase(0)]
    [TestCase(2)]
    public async Task GetAllAsync_LogsTheCountAtDebug(int count)
    {
        repository.Setup(r => r.ListAsync()).ReturnsAsync(Enumerable.Range(1, count).Select(id => new WorkoutSummary(id, Start)).ToList());

        await service.GetAllAsync();

        Assert.That(LoggedEvents.Of(logger), Is.EqualTo(new[] { (CoreEvent.HistoryLoaded, LogLevel.Debug, $"History loaded with {count} workouts") }));
        Assert.That(logger.LatestRecord.Exception, Is.Null);
    }

    [Test]
    public async Task GetAllAsync_RepositoryThrows_ReturnsFailedWithNoWorkouts()
    {
        repository.Setup(r => r.ListAsync()).ThrowsAsync(new InvalidOperationException("disk"));

        var result = await service.GetAllAsync();

        Assert.That(result.Failed, Is.True);
        Assert.That(result.Workouts, Is.Empty);
    }

    [Test]
    public async Task GetAllAsync_RepositoryThrows_LogsTheErrorWithTheException()
    {
        var exception = new InvalidOperationException("disk");
        repository.Setup(r => r.ListAsync()).ThrowsAsync(exception);

        await service.GetAllAsync();

        Assert.That(LoggedEvents.Of(logger), Is.EqualTo(new[] { (CoreEvent.HistoryLoadFailed, LogLevel.Error, "Loading the history failed") }));
        Assert.That(logger.LatestRecord.Exception, Is.SameAs(exception));
    }
}
