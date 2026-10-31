namespace WorkoutLog.Core.Services;

/// <summary>Reads the strength sets per muscle group in the last 7 days.</summary>
public interface IMuscleSetsService
{
    /// <summary>
    /// Returns every muscle group, in <see cref="Domain.Exercises.MuscleGroup"/> order, with the strength sets of the workouts started from
    /// 00:00 six days ago to now in the phone's time zone. Only the exercise's primary muscle counts, so only strength exercises with one
    /// count; ids not in the exercise list are left out. A failed read returns <see cref="MuscleSetsResult.Failed"/> and no groups.
    /// </summary>
    Task<MuscleSetsResult> GetAsync();
}
