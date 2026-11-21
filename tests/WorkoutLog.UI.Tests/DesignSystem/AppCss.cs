using System.Text.RegularExpressions;

namespace WorkoutLog.UI.Tests.DesignSystem;

/// <summary>Every style this repo owns: stylesheets, <c>&lt;style&gt;</c> blocks and inline <c>style=""</c> values in the app, its screens library and the stories projects. The kit's own are in its packages.</summary>
internal static partial class AppCss
{
    /// <summary>The projects whose styles and markup are scanned.</summary>
    public static readonly string[] Projects = ["WorkoutLog.UI", "WorkoutLog.App", "WorkoutLog.UI.Stories", "WorkoutLog.UI.Stories.Host"];

    /// <summary>Selector reported for a declaration in an inline <c>style=""</c> value.</summary>
    public const string InlineSelector = "style=\"\"";

    /// <summary>One declaration: where it is, the rule's selector (whitespace collapsed), and the lower-case property name.</summary>
    public readonly record struct Declaration(string File, string Selector, string Property);

    /// <summary>Full paths of the stylesheets.</summary>
    public static IEnumerable<string> Files() => Projects.SelectMany(project => RepoFiles.Find("*.css", "src", project));

    /// <summary>Full paths of the markup files that can hold a <c>&lt;style&gt;</c> block or an inline style.</summary>
    public static IEnumerable<string> MarkupFiles() =>
        Projects.SelectMany(project => RepoFiles.Find("*.razor", "src", project).Concat(RepoFiles.Find("*.html", "src", project)));

    /// <summary>Path relative to the repo root, with <c>/</c> separators.</summary>
    public static string Name(string file) => Path.GetRelativePath(RepoFiles.Root, file).Replace('\\', '/');

    /// <summary>CSS with its <c>/* */</c> comments removed.</summary>
    public static string StripComments(string css) => Comment().Replace(css, "");

    /// <summary>Every declaration in the stylesheets, in <c>&lt;style&gt;</c> blocks and in inline styles.</summary>
    public static IReadOnlyList<Declaration> All() =>
        [.. Files().SelectMany(file => InCss(Name(file), File.ReadAllText(file))),
         .. MarkupFiles().SelectMany(file => InMarkup(Name(file), File.ReadAllText(file)))];

    /// <summary>Declarations of a stylesheet's text.</summary>
    public static IEnumerable<Declaration> InCss(string file, string css) =>
        Rule().Matches(StripComments(css)).SelectMany(rule =>
        {
            var selector = Whitespace().Replace(rule.Groups[1].Value, " ").Trim();
            return Properties(rule.Groups[2].Value).Select(property => new Declaration(file, selector, property));
        });

    /// <summary>Declarations in the <c>&lt;style&gt;</c> blocks and <c>style=""</c> values of markup.</summary>
    public static IEnumerable<Declaration> InMarkup(string file, string markup) =>
        StyleBlock().Matches(markup).SelectMany(block => InCss(file, block.Groups[1].Value))
            .Concat(InlineStyle().Matches(markup).SelectMany(style =>
                Properties(style.Groups[1].Value).Select(property => new Declaration(file, InlineSelector, property))));

    private static IEnumerable<string> Properties(string body) =>
        body.Split(';')
            .Select(part => part.IndexOf(':') is var colon and > 0 ? part[..colon].Trim().ToLowerInvariant() : "")
            .Where(name => PropertyName().IsMatch(name));

    [GeneratedRegex(@"/\*.*?\*/", RegexOptions.Singleline)]
    private static partial Regex Comment();

    [GeneratedRegex(@"([^{}]+)\{([^{}]*)\}")]
    private static partial Regex Rule();

    [GeneratedRegex(@"\s+")]
    private static partial Regex Whitespace();

    [GeneratedRegex(@"^-{0,2}[a-z][a-z0-9-]*$")]
    private static partial Regex PropertyName();

    [GeneratedRegex(@"<style\b[^>]*>(.*?)</style>", RegexOptions.Singleline | RegexOptions.IgnoreCase)]
    private static partial Regex StyleBlock();

    [GeneratedRegex("\\bstyle=\"([^\"]*)\"", RegexOptions.IgnoreCase)]
    private static partial Regex InlineStyle();
}
