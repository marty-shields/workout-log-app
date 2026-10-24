using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using Microsoft.Extensions.Time.Testing;
using Moq;
using WorkoutLog.Core.Data;
using WorkoutLog.Core.Domain.Workouts;
using WorkoutLog.Core.Logging;
using WorkoutLog.Core.Services;

namespace WorkoutLog.Core.Tests.Services;

public class DashboardServiceTests
{
    /// <summary>Wednesday 21 Oct 2026, 12:00 UTC.</summary>
    private static readonly DateTimeOffset Now = new(2026, 10, 21, 12, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset MondayStart = new(2026, 10, 19, 0, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset MonthStart = new(2026, 10, 1, 0, 0, 0, TimeSpan.Zero);

    private Mock<IWorkoutRepository> repository = null!;
    private FakeLogger<DashboardService> logger = null!;
    private DashboardService service = null!;

    [SetUp]
    public void SetUp()
    {
        repository = new Mock<IWorkoutRepository>(MockBehavior.Strict);
        logger = new FakeLogger<DashboardService>();
        UseClock(Now);
    }

    [Test]
    public async Task GetAsync_ReturnsTheLatestStartAndTheCounts()
    {
        SetUpReads(MonthStart, Now, [Now.AddHours(-1)],
            [Now.AddHours(-1), MondayStart, MondayStart.AddTicks(-1), MonthStart]);

        var result = await service.GetAsync();

        Assert.That(result, Is.EqualTo(new DashboardResult(Now.AddHours(-1), 2, 4, Failed: false)));
    }

    [Test]
    public async Task GetAsync_NoWorkouts_ReturnsNoLastWorkoutAndZeroCounts()
    {
        SetUpReads(MonthStart, Now, [], []);

        var result = await service.GetAsync();

        Assert.That(result, Is.EqualTo(new DashboardResult(null, 0, 0, Failed: false)));
    }

    [Test]
    public async Task GetAsync_FutureStart_IsTheLastWorkout()
    {
        SetUpReads(MonthStart, Now, [Now.AddDays(2)], [Now]);

        var result = await service.GetAsync();

        Assert.That(result, Is.EqualTo(new DashboardResult(Now.AddDays(2), 1, 1, Failed: false)));
    }

    [TestCase(2026, 10, 19, 8, TestName = "GetAsync_OnAMonday_WeekStartsThatMorning")]
    [TestCase(2026, 10, 25, 23, TestName = "GetAsync_OnASunday_WeekStartsTheMondayBefore")]
    public async Task GetAsync_WeekStartsOnMonday(int year, int month, int day, int hour)
    {
        var now = new DateTimeOffset(year, month, day, hour, 0, 0, TimeSpan.Zero);
        UseClock(now);
        SetUpReads(MonthStart, now, [], [MondayStart, MondayStart.AddTicks(-1)]);

        var result = await service.GetAsync();

        Assert.That((result.ThisWeek, result.ThisMonth), Is.EqualTo((1, 2)));
    }

    [Test]
    public async Task GetAsync_WeekStartsInThePreviousMonth_ReadsFromTheWeekStart()
    {
        var now = new DateTimeOffset(2026, 10, 1, 9, 0, 0, TimeSpan.Zero);
        var weekStart = new DateTimeOffset(2026, 9, 28, 0, 0, 0, TimeSpan.Zero);
        UseClock(now);
        SetUpReads(weekStart, now, [], [now, MonthStart.AddTicks(-1), weekStart]);

        var result = await service.GetAsync();

        Assert.That((result.ThisWeek, result.ThisMonth), Is.EqualTo((3, 1)));
    }

    [Test]
    public async Task GetAsync_UsesThePhonesTimeZone()
    {
        var zone = TimeZoneInfo.CreateCustomTimeZone("UTC+10", TimeSpan.FromHours(10), "UTC+10", "UTC+10");
        var now = new DateTimeOffset(2026, 10, 18, 20, 0, 0, TimeSpan.Zero);
        var weekStart = new DateTimeOffset(2026, 10, 19, 0, 0, 0, TimeSpan.FromHours(10));
        UseClock(now, zone);
        SetUpReads(new DateTimeOffset(2026, 10, 1, 0, 0, 0, TimeSpan.FromHours(10)), now, [], [now, weekStart, weekStart.AddTicks(-1)]);

        var result = await service.GetAsync();

        Assert.That((result.ThisWeek, result.ThisMonth), Is.EqualTo((2, 3)));
    }

    [Test]
    public async Task GetAsync_MidnightSkipped_WeekStartsAtTheFirstRealInstant()
    {
        // Sunday 4 Oct 2026 at 23:30 (UTC-3) the clocks jump to Monday 00:30 (UTC-2), which is 02:30 UTC.
        var zone = Zone(TimeSpan.FromHours(-3),
            TimeZoneInfo.TransitionTime.CreateFixedDateRule(new DateTime(1, 1, 1, 23, 30, 0), 10, 4),
            TimeZoneInfo.TransitionTime.CreateFixedDateRule(new DateTime(1, 1, 1, 2, 0, 0), 2, 15));
        var now = new DateTimeOffset(2026, 10, 7, 12, 0, 0, TimeSpan.Zero);
        var weekStart = new DateTimeOffset(2026, 10, 5, 2, 30, 0, TimeSpan.Zero);
        UseClock(now, zone);
        SetUpReads(new DateTimeOffset(2026, 10, 1, 0, 0, 0, TimeSpan.FromHours(-3)), now, [], [weekStart, weekStart.AddTicks(-1)]);

        var result = await service.GetAsync();

        Assert.That(result.ThisWeek, Is.EqualTo(1));
    }

    [Test]
    public async Task GetAsync_MidnightSkippedOnThe1st_MonthStartsAtTheFirstRealInstant()
    {
        // Wednesday 30 Sep 2026 at 23:30 (UTC-3) the clocks jump to Thursday 1 Oct 00:30 (UTC-2), which is 02:30 UTC.
        var zone = Zone(TimeSpan.FromHours(-3),
            TimeZoneInfo.TransitionTime.CreateFixedDateRule(new DateTime(1, 1, 1, 23, 30, 0), 9, 30),
            TimeZoneInfo.TransitionTime.CreateFixedDateRule(new DateTime(1, 1, 1, 2, 0, 0), 2, 15));
        var now = new DateTimeOffset(2026, 10, 7, 12, 0, 0, TimeSpan.Zero);
        var monthStart = new DateTimeOffset(2026, 10, 1, 2, 30, 0, TimeSpan.Zero);
        UseClock(now, zone);
        SetUpReads(monthStart, now, [], [monthStart, monthStart.AddTicks(-1)]);

        var result = await service.GetAsync();

        Assert.That(result.ThisMonth, Is.EqualTo(1));
    }

    [Test]
    public async Task GetAsync_MidnightTwice_WeekStartsAtTheFirst()
    {
        // Monday 26 Oct 2026 at 01:00 (UTC+2) the clocks go back to 00:00 (UTC+1), so midnight is 22:00 and 23:00 UTC on the 25th.
        var zone = Zone(TimeSpan.FromHours(1),
            TimeZoneInfo.TransitionTime.CreateFixedDateRule(new DateTime(1, 1, 1, 2, 0, 0), 3, 29),
            TimeZoneInfo.TransitionTime.CreateFixedDateRule(new DateTime(1, 1, 1, 1, 0, 0), 10, 26));
        var now = new DateTimeOffset(2026, 10, 28, 12, 0, 0, TimeSpan.Zero);
        var weekStart = new DateTimeOffset(2026, 10, 25, 22, 0, 0, TimeSpan.Zero);
        UseClock(now, zone);
        SetUpReads(new DateTimeOffset(2026, 10, 1, 0, 0, 0, TimeSpan.FromHours(2)), now, [], [weekStart, weekStart.AddTicks(-1)]);

        var result = await service.GetAsync();

        Assert.That(result.ThisWeek, Is.EqualTo(1));
    }

    [Test]
    public async Task GetAsync_LogsTheCountsAtDebug()
    {
        SetUpReads(MonthStart, Now, [Now], [Now, MonthStart]);

        await service.GetAsync();

        Assert.That(LoggedEvents.Of(logger), Is.EqualTo(new[] { (CoreEvent.DashboardLoaded, LogLevel.Debug, "Dashboard loaded: 1 this week, 2 this month") }));
        Assert.That(logger.LatestRecord.Exception, Is.Null);
    }

    [TestCase(true, TestName = "GetAsync_LastWorkoutReadThrows_ReturnsFailedAndLogsTheError")]
    [TestCase(false, TestName = "GetAsync_CountsReadThrows_ReturnsFailedAndLogsTheError")]
    public async Task GetAsync_ReadThrows_ReturnsFailedAndLogsTheError(bool lastThrows)
    {
        var exception = new InvalidOperationException("disk");
        SetUpReads(MonthStart, Now, [Now], []);
        if (lastThrows)
        {
            repository.Setup(r => r.ListAsync(null, null, 1)).ThrowsAsync(exception);
        }
        else
        {
            repository.Setup(r => r.ListAsync(MonthStart, Now, null)).ThrowsAsync(exception);
        }

        var result = await service.GetAsync();

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.EqualTo(new DashboardResult(null, 0, 0, Failed: true)));
            Assert.That(LoggedEvents.Of(logger), Is.EqualTo(new[] { (CoreEvent.DashboardLoadFailed, LogLevel.Error, "Loading the Dashboard failed") }));
            Assert.That(logger.LatestRecord.Exception, Is.SameAs(exception));
        });
    }

    [Test]
    public async Task GetAsync_PeriodMathsThrows_ReturnsFailedAndLogsTheError()
    {
        var exception = new InvalidOperationException("clock");
        var time = new Mock<TimeProvider>();
        time.Setup(t => t.GetUtcNow()).Throws(exception);
        service = new DashboardService(repository.Object, time.Object, logger);

        var result = await service.GetAsync();

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.EqualTo(new DashboardResult(null, 0, 0, Failed: true)));
            Assert.That(LoggedEvents.Of(logger), Is.EqualTo(new[] { (CoreEvent.DashboardLoadFailed, LogLevel.Error, "Loading the Dashboard failed") }));
            Assert.That(logger.LatestRecord.Exception, Is.SameAs(exception));
        });
    }

    /// <summary>Creates the service with a clock at <paramref name="now"/> in <paramref name="zone"/>, UTC by default.</summary>
    private void UseClock(DateTimeOffset now, TimeZoneInfo? zone = null)
    {
        var time = new FakeTimeProvider(now);
        time.SetLocalTimeZone(zone ?? TimeZoneInfo.Utc);
        service = new DashboardService(repository.Object, time, logger);
    }

    /// <summary>Sets up the last workout read and the counts read, which must be from <paramref name="from"/> to <paramref name="to"/>.</summary>
    private void SetUpReads(DateTimeOffset from, DateTimeOffset to, DateTimeOffset[] last, DateTimeOffset[] recent)
    {
        repository.Setup(r => r.ListAsync(null, null, 1)).ReturnsAsync(Summaries(last));
        repository.Setup(r => r.ListAsync(from, to, null)).ReturnsAsync(Summaries(recent));
    }

    private static List<WorkoutSummary> Summaries(DateTimeOffset[] starts) =>
        starts.Select((start, i) => new WorkoutSummary(i + 1, start)).ToList();

    private static TimeZoneInfo Zone(TimeSpan baseOffset, TimeZoneInfo.TransitionTime daylightStart, TimeZoneInfo.TransitionTime daylightEnd) =>
        TimeZoneInfo.CreateCustomTimeZone("Test", baseOffset, "Test", "Test", "Test",
            [TimeZoneInfo.AdjustmentRule.CreateAdjustmentRule(DateTime.MinValue.Date, DateTime.MaxValue.Date, TimeSpan.FromHours(1), daylightStart, daylightEnd)]);
}
