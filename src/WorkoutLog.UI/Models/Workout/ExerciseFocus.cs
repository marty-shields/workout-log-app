namespace WorkoutLog.UI.Models.Workout;

/// <summary>A request to move focus to an exercise's card. Each instance is handled once.</summary>
/// <param name="addSet">Strength only: add a set first, as Add set does, then focus it.</param>
public sealed class ExerciseFocus(ExerciseState exercise, bool addSet = false)
{
    public ExerciseState Exercise { get; } = exercise;

    /// <summary>Strength only: add a set first, as Add set does, then focus it.</summary>
    public bool AddSet { get; } = addSet;
}
