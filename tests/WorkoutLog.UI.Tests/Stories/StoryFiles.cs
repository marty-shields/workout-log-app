using System.Text.RegularExpressions;

using WorkoutLog.UI.Stories;

namespace WorkoutLog.UI.Tests.Stories;

/// <summary>The stories files and pages under Stories, and the title each one declares.</summary>
internal static partial class StoryFiles
{
    /// <summary>Every *.stories.razor file and *.md page.</summary>
    public static IReadOnlyList<string> All() =>
        [.. RepoFiles.Find("*", "src", "WorkoutLog.UI.Stories", "Stories").Where(f => f.EndsWith(".stories.razor") || f.EndsWith(".md"))];

    /// <summary>
    /// The title a file declares, with <c>StoryTitles.Prefix</c> filled in where the file uses it; a title written
    /// without the constant is returned as written.
    /// </summary>
    public static string Title(string file)
    {
        var match = Declared().Match(File.ReadAllText(file));
        if (!match.Success)
        {
            throw new InvalidOperationException($"No [Stories(...)] or CustomPage(...) title found in {file}");
        }

        return (match.Groups[1].Success ? StoryTitles.Prefix : "") + match.Groups[2].Value;
    }

    [GeneratedRegex("""(?:\[Stories\(|CustomPage\()(StoryTitles\.Prefix \+ )?"([^"]+)"\)""")]
    private static partial Regex Declared();
}
