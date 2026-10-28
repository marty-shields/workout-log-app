using WorkoutLog.UI.Models.Workout;

namespace WorkoutLog.UI.Models.Exercises;

/// <summary>An exercise in the Exercises list or the add-exercise list.</summary>
/// <param name="Id">Exercise id.</param>
/// <param name="Name">Exercise name.</param>
/// <param name="Kind">Strength or cardio.</param>
/// <param name="Last">Date and best of the last session, e.g. Tue 20 Oct · 70kg × 3; null when not logged or not loaded.</param>
/// <param name="Opens">True when the exercise has an Exercise page: every strength exercise (FR-60).</param>
public sealed record ExerciseListItem(int Id, string Name, ExerciseKind Kind, string? Last, bool Opens = false);
