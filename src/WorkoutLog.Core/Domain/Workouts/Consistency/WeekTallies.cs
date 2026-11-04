namespace WorkoutLog.Core.Domain.Workouts.Consistency;

/// <summary>This week so far against last week up to the same point.</summary>
/// <param name="ThisWeek">From Monday 00:00 to now.</param>
/// <param name="LastWeekToDate">From last Monday 00:00 to the same weekday and local time of day as now, last week. At the end of a week, all of last week.</param>
public sealed record WeekTallies(WeekTally ThisWeek, WeekTally LastWeekToDate);
