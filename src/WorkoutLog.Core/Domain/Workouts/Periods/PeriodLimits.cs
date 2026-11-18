namespace WorkoutLog.Core.Domain.Workouts.Periods;

/// <summary>The periods the stepper can reach.</summary>
/// <param name="Earliest">The period of the first workout; with no workouts, the same as <paramref name="Latest"/>.</param>
/// <param name="Latest">The period that holds today.</param>
public sealed record PeriodLimits(Period Earliest, Period Latest);
