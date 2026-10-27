namespace WorkoutLog.Core.Services;

/// <summary>How an import ended.</summary>
public enum ImportStatus
{
    /// <summary>Every write was made.</summary>
    Imported,

    /// <summary>The write failed; nothing was written.</summary>
    Failed,
}
