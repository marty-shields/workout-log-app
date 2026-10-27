namespace WorkoutLog.Core.Export;

/// <summary>A workout in the file.</summary>
/// <param name="StartedAt">Start time, UTC.</param>
/// <param name="FinishedAt">Finish time, UTC.</param>
/// <param name="Exercises">Exercises in position order.</param>
internal sealed record ExportWorkout(DateTimeOffset StartedAt, DateTimeOffset FinishedAt, IReadOnlyList<ExportExercise> Exercises);
