namespace WorkoutLog.Core.Domain.Workouts.Consistency;

/// <summary>Works out the weekly streak.</summary>
internal interface IStreakRule
{
    /// <summary>
    /// Consecutive weeks in <paramref name="zone"/> with a workout, ending the week before the one <paramref name="now"/> is in, plus
    /// that week once it has one. <paramref name="starts"/> are workout start times, in any order.
    /// </summary>
    int Streak(IEnumerable<DateTimeOffset> starts, DateTimeOffset now, TimeZoneInfo zone);
}
