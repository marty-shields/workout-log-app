using WorkoutLog.Core.Domain.Workouts.Consistency;

namespace WorkoutLog.Core.Tests.Domain.Workouts.Consistency;

public class LocalWeekTests
{
    [TestCase(2026, 10, 26, TestName = "Monday_OnAMonday_IsThatDay")]
    [TestCase(2026, 10, 28, TestName = "Monday_OnAWednesday_IsTheMondayBefore")]
    [TestCase(2026, 11, 1, TestName = "Monday_OnASunday_IsTheMondayBefore")]
    public void Monday(int year, int month, int day) =>
        Assert.That(LocalWeek.Monday(new DateOnly(year, month, day)), Is.EqualTo(new DateOnly(2026, 10, 26)));

    [Test]
    public void Start_MidnightSkipped_IsTheEndOfTheGap() =>
        Assert.That(LocalWeek.Start(new DateOnly(2026, 10, 5), ConsistencyZones.MidnightSkippedOn5Oct),
            Is.EqualTo(new DateTimeOffset(2026, 10, 5, 2, 30, 0, TimeSpan.Zero)));

    [Test]
    public void Start_MidnightTwice_IsTheFirst() =>
        Assert.That(LocalWeek.Start(new DateOnly(2026, 10, 26), ConsistencyZones.MidnightTwiceOn26Oct),
            Is.EqualTo(new DateTimeOffset(2026, 10, 25, 22, 0, 0, TimeSpan.Zero)));

    [Test]
    public void Start_UsesTheZonesOffset() =>
        Assert.That(LocalWeek.Start(new DateOnly(2026, 10, 26), ConsistencyZones.Plus10),
            Is.EqualTo(new DateTimeOffset(2026, 10, 25, 14, 0, 0, TimeSpan.Zero)));
}
