namespace WorkoutLog.Core.Domain.Workouts.Consistency;

/// <summary>This week so far against last week up to the same day and time.</summary>
/// <param name="ThisWeek">From Monday 00:00 to now.</param>
/// <param name="LastWeek">From last Monday 00:00 to the same day and time last week.</param>
public sealed record WeekTallies(WeekTally ThisWeek, WeekTally LastWeek);
