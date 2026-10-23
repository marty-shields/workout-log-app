namespace WorkoutLog.UI.Models.Workout;

/// <summary>A request to move focus to an exercise's card. Each instance is handled once.</summary>
/// <param name="exercise">Exercise to focus.</param>
/// <param name="addSet">Strength only: add a set first, as Add set does, then focus it.</param>
public sealed class ExerciseFocus(ExerciseState exercise, bool addSet = false)
{
    /// <summary>Exercise to focus.</summary>
    public ExerciseState Exercise { get; } = exercise;

    /// <summary>Strength only: add a set first, as Add set does, then focus it.</summary>
    public bool AddSet { get; } = addSet;
}
