using WorkoutLog.Core.Domain.Workouts.Consistency;

namespace WorkoutLog.Core.Tests.Domain.Workouts.Consistency;

public class StreakRuleTests
{
    /// <summary>Wednesday 28 Oct 2026, 18:00 UTC.</summary>
    private static readonly DateTimeOffset Now = new(2026, 10, 28, 18, 0, 0, TimeSpan.Zero);

    private readonly StreakRule rule = new();

    [TestCase(new int[0], 0, TestName = "Streak_NoWorkouts_Is0")]
    [TestCase(new[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19 }, 20, TestName = "Streak_20Weeks_Is20")]
    [TestCase(new[] { 0, 1, 2, 3, 4, 5, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16 }, 6, TestName = "Streak_AnEmptyWeek_EndsIt")]
    [TestCase(new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14 }, 14, TestName = "Streak_ThisWeekEmpty_DoesNotBreakIt")]
    [TestCase(new[] { 0 }, 1, TestName = "Streak_OnlyThisWeek_Is1")]
    [TestCase(new[] { 0, 2, 3 }, 1, TestName = "Streak_LastWeekEmpty_IsThisWeekOnly")]
    [TestCase(new[] { 2, 3 }, 0, TestName = "Streak_LastWeekAndThisWeekEmpty_Is0")]
    [TestCase(new[] { 0, 0, 1, 1, 1 }, 2, TestName = "Streak_SeveralWorkoutsInAWeek_CountItOnce")]
    public void Streak(int[] weeksAgo, int expected)
    {
        var starts = weeksAgo.Select(weeks => Now.AddDays(-7 * weeks));

        Assert.That(rule.Streak(starts, Now, TimeZoneInfo.Utc), Is.EqualTo(expected));
    }

    [Test]
    public void Streak_WeeksAreInThePhonesTimeZone()
    {
        var lastMonday = new DateTimeOffset(2026, 10, 19, 0, 0, 0, TimeSpan.FromHours(10));

        Assert.Multiple(() =>
        {
            Assert.That(rule.Streak([lastMonday], Now, ConsistencyZones.Plus10), Is.EqualTo(1));
            Assert.That(rule.Streak([lastMonday.AddTicks(-1)], Now, ConsistencyZones.Plus10), Is.EqualTo(0));
        });
    }

    [Test]
    public void Streak_ClocksChangedInBetween_EachWeekEndsAtItsOwnSundayMidnight()
    {
        var starts = new[]
        {
            new DateTimeOffset(2026, 10, 25, 23, 59, 59, TimeSpan.FromHours(2)),
            new DateTimeOffset(2026, 10, 18, 23, 59, 59, TimeSpan.FromHours(1)),
        };

        Assert.That(rule.Streak(starts, Now, ConsistencyZones.ForwardOn25Oct), Is.EqualTo(2));
    }
}
