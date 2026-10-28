using WorkoutLog.Core.Domain.Exercises.Progress;

namespace WorkoutLog.Core.Services;

/// <summary>Outcome of loading an exercise's sessions.</summary>
/// <param name="Sessions">Sessions, by start time oldest first. Empty when loading failed.</param>
/// <param name="Failed">True when the sessions couldn't be read, or the id isn't an exercise with an Exercise page.</param>
public sealed record ExerciseProgressResult(IReadOnlyList<StrengthSession> Sessions, bool Failed);
