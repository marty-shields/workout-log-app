namespace WorkoutLog.Core.Domain.Workouts.Periods;

/// <summary>What a period's charts plot. Which lists are filled depends on the kind.</summary>
/// <param name="Buckets">Week and month: each day. Year: each month.</param>
/// <param name="Earlier">Week: each day of the week before. Empty otherwise.</param>
/// <param name="Weeks">Month: each Monday-to-Sunday week cut to the month. Empty otherwise.</param>
/// <param name="WorkoutsPerWeek">Year: workouts per week from 1 January to today, or to 31 December once past. 0 otherwise or before the year starts.</param>
public sealed record PeriodSeries(
    Period Period,
    IReadOnlyList<PeriodBucket> Buckets,
    IReadOnlyList<PeriodBucket> Earlier,
    IReadOnlyList<PeriodBucket> Weeks,
    double WorkoutsPerWeek);
