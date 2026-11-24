using WorkoutLog.UI.Models.Workout;

namespace WorkoutLog.UI.Models.Exercises;

/// <summary>An exercise in the add-exercise list.</summary>
/// <param name="Kind">Strength or cardio.</param>
/// <param name="Last">Date and best of the last session, e.g. Tue 20 Oct · 70kg × 3; null when not logged or not loaded.</param>
/// <param name="LastStartedAt">When the last saved workout with it started, to order by; null when not logged or not loaded. Not shown.</param>
public sealed record ExerciseListItem(int Id, string Name, ExerciseKind Kind, string? Last, DateTimeOffset? LastStartedAt = null);
