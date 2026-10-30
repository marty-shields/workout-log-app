using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using Microsoft.Extensions.Time.Testing;
using Moq;
using WorkoutLog.Core.Data;
using WorkoutLog.Core.Domain.Workouts;
using WorkoutLog.Core.Domain.Workouts.Consistency;
using WorkoutLog.Core.Logging;
using WorkoutLog.Core.Services;
using WorkoutLog.Core.Tests.Domain.Workouts.Consistency;

namespace WorkoutLog.Core.Tests.Services;

public class ConsistencyServiceTests
{
    /// <summary>Wednesday 28 Oct 2026, 18:00 UTC.</summary>
    private static readonly DateTimeOffset Now = new(2026, 10, 28, 18, 0, 0, TimeSpan.Zero);

    /// <summary>Monday 10 Aug 2026: the first of the 12 weeks.</summary>
    private static readonly DateTimeOffset From = new(2026, 8, 10, 0, 0, 0, TimeSpan.Zero);

    private Mock<IWorkoutRepository> repository = null!;
    private FakeLogger<ConsistencyService> logger = null!;
    private ConsistencyService service = null!;

    [SetUp]
    public void SetUp()
    {
        repository = new Mock<IWorkoutRepository>(MockBehavior.Strict);
        logger = new FakeLogger<ConsistencyService>();
        UseClock(Now, TimeZoneInfo.Utc);
    }

    [Test]
    public async Task GetAsync_ReadsThe12WeeksOnceAndSummarisesThem()
    {
        repository.Setup(r => r.ListTalliesAsync(From, Now)).ReturnsAsync(
        [
            Workout(From, 10),
            Workout(Now.AddDays(-7).AddHours(-2), 8),
            Workout(Now.AddDays(-8), 6),
            Workout(Now.AddHours(-3), 12),
        ]);

        var result = await service.GetAsync();

        Assert.Multiple(() =>
        {
            Assert.That(result.Comparison, Is.EqualTo(new WeekComparison(
                new WeekTally(1, TimeSpan.FromMinutes(45), 12),
                new WeekTally(2, TimeSpan.FromMinutes(90), 14))));
            Assert.That(result.WeekCounts, Is.EqualTo(new[] { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2, 1 }));
            Assert.That(result.Streak, Is.EqualTo(2));
            Assert.That(result.Failed, Is.False);
            repository.Verify(r => r.ListTalliesAsync(From, Now), Times.Once);
        });
    }

    [Test]
    public async Task GetAsync_ReadsFromMondayInThePhonesTimeZone()
    {
        var now = new DateTimeOffset(2026, 10, 26, 0, 0, 0, TimeSpan.FromHours(10));
        var from = new DateTimeOffset(2026, 8, 10, 0, 0, 0, TimeSpan.FromHours(10));
        UseClock(now, ConsistencyZones.Plus10);
        repository.Setup(r => r.ListTalliesAsync(from, now)).ReturnsAsync([Workout(now, 3)]);

        var result = await service.GetAsync();

        Assert.That(result.WeekCounts[^1], Is.EqualTo(1));
    }

    [Test]
    public async Task GetAsync_NoWorkouts_ReturnsZerosAndNoStreak()
    {
        repository.Setup(r => r.ListTalliesAsync(From, Now)).ReturnsAsync([]);

        var result = await service.GetAsync();

        Assert.That(result, Is.EqualTo(new ConsistencyResult(new WeekComparison(WeekTally.None, WeekTally.None), result.WeekCounts, 0, Failed: false)));
        Assert.That(result.WeekCounts, Is.EqualTo(new int[12]));
    }

    [Test]
    public async Task GetAsync_LogsTheCountAtInformation()
    {
        repository.Setup(r => r.ListTalliesAsync(From, Now)).ReturnsAsync([Workout(Now.AddDays(-7), 4), Workout(Now, 5)]);

        await service.GetAsync();

        Assert.That(LoggedEvents.Of(logger), Is.EqualTo(new[]
        {
            (CoreEvent.ConsistencyLoaded, LogLevel.Information, "Consistency loaded: 2 workouts in the last 12 weeks, 2-week streak"),
        }));
        Assert.That(logger.LatestRecord.Exception, Is.Null);
    }

    [Test]
    public async Task GetAsync_ReadThrows_ReturnsFailedAndLogsAWarning()
    {
        var exception = new InvalidOperationException("disk");
        repository.Setup(r => r.ListTalliesAsync(From, Now)).ThrowsAsync(exception);

        var result = await service.GetAsync();

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.SameAs(ConsistencyResult.FailedRead));
            Assert.That(result.Failed, Is.True);
            Assert.That(result.WeekCounts, Is.EqualTo(new int[12]));
            Assert.That(LoggedEvents.Of(logger), Is.EqualTo(new[] { (CoreEvent.ConsistencyLoadFailed, LogLevel.Warning, "Loading the Consistency card failed") }));
            Assert.That(logger.LatestRecord.Exception, Is.SameAs(exception));
        });
    }

    [Test]
    public async Task GetAsync_ClockThrows_ReturnsFailedAndLogsAWarning()
    {
        var exception = new InvalidOperationException("clock");
        var time = new Mock<TimeProvider>();
        time.Setup(t => t.GetUtcNow()).Throws(exception);
        service = new ConsistencyService(repository.Object, new WeekComparisonRule(), new WeekBarsRule(), new StreakRule(), time.Object, logger);

        var result = await service.GetAsync();

        Assert.Multiple(() =>
        {
            Assert.That(result.Failed, Is.True);
            Assert.That(logger.LatestRecord.Exception, Is.SameAs(exception));
        });
    }

    private void UseClock(DateTimeOffset now, TimeZoneInfo zone)
    {
        var time = new FakeTimeProvider(now);
        time.SetLocalTimeZone(zone);
        service = new ConsistencyService(repository.Object, new WeekComparisonRule(), new WeekBarsRule(), new StreakRule(), time, logger);
    }

    private static WorkoutTally Workout(DateTimeOffset startedAt, int sets) => new(startedAt, startedAt.AddMinutes(45), sets);
}
