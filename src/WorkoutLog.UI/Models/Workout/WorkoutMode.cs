namespace WorkoutLog.UI.Models.Workout;

/// <summary>Whether the workout page logs a new workout or edits a saved one.</summary>
public enum WorkoutMode
{
    /// <summary>New workout, with a live timer.</summary>
    New,

    /// <summary>Saved workout, with its total time.</summary>
    Edit,
}
