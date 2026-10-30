using WorkoutLog.Core.Domain.Workouts.Consistency;

namespace WorkoutLog.Core.Tests.Domain.Workouts.Consistency;

public class StreakRuleTests
{
    private readonly StreakRule rule = new();

    [TestCase(new[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }, 0, TestName = "Streak_NoWorkouts_Is0")]
    [TestCase(new[] { 2, 3, 1, 2, 3, 0, 3, 4, 2, 3, 2, 3 }, 6, TestName = "Streak_AGap_EndsIt")]
    [TestCase(new[] { 2, 3, 1, 2, 3, 0, 3, 4, 2, 3, 2, 0 }, 5, TestName = "Streak_ThisWeekEmpty_DoesNotBreakIt")]
    [TestCase(new[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1 }, 1, TestName = "Streak_OnlyThisWeek_Is1")]
    [TestCase(new[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 1 }, 1, TestName = "Streak_LastWeekEmpty_IsThisWeekOnly")]
    [TestCase(new[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0 }, 0, TestName = "Streak_LastWeekAndThisWeekEmpty_Is0")]
    [TestCase(new[] { 2, 3, 1, 2, 3, 2, 3, 4, 2, 3, 2, 3 }, 12, TestName = "Streak_All12Weeks_Is12")]
    [TestCase(new[] { 2, 3, 1, 2, 3, 2, 3, 4, 2, 3, 2, 0 }, 11, TestName = "Streak_All11WeeksBeforeAnEmptyThisWeek_Is11")]
    public void Streak(int[] weekCounts, int expected) => Assert.That(rule.Streak(weekCounts), Is.EqualTo(expected));
}
