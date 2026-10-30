using WorkoutLog.Core.Domain.Exercises.Progress;

namespace WorkoutLog.Core.Services;

/// <summary>Reads an exercise's sessions and works out its chart values and records.</summary>
public interface IExerciseProgressService
{
    /// <summary>
    /// Returns every session of the exercise, by start time oldest first, its sets in order.
    /// An id not in the exercise list, a cardio exercise or a failed read
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

    /// <summary>The assisted machine sessions of <paramref name="sessions"/> in <paramref name="range"/>, each with its least assist, and their records.</summary>
    AssistedProgress SummariseAssisted(IReadOnlyList<StrengthSession> sessions, ChartRange range);

    /// <summary>The hold sessions of <paramref name="sessions"/> in <paramref name="range"/>, each with its longest and heaviest set, and their records.</summary>
    HoldProgress SummariseHold(IReadOnlyList<StrengthSession> sessions, ChartRange range);

    /// <summary>The carry sessions of <paramref name="sessions"/> in <paramref name="range"/>, each with its heaviest and longest set, and their records.</summary>
    CarryProgress SummariseCarry(IReadOnlyList<StrengthSession> sessions, ChartRange range);
}
