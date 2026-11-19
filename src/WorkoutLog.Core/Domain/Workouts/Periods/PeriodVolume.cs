namespace WorkoutLog.Core.Domain.Workouts.Periods;

/// <summary>The weight lifted in a period against the period before, by the rule in <see cref="VolumeRule"/>.</summary>
/// <param name="Volume">Kilograms. The whole period, or from its start to today when it holds today.</param>
/// <param name="Earlier">Kilograms. The whole period before, or when this one holds today, up to the same point in it.</param>
public sealed record PeriodVolume(Period Period, decimal Volume, decimal Earlier);
