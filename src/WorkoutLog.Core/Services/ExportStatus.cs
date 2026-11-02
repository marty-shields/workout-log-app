namespace WorkoutLog.Core.Services;

public enum ExportStatus
{
    Exported,

    /// <summary>The file would be over the size limit; nothing was written.</summary>
    TooLarge,

    /// <summary>Reading the workouts or writing the file failed.</summary>
    Failed,
}
