namespace WorkoutLog.UI.Sharing;

/// <summary>Opens the phone's share sheet for a file. Implemented by the host.</summary>
public interface IFileShare
{
    /// <summary>Opens the share sheet for the file at <paramref name="filePath"/>, with <paramref name="title"/> as its title.</summary>
    Task ShareAsync(string filePath, string title);
}
