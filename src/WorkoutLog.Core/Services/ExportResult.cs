namespace WorkoutLog.Core.Services;

/// <param name="FilePath">Full path of the written file.</param>
public sealed record ExportedFile(string FilePath);

/// <summary>The file would be over the size limit; nothing was written.</summary>
public sealed record ExportTooLarge;

/// <summary>Reading the workouts or writing the file failed.</summary>
public sealed record ExportFailed;

public union ExportResult(ExportedFile, ExportTooLarge, ExportFailed);
