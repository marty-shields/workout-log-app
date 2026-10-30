namespace WorkoutLog.Core.Import;

/// <summary>Reads an export file into workouts for the import checks.</summary>
internal interface IImportFileReader
{
    /// <summary>
    /// Reads <paramref name="stream"/>, returning <see cref="Services.ImportError.TooLarge"/> once it passes <see cref="Export.ExportFormat.MaxBytes"/>;
    /// upgrades an older format version, checks the header and splits the workouts. A workout that can't be read is returned as <see cref="ImportWorkout.Malformed"/>.
    /// Throws when the stream can't be read.
    /// </summary>
    Task<ImportFileRead> ReadAsync(Stream stream);
}
