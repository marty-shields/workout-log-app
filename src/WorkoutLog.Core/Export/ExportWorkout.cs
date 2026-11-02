namespace WorkoutLog.Core.Export;

/// <summary>A workout in the file.</summary>
/// <param name="StartedAt">Start time, UTC.</param>
/// <param name="FinishedAt">Finish time, UTC.</param>
/// <param name="Note">The workout's note. Null when none.</param>
/// <param name="Exercises">Exercises in position order.</param>
internal sealed record ExportWorkout(DateTimeOffset StartedAt, DateTimeOffset FinishedAt, string? Note, IReadOnlyList<ExportExercise> Exercises);
