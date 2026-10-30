namespace WorkoutLog.UI.Models.Workout;

/// <summary>Where an <see cref="ErrorFocus"/> request moves focus.</summary>
public enum ErrorFocusTarget
{
    /// <summary>The workout's first error.</summary>
    FirstError,

    /// <summary>Save, after a save that failed to write.</summary>
    Save,

    /// <summary>Delete, after a delete that failed.</summary>
    Delete,
}
