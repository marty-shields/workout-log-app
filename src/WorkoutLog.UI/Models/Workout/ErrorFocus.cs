namespace WorkoutLog.UI.Models.Workout;

/// <summary>A request to move focus after a failed Save or Delete. Each instance is handled once.</summary>
/// <param name="target">Where focus moves.</param>
public sealed class ErrorFocus(ErrorFocusTarget target = ErrorFocusTarget.FirstError)
{
    /// <summary>Where focus moves.</summary>
    public ErrorFocusTarget Target { get; } = target;
}
