namespace WorkoutLog.UI.Models.Exercises;

/// <summary>A logged exercise in the Exercises list.</summary>
/// <param name="Id">Exercise id.</param>
/// <param name="Name">Exercise name.</param>
/// <param name="Last">Date and best of the last session, e.g. Tue 20 Oct · 70kg × 3.</param>
public sealed record ExerciseListItem(int Id, string Name, string Last);
