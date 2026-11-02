using System.Text.RegularExpressions;

namespace WorkoutLog.UI.Tests.Conventions;

/// <summary>Every tag on an E2E scenario or feature is one docs/e2e-tests.md lists under Tags.</summary>
public partial class E2eTagTests
{
    [GeneratedRegex(@"`@([\w-]+)`")]
    private static partial Regex DocumentedTag();

    [GeneratedRegex(@"^## Tags[ \t]*\r?$", RegexOptions.Multiline)]
    private static partial Regex TagsHeading();

    [GeneratedRegex(@"^## ", RegexOptions.Multiline)]
    private static partial Regex NextHeading();

    [GeneratedRegex(@"@(\S+)")]
    private static partial Regex Tag();

    internal static string[] TagsIn(string feature) =>
        feature.Split('\n')
            .Select(line => line.TrimStart())
            .Where(line => line.StartsWith('@'))
            .Select(line => line.Split('#')[0])
            .SelectMany(line => Tag().Matches(line).Select(m => m.Groups[1].Value))
            .ToArray();

    private static string[] Documented()
    {
        var docs = RepoFiles.Read("docs", "e2e-tests.md");
        var heading = TagsHeading().Match(docs);
        Assert.That(heading.Success, "docs/e2e-tests.md needs a '## Tags' heading listing each tag as `@name`.");
        var body = docs[(heading.Index + heading.Length)..];
        var next = NextHeading().Match(body);
        var section = next.Success ? body[..next.Index] : body;
        return DocumentedTag().Matches(section).Select(m => m.Groups[1].Value).Distinct().ToArray();
    }

    private static IEnumerable<string> FeatureFiles() => RepoFiles.Find("*.feature", "tests", "WorkoutLog.E2E.Tests");

    private static IEnumerable<TestCaseData> Features() =>
        FeatureFiles().Select(f => new TestCaseData(f).SetArgDisplayNames(Path.GetFileName(f)));

    [Test]
    public void TagsSection_ListsOnlyTagsInUse()
    {
        var used = FeatureFiles().SelectMany(f => TagsIn(File.ReadAllText(f))).Distinct();
        Assert.That(Documented().Except(used), Is.Empty, "Drop the tag from docs/e2e-tests.md Tags, or use it.");
    }

    [TestCaseSource(nameof(Features))]
    public void Feature_UsesOnlyDocumentedTags(string file) =>
        Assert.That(TagsIn(File.ReadAllText(file)).Except(Documented()), Is.Empty,
            "Add the tag to docs/e2e-tests.md Tags, or drop it. Story tags such as @US-17 were dropped in #144.");

    [Test]
    public void TagsIn_ReadsEveryTagTheFeatureCarries()
    {
        Assert.That(TagsIn("@smoke @release\nFeature: A"), Is.EqualTo(new[] { "smoke", "release" }));
        Assert.That(TagsIn("@smoke @release @US-17 # story tag\nFeature: A"), Is.EqualTo(new[] { "smoke", "release", "US-17" }));
        Assert.That(TagsIn("@smoke\n@US-17\nFeature: A"), Is.EqualTo(new[] { "smoke", "US-17" }));
        Assert.That(TagsIn("  @smoke\t@US-17\r\nFeature: A"), Is.EqualTo(new[] { "smoke", "US-17" }));
        Assert.That(TagsIn("@smoke @US-17 and some text"), Is.EqualTo(new[] { "smoke", "US-17" }));
    }

    [Test]
    public void TagsIn_IgnoresTextThatIsNotATag()
    {
        Assert.That(TagsIn("# @US-17 commented out\nFeature: A\n  Given mail to a@b.com\n  | @x |"), Is.Empty);
    }
}
