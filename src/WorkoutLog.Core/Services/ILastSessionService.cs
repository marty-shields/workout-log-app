namespace WorkoutLog.Core.Services;

/// <summary>Reads what an exercise's last saved workout had, to show beside the sets being logged.</summary>
public interface ILastSessionService
{
    /// <summary>
    /// Returns the exercise's sets or entry in the workout with the latest start that has it; with <paramref name="before"/>, the latest
    /// one that started before that time. No such workout returns a null <see cref="LastSessionResult.Exercise"/>.
    /// A failed read returns <see cref="LastSessionResult.Failed"/> and no exercise.
    /// </summary>
    /// <param name="before">Workouts must start before this time. Null for any.</param>
    Task<LastSessionResult> GetAsync(int exerciseId, DateTimeOffset? before = null);
}
