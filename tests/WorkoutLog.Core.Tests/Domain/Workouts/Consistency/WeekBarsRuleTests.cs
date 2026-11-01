using WorkoutLog.Core.Domain.Workouts;
using WorkoutLog.Core.Domain.Workouts.Consistency;

namespace WorkoutLog.Core.Tests.Domain.Workouts.Consistency;

public class WeekBarsRuleTests
{
    /// <summary>Wednesday 28 Oct 2026, 18:00 UTC.</summary>
    private static readonly DateTimeOffset Now = new(2026, 10, 28, 18, 0, 0, TimeSpan.Zero);

    private static readonly DateTimeOffset ThisMonday = new(2026, 10, 26, 0, 0, 0, TimeSpan.Zero);

    private readonly WeekBarsRule rule = new();

    [Test]
    public void Count_NoWorkouts_Is12Zeros() =>
        Assert.That(rule.Count([], Now, TimeZoneInfo.Utc), Is.EqualTo(new int[12]));

    [Test]
    public void Count_IsPerWeekOldestFirstWithGaps()
    {
        var counts = rule.Count(
        [
            Workout(ThisMonday.AddDays(-77)),
            Workout(ThisMonday.AddDays(-71)),
            Workout(ThisMonday.AddDays(-14).AddHours(9)),
            Workout(ThisMonday.AddTicks(-1)),
            Workout(ThisMonday),
            Workout(Now),
        ], Now, TimeZoneInfo.Utc);

        Assert.That(counts, Is.EqualTo(new[] { 2, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 2 }));
    }

    [Test]
    public void Count_WorkoutsOutsideThe12Weeks_AreNotCounted()
    {
        var counts = rule.Count([Workout(ThisMonday.AddDays(-77).AddTicks(-1)), Workout(ThisMonday.AddDays(7))], Now, TimeZoneInfo.Utc);

        Assert.That(counts, Is.EqualTo(new int[12]));
    }

    [Test]
    public void Count_WeeksAreInThePhonesTimeZone()
    {
        var monday = new DateTimeOffset(2026, 10, 26, 0, 0, 0, TimeSpan.FromHours(10));

        var counts = rule.Count([Workout(monday.AddTicks(-1)), Workout(monday)], Now, ConsistencyZones.Plus10);

        Assert.That(counts.TakeLast(2), Is.EqualTo(new[] { 1, 1 }));
    }

    [Test]
    public void Count_ClocksChangedInBetween_WeeksStillStartOnMonday()
    {
        var monday = new DateTimeOffset(2026, 10, 26, 0, 0, 0, TimeSpan.FromHours(2));

        var counts = rule.Count(
            [Workout(monday.AddTicks(-1)), Workout(monday), Workout(monday.AddDays(-77).AddHours(1))], Now, ConsistencyZones.ForwardOn25Oct);

        Assert.That(counts, Is.EqualTo(new[] { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1 }));
    }

    private static WorkoutTally Workout(DateTimeOffset startedAt) => new(startedAt, startedAt.AddMinutes(45), 1);
}
