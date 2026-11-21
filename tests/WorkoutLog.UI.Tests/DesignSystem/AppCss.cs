namespace WorkoutLog.UI.Tests.DesignSystem;

/// <summary>Every stylesheet this repo owns: the app, its screens library, and the stories projects. The kit's own are in its packages.</summary>
internal static class AppCss
{
    private static readonly string[] Projects = ["WorkoutLog.UI", "WorkoutLog.App", "WorkoutLog.UI.Stories", "WorkoutLog.UI.Stories.Host"];

    /// <summary>Full paths of the stylesheets.</summary>
    public static IEnumerable<string> Files() => Projects.SelectMany(project => RepoFiles.Find("*.css", "src", project));

    /// <summary>Path relative to the repo root, with <c>/</c> separators.</summary>
    public static string Name(string file) => Path.GetRelativePath(RepoFiles.Root, file).Replace('\\', '/');
}
