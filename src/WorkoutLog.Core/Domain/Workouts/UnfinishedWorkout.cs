namespace WorkoutLog.Core.Domain.Workouts;

/// <summary>
/// A new workout still open, as typed: kept so a crash or kill doesn't lose it. Nothing is checked, so values may be blank, half-typed or invalid.
/// </summary>
/// <param name="StartedAt">Start time, UTC.</param>
/// <param name="Note">The workout's note as typed; null when none.</param>
/// <param name="Exercises">Exercises in the order they were added, oldest first.</param>
public sealed record UnfinishedWorkout(DateTimeOffset StartedAt, string? Note, IReadOnlyList<UnfinishedExercise> Exercises);

/// <summary>An exercise of an <see cref="UnfinishedWorkout"/>.</summary>
/// <param name="Note">The exercise's note as typed; null when none.</param>
/// <param name="IsDone">The exercise was marked done.</param>
/// <param name="Entries">Sets (strength) or the entry (cardio), in order.</param>
public sealed record UnfinishedExercise(int ExerciseId, string? Note, bool IsDone, IReadOnlyList<UnfinishedEntry> Entries);

/// <summary>A set or entry of an <see cref="UnfinishedExercise"/>.</summary>
/// <param name="Values">The text typed in each field, by the field's key; null for a field never typed in.</param>
public sealed record UnfinishedEntry(IReadOnlyDictionary<string, string?> Values);
