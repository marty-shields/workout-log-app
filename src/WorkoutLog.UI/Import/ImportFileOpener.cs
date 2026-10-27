using Microsoft.Extensions.Logging;
using WorkoutLog.Core.Services;
using WorkoutLog.UI.Logging;

namespace WorkoutLog.UI.Import;

/// <summary>Picks the file with <see cref="IImportFilePicker"/> and reads it with <see cref="IImportService"/>. Logs a file too large by its size.</summary>
internal sealed partial class ImportFileOpener(IImportFilePicker picker, IImportService import, ILogger<ImportFileOpener> logger) : IImportFileOpener
{
    /// <inheritdoc />
    public async Task<ImportReview?> OpenAsync()
    {
        if (await picker.PickAsync() is not { } file)
        {
            return null;
        }

        if (file.Size is { } size && size > ImportLimits.MaxFileBytes)
        {
            Log.TooLarge(logger, size);
            return ImportReview.Rejected(file.Name, ImportError.TooLarge);
        }

        await using var stream = await file.Open();
        return await import.ReadAsync(stream, file.Name);
    }

    private static partial class Log
    {
        [LoggerMessage((int)UiEvent.ImportFileTooLarge, LogLevel.Warning, "The picked import file is {Bytes} bytes, over the limit; not read")]
        public static partial void TooLarge(ILogger logger, long bytes);
    }
}
