namespace WorkoutLog.Core.Domain.Workouts.Periods;

/// <summary>Workouts started on the local days <paramref name="Start"/> to <paramref name="End"/>.</summary>
/// <param name="Start">First day, inclusive.</param>
/// <param name="End">Last day, inclusive.</param>
/// <param name="Time">Sum of finish minus start; a workout past midnight counts wholly on its start day.</param>
public sealed record PeriodBucket(DateOnly Start, DateOnly End, int Workouts, TimeSpan Time);
