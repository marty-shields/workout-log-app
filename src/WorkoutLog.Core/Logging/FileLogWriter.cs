namespace WorkoutLog.Core.Logging;

/// <summary>
/// Appends lines to <c>workoutlog.log</c> in a folder, one write at a time.
/// When the file reaches the size cap it becomes <c>workoutlog.1.log</c>, replacing the old one.
/// </summary>
/// <param name="folder">Folder for the log files. Created on the first write.</param>
/// <param name="maxBytes">Size at which the file rolls over.</param>
public sealed class FileLogWriter(string folder, long maxBytes)
{
    /// <summary>The file name.</summary>
    public const string FileName = "workoutlog.log";

    /// <summary>The name of the previous file.</summary>
    public const string OldFileName = "workoutlog.1.log";

    /// <summary>The default size cap: 1 MB.</summary>
    public const long DefaultMaxBytes = 1024 * 1024;

    private readonly Lock gate = new();
    private readonly string path = Path.Combine(folder, FileName);
    private readonly string oldPath = Path.Combine(folder, OldFileName);

    /// <summary>Appends <paramref name="text"/> and a new line. Any write error is ignored.</summary>
    public void Write(string text)
    {
        lock (gate)
        {
            try
            {
                Directory.CreateDirectory(folder);
                var file = new FileInfo(path);
                if (file.Exists && file.Length >= maxBytes)
                {
                    File.Move(path, oldPath, overwrite: true);
                }

                File.AppendAllText(path, text + Environment.NewLine);
            }
            catch (Exception)
            {
            }
        }
    }
}
