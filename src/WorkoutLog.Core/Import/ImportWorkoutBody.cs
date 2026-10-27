namespace WorkoutLog.Core.Import;

/// <summary>The exercises of a workout in the file. The times are read separately.</summary>
/// <param name="Exercises">Exercises in position order. Required.</param>
internal sealed record ImportWorkoutBody(IReadOnlyList<ImportExercise> Exercises);
