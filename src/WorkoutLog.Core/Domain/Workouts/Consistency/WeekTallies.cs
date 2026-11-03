namespace WorkoutLog.Core.Domain.Workouts.Consistency;

/// <summary>This week so far against the whole of last week.</summary>
/// <param name="ThisWeek">From Monday 00:00 to now.</param>
/// <param name="LastWeek">From last Monday 00:00 to last Sunday 23:59:59.</param>
public sealed record WeekTallies(WeekTally ThisWeek, WeekTally LastWeek);
