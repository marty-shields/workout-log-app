using WorkoutLog.UI.Import;

namespace WorkoutLog.UI.Stories.Samples;

/// <summary>A file picker for the stories app: always cancelled. Stories show views only, so it is never called.</summary>
public sealed class SampleImportFilePicker : IImportFilePicker
{
    /// <inheritdoc />
    public Task<PickedFile?> PickAsync() => Task.FromResult<PickedFile?>(null);
}
