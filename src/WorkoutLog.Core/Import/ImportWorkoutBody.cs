using System.Text.Json.Serialization;

namespace WorkoutLog.Core.Import;

/// <summary>The exercises and note of a workout in the file. The times are read separately.</summary>
/// <param name="Exercises">Exercises in position order. Required.</param>
/// <param name="Note">The workout's note.</param>
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Skip)]
internal sealed record ImportWorkoutBody(IReadOnlyList<ImportExercise> Exercises, string? Note = null);
