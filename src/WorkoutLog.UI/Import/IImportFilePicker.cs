namespace WorkoutLog.UI.Import;

/// <summary>Opens the phone's file picker for a JSON file. Implemented by the host.</summary>
public interface IImportFilePicker
{
    /// <summary>Shows the picker. Returns the picked file, or null when it is cancelled.</summary>
    Task<PickedFile?> PickAsync();
}
