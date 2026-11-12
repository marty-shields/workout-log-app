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
        var clock = new FakeTimeProvider(Start);
        clock.SetLocalTimeZone(SummerTime());
        service = new WorkoutTotalsService(repository.Object, clock, logger);
    }

    [Test]
    public async Task GetAsync_AddsUpWorkoutsSetsAndTime()
    {
        SetUpRead(Tally(Start, TimeSpan.FromMinutes(45), 10), Tally(Start.AddDays(-2), TimeSpan.FromHours(1), 14), Tally(Start.AddDays(-4), TimeSpan.FromMinutes(30), 6));

        var result = await service.GetAsync(new DateOnly(2026, 10, 4), new DateOnly(2026, 10, 24));

        Assert.That(result.Loaded(), Is.EqualTo(new WorkoutTotals(3, 30, TimeSpan.FromMinutes(135), 1.0)));
    }

    [Test]
    public async Task GetAsync_ReadsFromLocalMidnightOfFromToTheLastTickOfTo()
    {
        repository.Setup(r => r.ListTalliesAsync(
                new DateTimeOffset(2026, 10, 20, 0, 0, 0, TimeSpan.FromHours(1)),
                new DateTimeOffset(2026, 10, 21, 23, 59, 59, TimeSpan.FromHours(1)).AddTicks(TimeSpan.TicksPerSecond - 1)))
            .ReturnsAsync([]);

        await service.GetAsync(new DateOnly(2026, 10, 20), new DateOnly(2026, 10, 21));

        repository.VerifyAll();
    }

    [Test]
    public async Task GetAsync_AcrossTheClockChange_UsesEachDaysOwnOffset()
    {
        repository.Setup(r => r.ListTalliesAsync(
                new DateTimeOffset(2026, 10, 24, 0, 0, 0, TimeSpan.FromHours(1)),
                new DateTimeOffset(2026, 10, 26, 0, 0, 0, TimeSpan.Zero).AddTicks(-1)))
            .ReturnsAsync([]);

        await service.GetAsync(new DateOnly(2026, 10, 24), new DateOnly(2026, 10, 25));

        repository.VerifyAll();
    }

    [Test]
    public async Task GetAsync_LastDayInTheClockChange_IsTheWholeLocalDay()
    {
        var lateOnLastDay = new DateTimeOffset(2026, 10, 25, 23, 50, 0, TimeSpan.Zero);
        repository.Setup(r => r.ListTalliesAsync(It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>()))
            .ReturnsAsync((DateTimeOffset _, DateTimeOffset to) => to >= lateOnLastDay ? [Tally(lateOnLastDay, TimeSpan.FromMinutes(20), 4)] : []);

        var result = await service.GetAsync(new DateOnly(2026, 10, 19), new DateOnly(2026, 10, 25));

        Assert.That(result.Loaded().Workouts, Is.EqualTo(1));
    }

    [Test]
    public async Task GetAsync_NoWorkouts_IsAllZeroWithoutDividing()
    {
        SetUpRead();

        var result = await service.GetAsync(new DateOnly(2026, 10, 1), new DateOnly(2026, 10, 1));

        Assert.That(result.Loaded(), Is.EqualTo(new WorkoutTotals(0, 0, TimeSpan.Zero, 0)));
    }

    [TestCase(1, 2, 2.0)]
    [TestCase(3, 2, 2.0)]
    [TestCase(7, 2, 2.0)]
    [TestCase(14, 3, 1.5)]
    [TestCase(28, 5, 1.25)]
    public async Task GetAsync_AveragesPerSevenDaysOfTheSpanWithAWeekAtLeast(int days, int workouts, double perWeek)
    {
        SetUpRead(Enumerable.Range(0, workouts).Select(i => Tally(Start.AddHours(i), TimeSpan.FromMinutes(30), 1)).ToArray());
        var from = new DateOnly(2026, 10, 1);

        var result = await service.GetAsync(from, from.AddDays(days - 1));

        Assert.That(result.Loaded().AveragePerWeek, Is.EqualTo(perWeek).Within(1e-9));
    }

    [Test]
    public async Task GetAsync_FinishBeforeStartOrMissing_AddsNoTime()
    {
        SetUpRead(
            Tally(Start, TimeSpan.FromMinutes(-10), 1),
            new WorkoutTally(Start, default, 2),
            new WorkoutTally(Start, Start, 3),
            Tally(Start, TimeSpan.FromMinutes(20), 4));

        var result = await service.GetAsync(new DateOnly(2026, 10, 1), new DateOnly(2026, 10, 31));

        Assert.That(result.Loaded(), Has.Property(nameof(WorkoutTotals.TimeTrained)).EqualTo(TimeSpan.FromMinutes(20))
            .And.Property(nameof(WorkoutTotals.Workouts)).EqualTo(4)
            .And.Property(nameof(WorkoutTotals.Sets)).EqualTo(10));
    }

    [Test]
    public async Task GetAsync_LogsTheCountsAtDebug()
    {
        SetUpRead(Tally(Start, TimeSpan.FromMinutes(45), 10), Tally(Start, TimeSpan.FromMinutes(45), 5));

        await service.GetAsync(new DateOnly(2026, 10, 1), new DateOnly(2026, 10, 31));

        Assert.That(LoggedEvents.Of(logger), Is.EqualTo(new[] { (CoreEvent.WorkoutTotalsLoaded, LogLevel.Debug, "Added up 2 workouts with 15 sets") }));
    }

    [Test]
    public async Task GetAsync_ReadThrows_ReturnsFailedAndLogsAnErrorWithTheException()
    {
        var failure = new InvalidOperationException("disk");
        repository.Setup(r => r.ListTalliesAsync(It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>())).ThrowsAsync(failure);

        var result = await service.GetAsync(new DateOnly(2026, 10, 1), new DateOnly(2026, 10, 31));

        Assert.Multiple(() =>
        {
            Assert.That(result.Value, Is.InstanceOf<ReadFailed>());
            Assert.That(LoggedEvents.Of(logger), Is.EqualTo(new[] { (CoreEvent.WorkoutTotalsLoadFailed, LogLevel.Error, "Adding up the workouts failed") }));
            Assert.That(logger.Collector.LatestRecord.Exception, Is.SameAs(failure));
        });
    }

    private static WorkoutTally Tally(DateTimeOffset start, TimeSpan length, int sets) => new(start, start + length, sets);

    private void SetUpRead(params WorkoutTally[] tallies) =>
        repository.Setup(r => r.ListTalliesAsync(It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>())).ReturnsAsync(tallies);

    private static TimeZoneInfo SummerTime() =>
        TimeZoneInfo.CreateCustomTimeZone("Summer", TimeSpan.Zero, "Summer", "Winter", "Summer",
        [
            TimeZoneInfo.AdjustmentRule.CreateAdjustmentRule(DateTime.MinValue.Date, DateTime.MaxValue.Date, TimeSpan.FromHours(1),
                TimeZoneInfo.TransitionTime.CreateFixedDateRule(new DateTime(1, 1, 1, 1, 0, 0), 3, 29),
                TimeZoneInfo.TransitionTime.CreateFixedDateRule(new DateTime(1, 1, 1, 2, 0, 0), 10, 25)),
        ]);
}
