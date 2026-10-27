namespace WorkoutLog.UI.Models.Settings;

/// <summary>Message under Export on Settings.</summary>
public enum ExportMessage
{
    /// <summary>No message.</summary>
    None,

    /// <summary>The export or share failed: "Couldn't export your workouts. Try again".</summary>
    Failed,

    /// <summary>The file would be over the size limit: "Too many workouts to export".</summary>
    TooLarge,
}
