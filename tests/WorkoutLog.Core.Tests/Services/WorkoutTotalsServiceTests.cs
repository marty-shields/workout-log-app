using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using Microsoft.Extensions.Time.Testing;
using Moq;
using WorkoutLog.Core.Data;
using WorkoutLog.Core.Domain.Workouts;
using WorkoutLog.Core.Logging;
using WorkoutLog.Core.Services;

namespace WorkoutLog.Core.Tests.Services;

public class WorkoutTotalsServiceTests
{
    private static readonly DateTimeOffset Start = new(2026, 10, 21, 17, 5, 0, TimeSpan.Zero);

    private Mock<IWorkoutRepository> repository = null!;
    private FakeLogger<WorkoutTotalsService> logger = null!;
    private WorkoutTotalsService service = null!;

    [SetUp]
    public void SetUp()
    {
        repository = new Mock<IWorkoutRepository>(MockBehavior.Strict);
        logger = new FakeLogger<WorkoutTotalsService>();
        UseZone(TimeZoneInfo.Utc);
    }

    [Test]
    public async Task GetAsync_AddsUpWorkoutsSetsAndTime()
    {
        SetUpRead(
            new DateTimeOffset(2026, 10, 1, 0, 0, 0, TimeSpan.Zero), new DateTimeOffset(2026, 10, 29, 0, 0, 0, TimeSpan.Zero).AddTicks(-1),
            new WorkoutTally(Start, Start.AddMinutes(60), 10),
            new WorkoutTally(Start.AddDays(1), Start.AddDays(1).AddMinutes(45), 7),
            new WorkoutTally(Start.AddDays(2), Start.AddDays(2).AddMinutes(30), 3));

        var result = await service.GetAsync(new DateOnly(2026, 10, 1), new DateOnly(2026, 10, 28));

        Assert.That(result.Loaded(), Is.EqualTo(new WorkoutTotals(3, 20, TimeSpan.FromMinutes(135), 0.75)));
    }

    [Test]
    public async Task GetAsync_EmptySpan_IsAllZeroWithoutDividingByZero()
    {
        SetUpRead(
            new DateTimeOffset(2026, 10, 1, 0, 0, 0, TimeSpan.Zero), new DateTimeOffset(2026, 10, 15, 0, 0, 0, TimeSpan.Zero).AddTicks(-1));

        var result = await service.GetAsync(new DateOnly(2026, 10, 1), new DateOnly(2026, 10, 14));

        Assert.That(result.Loaded(), Is.EqualTo(new WorkoutTotals(0, 0, TimeSpan.Zero, 0)));
    }

    [TestCase(1, 3.0)]
    [TestCase(3, 3.0)]
    [TestCase(7, 3.0)]
    [TestCase(14, 1.5)]
    public async Task GetAsync_SpanUnderAWeek_CountsAsAWeek(int days, double perWeek)
    {
        var from = new DateOnly(2026, 10, 1);
        var to = from.AddDays(days - 1);
        repository.Setup(r => r.ListTalliesAsync(It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>()))
            .ReturnsAsync([new WorkoutTally(Start, Start, 0), new WorkoutTally(Start, Start, 0), new WorkoutTally(Start, Start, 0)]);

        var result = await service.GetAsync(from, to);

        Assert.That(result.Loaded().AveragePerWeek, Is.EqualTo(perWeek));
    }

    [Test]
    public async Task GetAsync_NoFinishOrFinishBeforeStart_AddsZeroTime()
    {
        SetUpRead(
            new DateTimeOffset(2026, 10, 21, 0, 0, 0, TimeSpan.Zero), new DateTimeOffset(2026, 10, 22, 0, 0, 0, TimeSpan.Zero).AddTicks(-1),
            new WorkoutTally(Start, default, 4),
            new WorkoutTally(Start, Start.AddMinutes(-10), 2),
            new WorkoutTally(Start, Start.AddMinutes(20), 1));

        var result = await service.GetAsync(new DateOnly(2026, 10, 21), new DateOnly(2026, 10, 21));

        Assert.That(result.Loaded(), Is.EqualTo(new WorkoutTotals(3, 7, TimeSpan.FromMinutes(20), 3)));
    }

    [Test]
    public async Task GetAsync_ReadsFromTheFirstInstantOfFromToTheLastTickOfTo()
    {
        UseZone(SummerTime());
        SetUpRead(
            new DateTimeOffset(2026, 10, 20, 0, 0, 0, TimeSpan.FromHours(1)),
            new DateTimeOffset(2026, 10, 21, 23, 59, 59, TimeSpan.FromHours(1)).AddTicks(TimeSpan.TicksPerSecond - 1));

        await service.GetAsync(new DateOnly(2026, 10, 20), new DateOnly(2026, 10, 21));

        repository.VerifyAll();
    }

    [Test]
    public async Task GetAsync_SpanEndingOnTheClockChangeDay_EndsAtThatDaysLastTick()
    {
        UseZone(SummerTime());
        SetUpRead(
            new DateTimeOffset(2026, 10, 24, 0, 0, 0, TimeSpan.FromHours(1)),
            new DateTimeOffset(2026, 10, 26, 0, 0, 0, TimeSpan.Zero).AddTicks(-1));

        await service.GetAsync(new DateOnly(2026, 10, 24), new DateOnly(2026, 10, 25));

        repository.VerifyAll();
    }

    [Test]
    public async Task GetAsync_ClockChangeDay_CountsOneCalendarDayNotElapsedHours()
    {
        UseZone(SummerTime());
        repository.Setup(r => r.ListTalliesAsync(It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>()))
            .ReturnsAsync([new WorkoutTally(Start, Start, 0)]);

        var result = await service.GetAsync(new DateOnly(2026, 10, 19), new DateOnly(2026, 10, 25));

        Assert.That(result.Loaded().AveragePerWeek, Is.EqualTo(1));
    }

    [Test]
    public async Task GetAsync_LogsTheCountsAtDebug()
    {
        SetUpRead(
            new DateTimeOffset(2026, 10, 1, 0, 0, 0, TimeSpan.Zero), new DateTimeOffset(2026, 10, 8, 0, 0, 0, TimeSpan.Zero).AddTicks(-1),
            new WorkoutTally(Start, Start.AddMinutes(5), 4),
            new WorkoutTally(Start, Start.AddMinutes(5), 2));

        await service.GetAsync(new DateOnly(2026, 10, 1), new DateOnly(2026, 10, 7));

        Assert.That(LoggedEvents.Of(logger), Is.EqualTo(new[] { (CoreEvent.WorkoutTotalsLoaded, LogLevel.Debug, "Added up 2 workouts with 6 sets") }));
    }

    [Test]
    public async Task GetAsync_ReadThrows_ReturnsFailedAndLogsTheErrorWithTheException()
    {
        var exception = new InvalidOperationException("disk");
        repository.Setup(r => r.ListTalliesAsync(It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>())).ThrowsAsync(exception);

        var result = await service.GetAsync(new DateOnly(2026, 10, 1), new DateOnly(2026, 10, 7));

        Assert.Multiple(() =>
        {
            Assert.That(result.Value, Is.InstanceOf<ReadFailed>());
            Assert.That(LoggedEvents.Of(logger), Is.EqualTo(new[] { (CoreEvent.WorkoutTotalsLoadFailed, LogLevel.Error, "Adding up the workouts failed") }));
            Assert.That(logger.LatestRecord.Exception, Is.SameAs(exception));
        });
    }

    private void UseZone(TimeZoneInfo zone)
    {
        var clock = new FakeTimeProvider(Start);
        clock.SetLocalTimeZone(zone);
        service = new WorkoutTotalsService(repository.Object, clock, logger);
    }

    private void SetUpRead(DateTimeOffset from, DateTimeOffset to, params WorkoutTally[] tallies) =>
        repository.Setup(r => r.ListTalliesAsync(from, to)).ReturnsAsync(tallies);

    /// <summary>UTC in winter and UTC+1 from 01:00 on 29 Mar to 02:00 on 25 Oct, like London in 2026.</summary>
    private static TimeZoneInfo SummerTime() =>
        TimeZoneInfo.CreateCustomTimeZone("Summer", TimeSpan.Zero, "Summer", "Winter", "Summer",
        [
            TimeZoneInfo.AdjustmentRule.CreateAdjustmentRule(DateTime.MinValue.Date, DateTime.MaxValue.Date, TimeSpan.FromHours(1),
                TimeZoneInfo.TransitionTime.CreateFixedDateRule(new DateTime(1, 1, 1, 1, 0, 0), 3, 29),
                TimeZoneInfo.TransitionTime.CreateFixedDateRule(new DateTime(1, 1, 1, 2, 0, 0), 10, 25)),
        ]);
}
