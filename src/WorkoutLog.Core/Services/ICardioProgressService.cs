using WorkoutLog.Core.Domain;
using WorkoutLog.Core.Domain.Exercises.Progress;

namespace WorkoutLog.Core.Services;

/// <summary>Reads a cardio exercise's sessions and works out its chart values and records.</summary>
public interface ICardioProgressService
{
    /// <summary>
    /// Returns every session of the exercise, by start time oldest first.
    /// An id not in the exercise list or an exercise that isn't cardio returns <see cref="ReadFailed"/>, as a failed read does.
    /// </summary>
    Task<ReadResult<IReadOnlyList<CardioSession>>> LoadAsync(int exerciseId);

    /// <summary>The sessions of <paramref name="sessions"/> in <paramref name="range"/>, each with its chart values, and their records.</summary>
    CardioProgress Summarise(IReadOnlyList<CardioSession> sessions, HistoryRange range);
}
