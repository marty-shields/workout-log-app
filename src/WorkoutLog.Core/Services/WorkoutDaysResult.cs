namespace WorkoutLog.Core.Services;

/// <summary>Outcome of counting the saved workouts by the local day they started on.</summary>
/// <param name="PerDay">Workouts started on each day in the phone's time zone; days with none are left out. Empty when loading failed.</param>
/// <param name="Failed">True when the workouts couldn't be read.</param>
public sealed record WorkoutDaysResult(IReadOnlyDictionary<DateOnly, int> PerDay, bool Failed);
