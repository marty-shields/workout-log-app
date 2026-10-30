using WorkoutLog.Core.Domain.Exercises.Progress;

namespace WorkoutLog.Core.Services;

/// <summary>Reads a cardio exercise's sessions and works out its chart values and records.</summary>
public interface ICardioProgressService
{
    /// <summary>
    /// Returns every session of the exercise, by start time oldest first.
    /// An id not in the exercise list, an exercise that isn't cardio, or a failed read returns <see cref="CardioProgressResult.Failed"/> and no sessions.
    /// </summary>
    Task<CardioProgressResult> LoadAsync(int exerciseId);

    /// <summary>The sessions of <paramref name="sessions"/> in <paramref name="range"/>, each with its chart values, and their records.</summary>
    CardioProgress Summarise(IReadOnlyList<CardioSession> sessions, ChartRange range);
}
