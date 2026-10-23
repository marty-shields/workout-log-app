namespace WorkoutLog.UI.Models.Workout;

/// <summary>A request to move focus after a failed Save: to the workout's first error, or to Save. Each instance is handled once.</summary>
/// <param name="focusSave">True to focus Save, after a save that failed to write (D176).</param>
public sealed class ErrorFocus(bool focusSave = false)
{
    /// <summary>True to focus Save instead of the first error.</summary>
    public bool FocusSave { get; } = focusSave;
}
