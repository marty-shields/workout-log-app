namespace WorkoutLog.Core.Services;

/// <summary>Outcome of an export.</summary>
/// <param name="Status">How the export ended.</param>
/// <param name="FilePath">Full path of the written file. Null unless <see cref="ExportStatus.Exported"/>.</param>
public sealed record ExportResult(ExportStatus Status, string? FilePath);
