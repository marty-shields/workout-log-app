namespace WorkoutLog.UI.Models.Workout;

/// <summary>Where in an exercise's card <see cref="ExerciseFocus"/> moves focus.</summary>
public enum ExerciseFocusTarget
{
    /// <summary>The open set, or the cardio entry.</summary>
    OpenSet,

    /// <summary>Strength: a set added first, as Add set does, then that set. Cardio: the entry.</summary>
    AddedSet,

    /// <summary>What Undo put back: the set's Edit, or with no <see cref="ExerciseFocus.Set"/> the card's first button.</summary>
    Restored,
}

/// <summary>A request to move focus to an exercise's card. Each instance is handled once.</summary>
/// <param name="set">With <see cref="ExerciseFocusTarget.Restored"/>: the set Undo put back. Null for the exercise itself.</param>
public sealed class ExerciseFocus(ExerciseState exercise, ExerciseFocusTarget target = ExerciseFocusTarget.OpenSet, EntryState? set = null)
{
    public ExerciseState Exercise { get; } = exercise;

    public ExerciseFocusTarget Target { get; } = target;

    /// <summary>With <see cref="ExerciseFocusTarget.Restored"/>: the set Undo put back. Null for the exercise itself.</summary>
    public EntryState? Set { get; } = set;
}
