using WorkoutLog.UI.Import;

namespace WorkoutLog.App.Import;

/// <summary>Opens Android's file picker through MAUI <see cref="FilePicker"/> for JSON files.</summary>
internal sealed class MauiImportFilePicker : IImportFilePicker
{
    /// <summary>JSON files; some file managers give them as octet-stream.</summary>
    private static readonly FilePickerFileType JsonFiles = new(new Dictionary<DevicePlatform, IEnumerable<string>>
    {
        [DevicePlatform.Android] = ["application/json", "application/octet-stream"],
    });

    /// <inheritdoc />
    /// <remarks>The size comes from the picker's local copy of the file; null when it can't be read.</remarks>
    public async Task<PickedFile?> PickAsync()
    {
        if (await FilePicker.Default.PickAsync(new PickOptions { PickerTitle = "Import workouts", FileTypes = JsonFiles }) is not { } file)
        {
            return null;
        }

        return new PickedFile(file.FileName, Size(file.FullPath), file.OpenReadAsync);
    }

    private static long? Size(string path)
    {
        try
        {
            return new FileInfo(path).Length;
        }
        catch (Exception)
        {
            return null;
        }
    }
}
