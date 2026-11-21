using Shields.UI.Testing;

namespace WorkoutLog.UI.Tests.DesignSystem;

/// <summary>This app's own tokens.css, which loads after the kit's.</summary>
internal static class AppTokens
{
    public static string Css { get; } = RepoFiles.Read("src", "WorkoutLog.UI", "wwwroot", "css", "tokens.css");

    /// <summary>Names of every custom property this app's tokens.css declares.</summary>
    public static IReadOnlyList<string> Names { get; } = DesignTokens.Declares(Css).ToList();

    /// <summary>The kit's tokens.css followed by this app's, as the page loads them; the later declaration wins.</summary>
    public static string WithKit { get; } = KitCss.Tokens + "\n" + Css;

    public static ThemedColour Colour(string name) => DesignTokens.Colour(WithKit, name);
}
