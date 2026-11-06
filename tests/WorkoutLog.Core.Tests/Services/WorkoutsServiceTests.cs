using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using Microsoft.Extensions.Time.Testing;
using Moq;
using WorkoutLog.Core.Data;
using WorkoutLog.Core.Domain.Workouts;
using WorkoutLog.Core.Logging;
using WorkoutLog.Core.Services;
using WorkoutLog.Core.Tests.Domain.Workouts.Consistency;

namespace WorkoutLog.Core.Tests.Services;

public class WorkoutsServiceTests
{
    private static readonly DateTimeOffset Start = new(2026, 10, 21, 17, 5, 0, TimeSpan.Zero);

    private Mock<IWorkoutRepository> repository = null!;
    private FakeLogger<WorkoutsService> logger = null!;
    private WorkoutsService service = null!;

    [SetUp]
    public void SetUp()
    {
        repository = new Mock<IWorkoutRepository>(MockBehavior.Strict);
        logger = new FakeLogger<WorkoutsService>();
        var clock = new FakeTimeProvider(Start);
        clock.SetLocalTimeZone(SummerTime());
        service = new WorkoutsService(repository.Object, clock, logger);
    }

    [Test]
    public async Task GetBetweenAsync_ReadsFromLocalMidnightOfFromToTheLastTickOfTo()
    {
        WorkoutSummary[] workouts = [new(2, Start), new(1, Start.AddDays(-1))];
        repository.Setup(r => r.ListAsync(
                new DateTimeOffset(2026, 10, 20, 0, 0, 0, TimeSpan.FromHours(1)),
                new DateTimeOffset(2026, 10, 21, 23, 59, 59, TimeSpan.FromHours(1)).AddTicks(TimeSpan.TicksPerSecond - 1),
                null))
            .ReturnsAsync(workouts);

        var result = await service.GetBetweenAsync(new DateOnly(2026, 10, 20), new DateOnly(2026, 10, 21));

        Assert.That(result.Failed, Is.False);
        Assert.That(result.Workouts, Is.EqualTo(workouts));
    }

    [Test]
    public async Task GetBetweenAsync_AcrossTheClockChange_UsesEachDaysOwnOffset()
    {
        repository.Setup(r => r.ListAsync(
                new DateTimeOffset(2026, 10, 24, 0, 0, 0, TimeSpan.FromHours(1)),
                new DateTimeOffset(2026, 10, 26, 0, 0, 0, TimeSpan.Zero).AddTicks(-1),
                null))
            .ReturnsAsync([]);

        var result = await service.GetBetweenAsync(new DateOnly(2026, 10, 24), new DateOnly(2026, 10, 25));

        Assert.That(result.Failed, Is.False);
    }

    [TestCase(0)]
    [TestCase(2)]
    public async Task GetBetweenAsync_LogsTheCountAtDebug(int count)
    {
        repository.Setup(r => r.ListAsync(It.IsAny<DateTimeOffset?>(), It.IsAny<DateTimeOffset?>(), null))
            .ReturnsAsync(Enumerable.Range(1, count).Select(id => new WorkoutSummary(id, Start)).ToList());

        await service.GetBetweenAsync(new DateOnly(2026, 10, 21), new DateOnly(2026, 10, 21));

        Assert.That(LoggedEvents.Of(logger), Is.EqualTo(new[] { (CoreEvent.WorkoutsLoaded, LogLevel.Debug, $"Loaded {count} workouts") }));
        Assert.That(logger.LatestRecord.Exception, Is.Null);
    }

    [Test]
    public async Task GetBetweenAsync_RepositoryThrows_ReturnsFailedAndLogsTheError()
    {
        var exception = new InvalidOperationException("disk");
        repository.Setup(r => r.ListAsync(It.IsAny<DateTimeOffset?>(), It.IsAny<DateTimeOffset?>(), null)).ThrowsAsync(exception);

        var result = await service.GetBetweenAsync(new DateOnly(2026, 10, 21), new DateOnly(2026, 10, 21));

        Assert.That(result.Failed, Is.True);
        Assert.That(result.Workouts, Is.Empty);
        Assert.That(LoggedEvents.Of(logger), Is.EqualTo(new[] { (CoreEvent.WorkoutsLoadFailed, LogLevel.Error, "Loading the workouts failed") }));
        Assert.That(logger.LatestRecord.Exception, Is.SameAs(exception));
    }

    [Test]
    public async Task GetDaysAsync_CountsWorkoutsByTheirLocalDay()
    {
        repository.Setup(r => r.ListStartsAsync()).ReturnsAsync(
        [
            new DateTimeOffset(2026, 7, 20, 7, 0, 0, TimeSpan.Zero),
            new DateTimeOffset(2026, 7, 20, 17, 0, 0, TimeSpan.Zero),
            new DateTimeOffset(2026, 7, 20, 22, 59, 59, TimeSpan.Zero),
            new DateTimeOffset(2026, 7, 20, 23, 0, 0, TimeSpan.Zero),
        ]);

        var result = await service.GetDaysAsync();

        Assert.That(result.Failed, Is.False);
        Assert.That(result.PerDay, Is.EquivalentTo(new Dictionary<DateOnly, int>
        {
            [new DateOnly(2026, 7, 20)] = 3,
            [new DateOnly(2026, 7, 21)] = 1,
        }));
    }

    [Test]
    public async Task GetDaysAsync_LateWorkouts_UseTheOffsetOfTheirOwnDateNotTodays()
    {
        repository.Setup(r => r.ListStartsAsync()).ReturnsAsync(
        [
            new DateTimeOffset(2026, 7, 20, 23, 30, 0, TimeSpan.Zero),
            new DateTimeOffset(2026, 12, 20, 23, 30, 0, TimeSpan.Zero),
        ]);

        var result = await service.GetDaysAsync();

        Assert.That(result.PerDay.Keys, Is.EquivalentTo(new[] { new DateOnly(2026, 7, 21), new DateOnly(2026, 12, 20) }));
    }

    [Test]
    public async Task GetDaysAsync_LogsTheCountsAtDebug()
    {
        repository.Setup(r => r.ListStartsAsync()).ReturnsAsync([Start, Start.AddHours(1), Start.AddDays(-1)]);

        await service.GetDaysAsync();

        Assert.That(LoggedEvents.Of(logger), Is.EqualTo(new[] { (CoreEvent.WorkoutDaysLoaded, LogLevel.Debug, "Counted 3 workouts on 2 days") }));
    }

    [Test]
    public async Task GetDaysAsync_RepositoryThrows_ReturnsFailedAndLogsTheError()
    {
        var exception = new InvalidOperationException("disk");
        repository.Setup(r => r.ListStartsAsync()).ThrowsAsync(exception);

        var result = await service.GetDaysAsync();

        Assert.That(result.Failed, Is.True);
        Assert.That(result.PerDay, Is.Empty);
        Assert.That(LoggedEvents.Of(logger), Is.EqualTo(new[] { (CoreEvent.WorkoutDaysLoadFailed, LogLevel.Error, "Counting the workouts per day failed") }));
        Assert.That(logger.LatestRecord.Exception, Is.SameAs(exception));
    }

    [Test]
    public async Task MidnightSkipped_BoundsAndDaysAgreeOnTheFirstInstantOfTheDay()
    {
        var service = InZone(ConsistencyZones.MidnightSkippedOn5Oct);
        var first = new DateTimeOffset(2026, 10, 5, 2, 30, 0, TimeSpan.Zero);
        repository.Setup(r => r.ListAsync(first, It.IsAny<DateTimeOffset?>(), null)).ReturnsAsync([]);
        repository.Setup(r => r.ListStartsAsync()).ReturnsAsync([first, first.AddTicks(-1)]);

        await service.GetBetweenAsync(new DateOnly(2026, 10, 5), new DateOnly(2026, 10, 5));
        var days = await service.GetDaysAsync();

        Assert.That(days.PerDay.Keys, Is.EquivalentTo(new[] { new DateOnly(2026, 10, 4), new DateOnly(2026, 10, 5) }));
        repository.Verify(r => r.ListAsync(first, It.IsAny<DateTimeOffset?>(), null), Times.Once);
    }

    [Test]
    public async Task MidnightTwice_BoundsAndDaysAgreeThatTheDayStartsAtTheFirstMidnight()
    {
        var service = InZone(ConsistencyZones.MidnightTwiceOn26Oct);
        var first = new DateTimeOffset(2026, 10, 25, 22, 0, 0, TimeSpan.Zero);
        repository.Setup(r => r.ListAsync(It.IsAny<DateTimeOffset?>(), first.AddTicks(-1), null)).ReturnsAsync([]);
        repository.Setup(r => r.ListStartsAsync()).ReturnsAsync([first.AddTicks(-1), first, first.AddMinutes(90)]);

        await service.GetBetweenAsync(new DateOnly(2026, 10, 25), new DateOnly(2026, 10, 25));
        var days = await service.GetDaysAsync();

        Assert.That(days.PerDay, Is.EquivalentTo(new Dictionary<DateOnly, int> { [new DateOnly(2026, 10, 25)] = 1, [new DateOnly(2026, 10, 26)] = 2 }));
        repository.Verify(r => r.ListAsync(It.IsAny<DateTimeOffset?>(), first.AddTicks(-1), null), Times.Once);
    }

    private WorkoutsService InZone(TimeZoneInfo zone)
    {
        var clock = new FakeTimeProvider(Start);
        clock.SetLocalTimeZone(zone);
        return new WorkoutsService(repository.Object, clock, logger);
    }

    /// <summary>UTC in winter and UTC+1 from 01:00 on 29 Mar to 02:00 on 25 Oct, like London in 2026.</summary>
    private static TimeZoneInfo SummerTime() =>
        TimeZoneInfo.CreateCustomTimeZone("Summer", TimeSpan.Zero, "Summer", "Winter", "Summer",
        [
            TimeZoneInfo.AdjustmentRule.CreateAdjustmentRule(DateTime.MinValue.Date, DateTime.MaxValue.Date, TimeSpan.FromHours(1),
                TimeZoneInfo.TransitionTime.CreateFixedDateRule(new DateTime(1, 1, 1, 1, 0, 0), 3, 29),
                TimeZoneInfo.TransitionTime.CreateFixedDateRule(new DateTime(1, 1, 1, 2, 0, 0), 10, 25)),
        ]);
}
