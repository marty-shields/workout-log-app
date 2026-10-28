using WorkoutLog.Core.Domain.Exercises.Progress;

namespace WorkoutLog.Core.Services;

/// <summary>Reads a lift's sessions and works out its chart values and records (FR-61).</summary>
public interface IExerciseProgressService
{
    /// <summary>
    /// Returns every session of the lift, by start time oldest first; a workout holding it twice is one session, its sets in workout order.
    /// An id not in the exercise list, an exercise that isn't a lift, or a failed read returns <see cref="ExerciseProgressResult.Failed"/> and no sessions.
    /// </summary>
    Task<ExerciseProgressResult> LoadAsync(int exerciseId);

    /// <summary>The sessions of <paramref name="sessions"/> in <paramref name="range"/>, each with its top set and e1RM, and their records.</summary>
    LiftProgress Summarise(IReadOnlyList<LiftSession> sessions, ChartRange range);
}
