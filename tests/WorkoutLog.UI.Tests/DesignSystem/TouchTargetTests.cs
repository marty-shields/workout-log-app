using System.Text.RegularExpressions;

namespace WorkoutLog.UI.Tests.DesignSystem;

public partial class TouchTargetTests
{
    private const string TouchTarget = "var(--touch-target)";

    private static IEnumerable<TestCaseData> Controls() =>
        RepoFiles.Find("*.razor", "src", "WorkoutLog.UI")
            .SelectMany(file => Element().Matches(File.ReadAllText(file)).Select(m => (File: file, Tag: m.Groups[1].Value, Attributes: m.Groups[2].Value)))
            .Select(c => (c.File, c.Tag, Class: FirstClass(c.Attributes), Icon: c.Tag == "button" && c.Attributes.Contains("aria-label=")))
            .Distinct()
            .Select(c => new TestCaseData(c.File, c.Class, c.Icon).SetArgDisplayNames($"{Path.GetFileName(c.File)} {c.Tag}.{c.Class}"));

    [TestCaseSource(nameof(Controls))]
    public void Control_IsAtLeastATouchTarget(string razorFile, string? cssClass, bool icon)
    {
        Assert.That(cssClass, Is.Not.Null, "Buttons and inputs need a class that sizes them");
        var declarations = Rules(File.ReadAllText(razorFile + ".css"))
            .Where(r => r.Selectors.Contains($".{cssClass}"))
            .SelectMany(r => r.Declarations)
            .ToList();

        Assert.Multiple(() =>
        {
            Assert.That(declarations, Has.Member($"min-height: {TouchTarget}"));
            if (icon)
            {
                Assert.That(declarations, Has.Member($"min-width: {TouchTarget}"));
            }
        });
    }

    private static string? FirstClass(string attributes) =>
        Class().Match(attributes) is { Success: true } m
            ? m.Groups[1].Value.Split(' ', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault(c => !c.StartsWith('@'))
            : null;

    private static IEnumerable<(string[] Selectors, string[] Declarations)> Rules(string css) =>
        Rule().Matches(Comment().Replace(css, "")).Select(m => (
            m.Groups[1].Value.Split(',', StringSplitOptions.TrimEntries),
            m.Groups[2].Value.Split(';', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)));

    // Opening tag of a button or input; quoted attribute values may contain '>'.
    [GeneratedRegex("""<(button|input)\b((?:[^>"]|"[^"]*")*)>""")]
    private static partial Regex Element();

    [GeneratedRegex("\\bclass=\"([^\"]*)\"")]
    private static partial Regex Class();

    [GeneratedRegex(@"([^{}]+)\{([^{}]*)\}")]
    private static partial Regex Rule();

    [GeneratedRegex(@"/\*.*?\*/", RegexOptions.Singleline)]
    private static partial Regex Comment();
}
