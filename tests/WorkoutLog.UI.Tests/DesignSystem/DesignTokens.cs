using System.Globalization;
using System.Text.RegularExpressions;

namespace WorkoutLog.UI.Tests.DesignSystem;

/// <summary>Custom properties declared in tokens.css and base.css.</summary>
internal static partial class DesignTokens
{
    private static readonly string TokensCss = RepoFiles.Read("src", "WorkoutLog.UI", "wwwroot", "css", "tokens.css");
    private static readonly string BaseCss = RepoFiles.Read("src", "WorkoutLog.UI", "wwwroot", "css", "base.css");

    /// <summary>Names of every custom property declared in tokens.css and base.css.</summary>
    public static IReadOnlySet<string> Names { get; } =
        Declaration().Matches(TokensCss + BaseCss).Select(m => m.Groups[1].Value).ToHashSet();

    /// <summary>Light and dark value of a colour token: a <c>light-dark()</c> pair or one hex value for both.</summary>
    public static (Rgb Light, Rgb Dark) Colour(string name)
    {
        var value = Declaration().Matches(TokensCss).Single(m => m.Groups[1].Value == name).Groups[2].Value.Trim();
        var hexes = Hex().Matches(value).Select(m => Rgb.Parse(m.Value)).ToList();
        return hexes.Count == 1 ? (hexes[0], hexes[0]) : (hexes[0], hexes[1]);
    }

    /// <summary>Custom properties used through <c>var()</c> in a piece of CSS.</summary>
    public static IEnumerable<string> Uses(string css) => Use().Matches(css).Select(m => m.Groups[1].Value);

    /// <summary>Custom properties declared in a piece of CSS.</summary>
    public static IEnumerable<string> Declares(string css) => Declaration().Matches(css).Select(m => m.Groups[1].Value);

    [GeneratedRegex(@"^\s*(--[a-z0-9-]+)\s*:([^;]*);", RegexOptions.Multiline)]
    private static partial Regex Declaration();

    [GeneratedRegex(@"var\((--[a-z0-9-]+)")]
    private static partial Regex Use();

    [GeneratedRegex("#[0-9a-fA-F]{6}")]
    private static partial Regex Hex();
}

/// <summary>An sRGB colour.</summary>
internal readonly record struct Rgb(int R, int G, int B)
{
    public static Rgb Parse(string hex) => new(
        int.Parse(hex[1..3], NumberStyles.HexNumber),
        int.Parse(hex[3..5], NumberStyles.HexNumber),
        int.Parse(hex[5..7], NumberStyles.HexNumber));

    /// <summary>WCAG 2 relative luminance.</summary>
    public double Luminance => 0.2126 * Channel(R) + 0.7152 * Channel(G) + 0.0722 * Channel(B);

    /// <summary>WCAG 2 contrast ratio between two colours.</summary>
    public static double Contrast(Rgb a, Rgb b)
    {
        var (light, dark) = a.Luminance > b.Luminance ? (a.Luminance, b.Luminance) : (b.Luminance, a.Luminance);
        return (light + 0.05) / (dark + 0.05);
    }

    public override string ToString() => $"#{R:x2}{G:x2}{B:x2}";

    private static double Channel(int value)
    {
        var c = value / 255.0;
        return c <= 0.04045 ? c / 12.92 : Math.Pow((c + 0.055) / 1.055, 2.4);
    }
}
