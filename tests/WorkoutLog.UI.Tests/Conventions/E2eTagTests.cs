using System.Text.RegularExpressions;

namespace WorkoutLog.UI.Tests.Conventions;

/// <summary>Every tag on an E2E scenario or feature is one docs/e2e-tests.md lists under Tags.</summary>
public partial class E2eTagTests
{
    [GeneratedRegex(@"`@([\w-]+)`")]
    private static partial Regex DocumentedTag();

    [GeneratedRegex(@"^\s*(@[\w-]+(\s+@[\w-]+)*)\s*$", RegexOptions.Multiline)]
    private static partial Regex TagLine();

    private static string[] Documented()
    {
        var docs = RepoFiles.Read("docs", "e2e-tests.md");
        var tagsSection = docs[docs.IndexOf("## Tags", StringComparison.Ordinal)..];
        var section = tagsSection[..tagsSection.IndexOf("\n## ", 1, StringComparison.Ordinal)];
        return DocumentedTag().Matches(section).Select(m => m.Groups[1].Value).Distinct().ToArray();
    }

    private static IEnumerable<TestCaseData> Features() =>
        RepoFiles.Find("*.feature", "tests", "WorkoutLog.E2E.Tests")
            .Select(f => new TestCaseData(f).SetArgDisplayNames(Path.GetFileName(f)));

    [Test]
    public void TagsSection_ListsTheTagsInUse() =>
        Assert.That(Documented(), Is.Not.Empty.And.Contain("smoke").And.Contain("release"));

    [TestCaseSource(nameof(Features))]
    public void Feature_UsesOnlyDocumentedTags(string file)
    {
        var used = TagLine().Matches(File.ReadAllText(file))
            .SelectMany(m => m.Groups[1].Value.Split(' ', StringSplitOptions.RemoveEmptyEntries))
            .Select(tag => tag.TrimStart('@'));

        Assert.That(used.Except(Documented()), Is.Empty, "Add the tag to docs/e2e-tests.md Tags, or drop it. Story tags such as @US-17 were dropped in #144.");
    }
}
