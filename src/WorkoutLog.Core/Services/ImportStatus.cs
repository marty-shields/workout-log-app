namespace WorkoutLog.Core.Services;

public enum ImportStatus
{
    /// <summary>Every write was made.</summary>
    Imported,

    /// <summary>The write failed; nothing was written.</summary>
    Failed,
}
