namespace WorkoutLog.UI.Tests;

/// <summary>Source files in the repo, found from the test output folder.</summary>
internal static class RepoFiles
{
    /// <summary>Repo root: the folder that holds WorkoutLog.slnx.</summary>
    public static string Root { get; } = FindRoot();

    /// <summary>Full path of a file or folder under the repo root.</summary>
    public static string PathOf(params string[] parts) => Path.Combine([Root, .. parts]);

    /// <summary>Text of a file under the repo root.</summary>
    public static string Read(params string[] parts) => File.ReadAllText(PathOf(parts));

    /// <summary>Files under a repo folder matching a pattern, excluding bin and obj.</summary>
    public static IEnumerable<string> Find(string pattern, params string[] folder) =>
        Directory.EnumerateFiles(PathOf(folder), pattern, SearchOption.AllDirectories)
            .Where(f => !f.Split(Path.DirectorySeparatorChar).Any(part => part is "bin" or "obj"));

    private static string FindRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (!File.Exists(Path.Combine(directory.FullName, "WorkoutLog.slnx")))
        {
            directory = directory.Parent!;
        }

        return directory.FullName;
    }
}
