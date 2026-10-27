namespace WorkoutLog.Core.Services;

/// <summary>Writes every saved workout to one JSON file (FR-51).</summary>
public interface IExportService
{
    /// <summary>Returns the number of saved workouts. A failed read returns <see cref="ExportCount.Failed"/> and 0.</summary>
    Task<ExportCount> CountAsync();

    /// <summary>
    /// Writes every saved workout to <c>workout-log-YYYY-MM-DD-HHmm.json</c> (local time) in <see cref="IExportEnvironment.CacheFolder"/>,
    /// replacing a file of the same name. A file over <see cref="Export.ExportFormat.MaxBytes"/> is not written.
    /// </summary>
    Task<ExportResult> ExportAsync();
}
