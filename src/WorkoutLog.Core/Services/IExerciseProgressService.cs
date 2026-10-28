using WorkoutLog.Core.Domain.Exercises;
using WorkoutLog.Core.Domain.Exercises.Progress;

namespace WorkoutLog.Core.Services;

/// <summary>Reads an exercise's sessions and works out its chart values and records (FR-61, FR-62).</summary>
public interface IExerciseProgressService
{
    /// <summary>
    /// Returns every session of the exercise, by start time oldest first; a workout holding it twice is one session, its sets in workout order.
    /// An id not in the exercise list, an exercise with no Exercise page (<see cref="ExerciseDefinition.HasProgressPage"/>) or a failed read
    /// returns <see cref="ExerciseProgressResult.Failed"/> and no sessions.
    /// </summary>
    Task<ExerciseProgressResult> LoadAsync(int exerciseId);

    /// <summary>The lift sessions of <paramref name="sessions"/> in <paramref name="range"/>, each with its top set and e1RM, and their records.</summary>
    LiftProgress SummariseLift(IReadOnlyList<StrengthSession> sessions, ChartRange range);

    /// <summary>
    /// The bodyweight sessions of <paramref name="sessions"/> in <paramref name="range"/>, each with its best set, total, top set and e1RM,
    /// and their records.
    /// </summary>
    BodyweightProgress SummariseBodyweight(IReadOnlyList<StrengthSession> sessions, ChartRange range);
}
