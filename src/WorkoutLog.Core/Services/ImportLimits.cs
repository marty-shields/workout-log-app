namespace WorkoutLog.Core.Services;

/// <summary>Limits of an import file.</summary>
public static class ImportLimits
{
    /// <summary>Largest file that can be imported, in bytes: 5 MB.</summary>
    public const int MaxFileBytes = Export.ExportFormat.MaxBytes;
}
