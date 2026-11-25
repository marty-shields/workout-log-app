namespace WorkoutLog.Core.Services;

/// <summary>Whether a write to the unfinished workout succeeded.</summary>
public enum UnfinishedWrite
{
    Done,

    /// <summary>The write failed, and the service has logged why. The stored unfinished workout is as it was.</summary>
    Failed,
}
