namespace WorkoutLog.UI.Models.Workout;

/// <summary>An exercise listed in the exercise picker.</summary>
/// <param name="Id">Exercise id.</param>
/// <param name="Name">Exercise name.</param>
/// <param name="Kind">Strength or cardio.</param>
public sealed record PickerItem(int Id, string Name, ExerciseKind Kind);
