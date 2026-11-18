using WorkoutLog.Core.Domain.Workouts.Consistency;

namespace WorkoutLog.Core.Domain.Workouts.Periods;

/// <summary>A period's totals against the period before.</summary>
/// <param name="Totals">The whole period, or from its start to now when it holds today.</param>
/// <param name="Earlier">The whole period before, or when this one holds today, up to the same point in it.</param>
public sealed record PeriodComparison(Period Period, WeekTally Totals, WeekTally Earlier);
