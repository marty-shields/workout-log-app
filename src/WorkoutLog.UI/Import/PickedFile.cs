namespace WorkoutLog.UI.Import;

/// <summary>A file the user picked.</summary>
/// <param name="Name">File name.</param>
/// <param name="Size">Size in bytes, or null when unknown.</param>
/// <param name="Open">Opens the file for reading. The caller disposes the stream.</param>
public sealed record PickedFile(string Name, long? Size, Func<Task<Stream>> Open);
