using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using Microsoft.Extensions.Time.Testing;
using Moq;
using WorkoutLog.Core.Data;
using WorkoutLog.Core.Domain.Workouts.Consistency;
using WorkoutLog.Core.Domain.Workouts.Periods;
using WorkoutLog.Core.Logging;
using WorkoutLog.Core.Services;
using static WorkoutLog.Core.Tests.Domain.Workouts.Periods.PeriodTestData;

namespace WorkoutLog.Core.Tests.Services;

public class PeriodTotalsServiceTests
{
    /// <summary>Wednesday 28 Oct 2026, 18:00 UTC.</summary>
    private static readonly DateTimeOffset Now = At("2026-10-28T18:00");

    private Mock<IWorkoutRepository> repository = null!;
    private FakeLogger<PeriodTotalsService> logger = null!;
    private PeriodTotalsService service = null!;

    [SetUp]
    public void SetUp()
    {
        repository = new Mock<IWorkoutRepository>(MockBehavior.Strict);
        logger = new FakeLogger<PeriodTotalsService>();
        service = Create(new FakeTimeProvider(Now));
    }

    private PeriodTotalsService Create(TimeProvider time) =>
        new(repository.Object, new PeriodComparisonRule(new WeekComparisonRule()), new PeriodSeriesRule(), new LongestRunRule(), new WeekBarsRule(), new StreakRule(), time, logger);

    [Test]
    public async Task GetComparisonAsync_ReadsOnlyTheNeededRangeAndSummarisesIt()
    {
        repository.Setup(r => r.ListTalliesAsync(At("2026-08-31T00:00"), Now))
            .ReturnsAsync([Workout("2026-09-10T10:00", 30, 2), Workout("2026-10-02T10:00", 60, 5)]);

        var result = await service.GetComparisonAsync(Month(2026, 10));

        Assert.That(result.Loaded(), Is.EqualTo(new PeriodComparison(Month(2026, 10), new WeekTally(1, TimeSpan.FromHours(1), 5), new WeekTally(1, TimeSpan.FromMinutes(30), 2))));
    }

    [Test]
    public async Task GetComparisonAsync_PastPeriod_StopsTheReadShortOfNow()
    {
        repository.Setup(r => r.ListTalliesAsync(At("2026-01-31T00:00"), At("2026-04-02T00:00"))).ReturnsAsync([]);

        var result = await service.GetComparisonAsync(Month(2026, 3));

        Assert.That(result.Loaded(), Is.EqualTo(new PeriodComparison(Month(2026, 3), WeekTally.None, WeekTally.None)));
    }

    [Test]
    public async Task GetComparisonAsync_LogsAtInformation()
    {
        repository.Setup(r => r.ListTalliesAsync(It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>())).ReturnsAsync([]);

        await service.GetComparisonAsync(Week(2026, 10, 28));

        Assert.That(LoggedEvents.Of(logger), Is.EqualTo(new[] { (CoreEvent.PeriodComparisonLoaded, LogLevel.Information, "Period comparison loaded: Week from 2026-10-26") }));
    }

    [Test]
    public async Task GetComparisonAsync_ReadThrows_ReturnsFailedAndLogsAWarning()
    {
        var exception = new InvalidOperationException("disk");
        repository.Setup(r => r.ListTalliesAsync(It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>())).ThrowsAsync(exception);

        var result = await service.GetComparisonAsync(Year(2026));

        Assert.Multiple(() =>
        {
            Assert.That(result.Value, Is.InstanceOf<ReadFailed>());
            Assert.That(LoggedEvents.Of(logger).Select(e => (e.Item1, e.Item2)), Is.EqualTo(new[] { (CoreEvent.PeriodComparisonLoadFailed, LogLevel.Warning) }));
            Assert.That(logger.LatestRecord.Exception, Is.SameAs(exception));
        });
    }

    [Test]
    public async Task GetSeriesAsync_Week_ReadsFromTheWeekBefore()
    {
        repository.Setup(r => r.ListTalliesAsync(At("2026-10-18T00:00"), Now)).ReturnsAsync([Workout("2026-10-19T08:00"), Workout("2026-10-27T08:00")]);

        var result = await service.GetSeriesAsync(Week(2026, 10, 28));

        Assert.Multiple(() =>
        {
            Assert.That(result.Loaded().Earlier[0].Workouts, Is.EqualTo(1));
            Assert.That(result.Loaded().Buckets[1].Workouts, Is.EqualTo(1));
            Assert.That(LoggedEvents.Of(logger).Select(e => (e.Item1, e.Item2)), Is.EqualTo(new[] { (CoreEvent.PeriodSeriesLoaded, LogLevel.Information) }));
        });
    }

    [Test]
    public async Task GetSeriesAsync_Year_ReadsFromTheYearStart()
    {
        repository.Setup(r => r.ListTalliesAsync(At("2025-12-31T00:00"), Now)).ReturnsAsync([Workout("2026-01-02T08:00")]);

        var result = await service.GetSeriesAsync(Year(2026));

        Assert.That(result.Loaded().Buckets[0].Workouts, Is.EqualTo(1));
    }

    [Test]
    public async Task GetSeriesAsync_ReadThrows_ReturnsFailedAndLogsAWarning()
    {
        var exception = new InvalidOperationException("disk");
        repository.Setup(r => r.ListTalliesAsync(It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>())).ThrowsAsync(exception);

        var result = await service.GetSeriesAsync(Month(2026, 10));

        Assert.Multiple(() =>
        {
            Assert.That(result.Value, Is.InstanceOf<ReadFailed>());
            Assert.That(LoggedEvents.Of(logger).Select(e => (e.Item1, e.Item2)), Is.EqualTo(new[] { (CoreEvent.PeriodSeriesLoadFailed, LogLevel.Warning) }));
            Assert.That(logger.LatestRecord.Exception, Is.SameAs(exception));
        });
    }

    [Test]
    public async Task GetStreaksAsync_GivesTheCurrentStreakTheWeekCountsAndTheLongestRun()
    {
        repository.Setup(r => r.ListTalliesAsync(DateTimeOffset.MinValue, Now))
            .ReturnsAsync([Workout("2026-01-05T09:00"), Workout("2026-01-12T09:00"), Workout("2026-01-19T09:00"), Workout("2026-10-19T09:00"), Workout("2026-10-26T09:00")]);

        var result = await service.GetStreaksAsync(2026);

        Assert.Multiple(() =>
        {
            Assert.That(result.Loaded().Current, Is.EqualTo(2));
            Assert.That(result.Loaded().WeekCounts, Is.EqualTo(new[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1 }));
            Assert.That(result.Loaded().Longest, Is.EqualTo(new LongestRun(3, false)));
            Assert.That(LoggedEvents.Of(logger).Select(e => e.Item3), Is.EqualTo(new[] { "Streaks loaded for 2026: 2-week streak, longest run 3 weeks" }));
        });
    }

    [Test]
    public async Task GetStreaksAsync_ReadThrows_ReturnsFailedAndLogsAWarning()
    {
        var exception = new InvalidOperationException("disk");
        repository.Setup(r => r.ListTalliesAsync(DateTimeOffset.MinValue, Now)).ThrowsAsync(exception);

        var result = await service.GetStreaksAsync(2026);

        Assert.Multiple(() =>
        {
            Assert.That(result.Value, Is.InstanceOf<ReadFailed>());
            Assert.That(LoggedEvents.Of(logger), Is.EqualTo(new[] { (CoreEvent.PeriodStreaksLoadFailed, LogLevel.Warning, "Loading the streaks for 2026 failed") }));
            Assert.That(logger.LatestRecord.Exception, Is.SameAs(exception));
        });
    }

    [TestCase(PeriodKind.Week, "2025-12-29", "2026-10-26")]
    [TestCase(PeriodKind.Month, "2025-12-01", "2026-10-01")]
    [TestCase(PeriodKind.Year, "2025-01-01", "2026-01-01")]
    public async Task GetLimitsAsync_IsTheFirstWorkoutsPeriodAndTodays(PeriodKind kind, string earliest, string latest)
    {
        repository.Setup(r => r.ListTalliesAsync(DateTimeOffset.MinValue, Now)).ReturnsAsync([Workout("2025-12-31T23:30"), Workout("2026-03-05T10:00")]);

        var result = await service.GetLimitsAsync(kind);

        Assert.That((result.Loaded().Earliest.Start, result.Loaded().Latest.Start), Is.EqualTo((DateOnly.Parse(earliest), DateOnly.Parse(latest))));
    }

    [Test]
    public async Task GetLimitsAsync_NoWorkouts_BothAreTodaysPeriod()
    {
        repository.Setup(r => r.ListTalliesAsync(DateTimeOffset.MinValue, Now)).ReturnsAsync([]);

        var result = await service.GetLimitsAsync(PeriodKind.Month);

        Assert.That((result.Loaded().Earliest, result.Loaded().Latest), Is.EqualTo((Month(2026, 10), Month(2026, 10))));
    }

    [Test]
    public async Task GetLimitsAsync_UsesTheLocalDayOfTheFirstWorkoutAndOfToday()
    {
        var time = new FakeTimeProvider(At("2026-10-31T15:30"));
        time.SetLocalTimeZone(TimeZoneInfo.CreateCustomTimeZone("UTC+10", TimeSpan.FromHours(10), "UTC+10", "UTC+10"));
        service = Create(time);
        repository.Setup(r => r.ListTalliesAsync(DateTimeOffset.MinValue, At("2026-10-31T15:30"))).ReturnsAsync([Workout("2026-09-30T14:30")]);

        var result = await service.GetLimitsAsync(PeriodKind.Month);

        Assert.That((result.Loaded().Earliest, result.Loaded().Latest), Is.EqualTo((Month(2026, 10), Month(2026, 11))));
    }

    [Test]
    public async Task GetLimitsAsync_ReadThrows_ReturnsFailedAndLogsAWarning()
    {
        var exception = new InvalidOperationException("disk");
        repository.Setup(r => r.ListTalliesAsync(DateTimeOffset.MinValue, Now)).ThrowsAsync(exception);

        var result = await service.GetLimitsAsync(PeriodKind.Week);

        Assert.Multiple(() =>
        {
            Assert.That(result.Value, Is.InstanceOf<ReadFailed>());
            Assert.That(LoggedEvents.Of(logger), Is.EqualTo(new[] { (CoreEvent.PeriodLimitsLoadFailed, LogLevel.Warning, "Loading the period limits failed: Week") }));
            Assert.That(logger.LatestRecord.Exception, Is.SameAs(exception));
        });
    }

    [Test]
    public async Task GetLimitsAsync_LogsAtInformation()
    {
        repository.Setup(r => r.ListTalliesAsync(DateTimeOffset.MinValue, Now)).ReturnsAsync([]);

        await service.GetLimitsAsync(PeriodKind.Year);

        Assert.That(LoggedEvents.Of(logger), Is.EqualTo(new[] { (CoreEvent.PeriodLimitsLoaded, LogLevel.Information, "Period limits loaded: Year") }));
    }

    [Test]
    public async Task GetLimitsAsync_ClockThrows_ReturnsFailedAndLogsAWarning()
    {
        var exception = new InvalidOperationException("clock");
        var time = new Mock<TimeProvider>();
        time.Setup(t => t.GetUtcNow()).Throws(exception);
        service = Create(time.Object);

        var result = await service.GetLimitsAsync(PeriodKind.Week);

        Assert.Multiple(() =>
        {
            Assert.That(result.Value, Is.InstanceOf<ReadFailed>());
            Assert.That(logger.LatestRecord.Exception, Is.SameAs(exception));
        });
    }
}
