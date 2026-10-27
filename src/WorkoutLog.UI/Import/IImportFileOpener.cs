using WorkoutLog.Core.Services;

namespace WorkoutLog.UI.Import;

/// <summary>Picks an import file and reads it.</summary>
public interface IImportFileOpener
{
    /// <summary>
    /// Opens the picker, then reads the picked file with <see cref="IImportService.ReadAsync"/>.
    /// Returns null when the picker is cancelled. A file over <see cref="ImportLimits.MaxFileBytes"/> by its picked size is
    /// <see cref="ImportError.TooLarge"/> without being opened. Throws when the picker or opening the file fails.
    /// </summary>
    Task<ImportReview?> OpenAsync();
}
