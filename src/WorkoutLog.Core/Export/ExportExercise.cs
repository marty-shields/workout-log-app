namespace WorkoutLog.Core.Export;

/// <summary>An exercise in a workout: sets for strength, an entry for cardio.</summary>
/// <param name="Exercise">The exercise.</param>
/// <param name="Note">The exercise's note in the workout. Null when none.</param>
/// <param name="Sets">Sets in the order added. Null for cardio.</param>
/// <param name="Entry">The entry. Null for strength.</param>
internal sealed record ExportExercise(ExportExerciseInfo Exercise, string? Note, IReadOnlyList<ExportSet>? Sets, ExportEntry? Entry);
