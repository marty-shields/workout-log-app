using WorkoutLog.Core.Domain.Exercises.Progress;

namespace WorkoutLog.Core.Services;

/// <summary>Outcome of loading a lift's sessions.</summary>
/// <param name="Sessions">Sessions, by start time oldest first. Empty when loading failed.</param>
/// <param name="Failed">True when the sessions couldn't be read, or the id isn't a lift in the exercise list.</param>
public sealed record ExerciseProgressResult(IReadOnlyList<LiftSession> Sessions, bool Failed);
