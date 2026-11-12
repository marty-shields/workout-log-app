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

public class WorkoutTotalsServiceTests
{
    private static readonly DateTimeOffset Start = new(2026, 10, 21, 17, 5, 0, TimeSpan.Zero);
    private static readonly DateOnly Monday = new(2026, 10, 12);
    private static readonly DateOnly NextSunday = new(2026, 10, 25);

    private Mock<IWorkoutRepository> repository = null!;
    private FakeLogger<WorkoutTotalsService> logger = null!;
    private WorkoutTotalsService service = null!;

    [SetUp]
    public void SetUp()
    {
        repository = new Mock<IWorkoutRepository>(MockBehavior.Strict);
        logger = new FakeLogger<WorkoutTotalsService>();
        service = InZone(TimeZoneInfo.Utc);
    }

    [Test]
    public async Task GetAsync_AddsUpWorkoutsSetsAndTime()
    {
        SetUpRead(
            Tally(Start, TimeSpan.FromMinutes(45), 10),
            Tally(Start.AddDays(1), TimeSpan.FromMinutes(90), 12),
            Tally(Start.AddDays(2), TimeSpan.FromMinutes(30), 0));

        var result = await service.GetAsync(Monday, NextSunday);

        Assert.That(result.Loaded(), Is.EqualTo(new WorkoutTotals(3, 22, TimeSpan.FromMinutes(165), 1.5)));
    }

    [Test]
    public async Task GetAsync_NothingInTheSpan_IsAllZeroWithoutDividing()
    {
        SetUpRead();

        var result = await service.GetAsync(Monday, NextSunday);

        Assert.That(result.Loaded(), Is.EqualTo(new WorkoutTotals(0, 0, TimeSpan.Zero, 0)));
    }

    [Test]
    public async Task GetAsync_NothingInAOneDaySpan_IsAllZero()
    {
        SetUpRead();

        var result = await service.GetAsync(Monday, Monday);

        Assert.That(result.Loaded(), Is.EqualTo(new WorkoutTotals(0, 0, TimeSpan.Zero, 0)));
    }

    [TestCase(1, 2, 2.0)]
    [TestCase(3, 3, 3.0)]
    [TestCase(7, 7, 7.0)]
    [TestCase(14, 7, 3.5)]
    [TestCase(28, 8, 2.0)]
    public async Task GetAsync_AveragePerWeek_IsWorkoutsOverWeeksWithAtLeastOneWeek(int days, int workouts, double expected)
    {
        SetUpRead([.. Enumerable.Range(0, workouts).Select(i => Tally(Start.AddHours(i), TimeSpan.FromMinutes(30), 1))]);

        var result = await service.GetAsync(Monday, Monday.AddDays(days - 1));

        Assert.That(result.Loaded().AveragePerWeek, Is.EqualTo(expected).Within(1e-9));
    }

    [Test]
    public async Task GetAsync_ASpanUnderAWeek_IsNotInflated()
    {
        SetUpRead(Tally(Start, TimeSpan.FromMinutes(30), 5));

        var result = await service.GetAsync(Monday, Monday.AddDays(2));

        Assert.That(result.Loaded().AveragePerWeek, Is.EqualTo(1.0).Within(1e-9));
    }

    [Test]
    public async Task GetAsync_NoFinishTime_AddsNoTimeButStillCounts()
    {
        SetUpRead(
            new WorkoutTally(Start, default, 4),
            new WorkoutTally(Start.AddDays(1), Start.AddDays(1), 3),
            Tally(Start.AddDays(2), TimeSpan.FromMinutes(20), 2));

        var result = await service.GetAsync(Monday, NextSunday);

        Assert.That(result.Loaded(), Has.Property(nameof(WorkoutTotals.Workouts)).EqualTo(3)
            .And.Property(nameof(WorkoutTotals.Sets)).EqualTo(9)
            .And.Property(nameof(WorkoutTotals.TimeTrained)).EqualTo(TimeSpan.FromMinutes(20)));
    }

    [Test]
    public async Task GetAsync_FinishBeforeStart_AddsNoTime()
    {
        SetUpRead(Tally(Start, TimeSpan.FromMinutes(-15), 4), Tally(Start.AddDays(1), TimeSpan.FromMinutes(50), 1));

        var result = await service.GetAsync(Monday, NextSunday);

        Assert.That(result.Loaded().TimeTrained, Is.EqualTo(TimeSpan.FromMinutes(50)));
    }

    [Test]
    public async Task GetAsync_ReadsFromLocalMidnightOfFromToTheLastTickOfTo()
    {
        var plus10 = InZone(ConsistencyZones.Plus10);
        repository.Setup(r => r.ListTalliesAsync(
                new DateTimeOffset(2026, 10, 12, 0, 0, 0, TimeSpan.FromHours(10)),
                new DateTimeOffset(2026, 10, 25, 23, 59, 59, TimeSpan.FromHours(10)).AddTicks(TimeSpan.TicksPerSecond - 1)))
            .ReturnsAsync([]);

        await plus10.GetAsync(Monday, NextSunday);

        repository.VerifyAll();
    }

    [Test]
    public async Task GetAsync_LateOnTheLastDay_IsInsideTheBounds()
    {
        var plus10 = InZone(ConsistencyZones.Plus10);
        var lateOnLastDay = new DateTimeOffset(2026, 10, 25, 23, 59, 0, TimeSpan.FromHours(10));
        DateTimeOffset upper = default;
        repository.Setup(r => r.ListTalliesAsync(It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>()))
            .Callback((DateTimeOffset _, DateTimeOffset to) => upper = to)
            .ReturnsAsync([Tally(lateOnLastDay, TimeSpan.FromMinutes(1), 1)]);

        var result = await plus10.GetAsync(Monday, NextSunday);

        Assert.Multiple(() =>
        {
            Assert.That(lateOnLastDay, Is.LessThanOrEqualTo(upper));
            Assert.That(lateOnLastDay.AddMinutes(1), Is.GreaterThan(upper));
            Assert.That(result.Loaded().Workouts, Is.EqualTo(1));
        });
    }

    [Test]
    public async Task GetAsync_AcrossTheClockChange_UsesEachDaysOwnOffset()
    {
        var zone = InZone(ConsistencyZones.BackOn25Oct);
        repository.Setup(r => r.ListTalliesAsync(
                new DateTimeOffset(2026, 10, 24, 0, 0, 0, TimeSpan.FromHours(1)),
                new DateTimeOffset(2026, 10, 25, 23, 59, 59, TimeSpan.Zero).AddTicks(TimeSpan.TicksPerSecond - 1)))
            .ReturnsAsync([]);

        await zone.GetAsync(new DateOnly(2026, 10, 24), new DateOnly(2026, 10, 25));

        repository.VerifyAll();
    }

    [Test]
    public async Task GetAsync_TheDayClocksGoForward_SpanEndsAtTheNextLocalMidnight()
    {
        var zone = InZone(ConsistencyZones.ForwardOn25Oct);
        repository.Setup(r => r.ListTalliesAsync(
                new DateTimeOffset(2026, 10, 25, 0, 0, 0, TimeSpan.FromHours(1)),
                new DateTimeOffset(2026, 10, 25, 23, 59, 59, TimeSpan.FromHours(2)).AddTicks(TimeSpan.TicksPerSecond - 1)))
            .ReturnsAsync([]);

        await zone.GetAsync(new DateOnly(2026, 10, 25), new DateOnly(2026, 10, 25));

        repository.VerifyAll();
    }

    [Test]
    public async Task GetAsync_LogsTheCountsAtDebug()
    {
        SetUpRead(Tally(Start, TimeSpan.FromMinutes(45), 10), Tally(Start.AddDays(1), TimeSpan.FromMinutes(45), 5));

        await service.GetAsync(Monday, NextSunday);

        Assert.That(LoggedEvents.Of(logger), Is.EqualTo(new[] { (CoreEvent.WorkoutTotalsLoaded, LogLevel.Debug, "Totalled 2 workouts and 15 sets") }));
    }

    [Test]
    public async Task GetAsync_RepositoryThrows_ReturnsFailedAndLogsTheError()
    {
        var exception = new InvalidOperationException("disk");
        repository.Setup(r => r.ListTalliesAsync(It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>())).ThrowsAsync(exception);

        var result = await service.GetAsync(Monday, NextSunday);

        Assert.Multiple(() =>
        {
            Assert.That(result.Value, Is.InstanceOf<ReadFailed>());
            Assert.That(LoggedEvents.Of(logger), Is.EqualTo(new[] { (CoreEvent.WorkoutTotalsLoadFailed, LogLevel.Error, "Totalling the workouts failed") }));
            Assert.That(logger.LatestRecord.Exception, Is.SameAs(exception));
        });
    }

    private static WorkoutTally Tally(DateTimeOffset start, TimeSpan length, int sets) => new(start, start + length, sets);

    private void SetUpRead(params WorkoutTally[] tallies) =>
        repository.Setup(r => r.ListTalliesAsync(It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>())).ReturnsAsync(tallies);

    private WorkoutTotalsService InZone(TimeZoneInfo zone)
    {
        var clock = new FakeTimeProvider(Start);
        clock.SetLocalTimeZone(zone);
        return new WorkoutTotalsService(repository.Object, clock, logger);
    }
}
