namespace WorkoutLog.Core.Services;

/// <summary>How an export ended.</summary>
public enum ExportStatus
{
    /// <summary>The file was written.</summary>
    Exported,

    /// <summary>The file would be over the size limit; nothing was written.</summary>
    TooLarge,

    /// <summary>Reading the workouts or writing the file failed.</summary>
    Failed,
}
