using WorkoutLog.UI.Sharing;

namespace WorkoutLog.App.Sharing;

/// <summary>Opens Android's share sheet for a JSON file through MAUI <see cref="Share"/>.</summary>
internal sealed class MauiFileShare : IFileShare
{
    /// <inheritdoc />
    public Task ShareAsync(string filePath, string title) =>
        Share.Default.RequestAsync(new ShareFileRequest(title, new ShareFile(filePath, "application/json")));
}
