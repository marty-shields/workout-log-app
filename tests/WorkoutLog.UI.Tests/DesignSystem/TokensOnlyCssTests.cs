using System.Text.RegularExpressions;

namespace WorkoutLog.UI.Tests.DesignSystem;

public partial class TokensOnlyCssTests
{
    // Literal values allowed outside tokens.css: the safe-area fallbacks and the visually-hidden helper.
    private static readonly HashSet<string> Allowed =
    [
        "base.css: --inset-top: env(safe-area-inset-top, 0px);",
        "base.css: --inset-bottom: env(safe-area-inset-bottom, 0px);",
        "base.css: width: 1px;",
        "base.css: height: 1px;",
        "base.css: margin: -1px;",
    ];

    private static IEnumerable<TestCaseData> StyleFiles() =>
        RepoFiles.Find("*.css", "src", "WorkoutLog.UI")
            .Where(f => Path.GetFileName(f) != "tokens.css")
            .Select(f => new TestCaseData(f).SetArgDisplayNames(Path.GetFileName(f)));

    private static IEnumerable<TestCaseData> MarkupFiles() =>
        RepoFiles.Find("*.razor", "src", "WorkoutLog.UI")
            .Select(f => new TestCaseData(f).SetArgDisplayNames(Path.GetFileName(f)));

    [TestCaseSource(nameof(StyleFiles))]
    public void Css_HasNoLiteralColoursOrSizes(string file)
    {
        var literals = StripComments(File.ReadAllText(file)).Split('\n')
            .Select(line => line.Trim())
            .Where(line => Literal().IsMatch(line))
            .Select(line => $"{Path.GetFileName(file)}: {line}")
            .Where(line => !Allowed.Contains(line));

        Assert.That(literals, Is.Empty);
    }

    [TestCaseSource(nameof(MarkupFiles))]
    public void InlineStyles_HaveNoLiteralColoursOrSizes(string file)
    {
        var literals = InlineStyle().Matches(File.ReadAllText(file))
            .Select(m => m.Groups[1].Value)
            .Where(style => Literal().IsMatch(style));

        Assert.That(literals, Is.Empty);
    }

    [TestCaseSource(nameof(StyleFiles))]
    public void Css_UsesOnlyDeclaredTokens(string file)
    {
        var css = File.ReadAllText(file);

        var unknown = DesignTokens.Uses(css).Except(DesignTokens.Names).Except(DesignTokens.Declares(css));

        Assert.That(unknown, Is.Empty);
    }

    private static string StripComments(string css) => Comment().Replace(css, "");

    // Hex, colour functions, named colours in a value, sizes with units, and percentages other than 100%.
    [GeneratedRegex(@"#[0-9a-fA-F]{3,8}\b|\b(rgba?|hsla?|hwb|lab|lch|oklab|oklch|color|color-mix)\(|(?<=:[^;{}]*)\b(" + NamedColours + @")\b|\d(px|rem|em|vh|vw|vmin|vmax|pt)\b|(?<![\d.])(?!100%)\d+(\.\d+)?%", RegexOptions.IgnoreCase)]
    private static partial Regex Literal();

    private const string NamedColours =
        "aliceblue|antiquewhite|aqua|aquamarine|azure|beige|bisque|black|blanchedalmond|blue|blueviolet|brown|burlywood|" +
        "cadetblue|chartreuse|chocolate|coral|cornflowerblue|cornsilk|crimson|cyan|darkblue|darkcyan|darkgoldenrod|darkgray|" +
        "darkgreen|darkgrey|darkkhaki|darkmagenta|darkolivegreen|darkorange|darkorchid|darkred|darksalmon|darkseagreen|" +
        "darkslateblue|darkslategray|darkslategrey|darkturquoise|darkviolet|deeppink|deepskyblue|dimgray|dimgrey|dodgerblue|" +
        "firebrick|floralwhite|forestgreen|fuchsia|gainsboro|ghostwhite|gold|goldenrod|gray|green|greenyellow|grey|honeydew|" +
        "hotpink|indianred|indigo|ivory|khaki|lavender|lavenderblush|lawngreen|lemonchiffon|lightblue|lightcoral|lightcyan|" +
        "lightgoldenrodyellow|lightgray|lightgreen|lightgrey|lightpink|lightsalmon|lightseagreen|lightskyblue|lightslategray|" +
        "lightslategrey|lightsteelblue|lightyellow|lime|limegreen|linen|magenta|maroon|mediumaquamarine|mediumblue|" +
        "mediumorchid|mediumpurple|mediumseagreen|mediumslateblue|mediumspringgreen|mediumturquoise|mediumvioletred|" +
        "midnightblue|mintcream|mistyrose|moccasin|navajowhite|navy|oldlace|olive|olivedrab|orange|orangered|orchid|" +
        "palegoldenrod|palegreen|paleturquoise|palevioletred|papayawhip|peachpuff|peru|pink|plum|powderblue|purple|" +
        "rebeccapurple|red|rosybrown|royalblue|saddlebrown|salmon|sandybrown|seagreen|seashell|sienna|silver|skyblue|" +
        "slateblue|slategray|slategrey|snow|springgreen|steelblue|tan|teal|thistle|tomato|turquoise|violet|wheat|white|" +
        "whitesmoke|yellow|yellowgreen";

    [GeneratedRegex(@"/\*.*?\*/", RegexOptions.Singleline)]
    private static partial Regex Comment();

    [GeneratedRegex("\\bstyle=\"([^\"]*)\"")]
    private static partial Regex InlineStyle();
}
