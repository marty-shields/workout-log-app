namespace WorkoutLog.UI.Models.Workout;

/// <summary>Where an <see cref="ErrorFocus"/> request moves focus.</summary>
public enum ErrorFocusTarget
{
    /// <summary>The workout's first error (D171).</summary>
    FirstError,

    /// <summary>Save, after a save that failed to write (D176).</summary>
    Save,

    /// <summary>Delete, after a delete that failed (D191).</summary>
    Delete,
}
