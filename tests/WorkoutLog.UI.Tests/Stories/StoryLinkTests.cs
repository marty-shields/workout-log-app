using System.Text.RegularExpressions;

namespace WorkoutLog.UI.Tests.Stories;

/// <summary>Every <c>./?path=</c> link in a stories page points at a story, docs page or custom page that exists.</summary>
public partial class StoryLinkTests
{
    private static readonly IReadOnlyList<string> Pages = [.. StoryFiles.All().Where(f => f.EndsWith(".md"))];

    private static readonly HashSet<string> Ids = RealIds();

    private static IEnumerable<TestCaseData> Links() =>
        Pages.SelectMany(page => Link().Matches(File.ReadAllText(page))
            .Select(m => new TestCaseData(Path.GetFileName(page), m.Groups[1].Value, m.Groups[2].Value)
                .SetArgDisplayNames(Path.GetFileName(page), m.Groups[1].Value + "/" + m.Groups[2].Value)));

    [TestCaseSource(nameof(Links))]
    public void Link_PointsAtARealStoryOrPage(string page, string kind, string id)
    {
        Assert.That(Ids, Has.Member($"{kind}/{id}"), $"{page} links to ./?path=/{kind}/{id}, which is no story, docs page or custom page");
    }

    [Test]
    public void StoriesPages_HaveLinks()
    {
        Assert.That(Links().Count(), Is.GreaterThan(50));
    }

    private static HashSet<string> RealIds()
    {
        var ids = new HashSet<string>();
        foreach (var file in StoryFiles.All())
        {
            var title = Slug(StoryFiles.Title(file));
            if (file.EndsWith(".md"))
            {
                ids.Add($"custom/{title}");
                continue;
            }

            ids.Add($"docs/{title}--docs");
            foreach (Match name in StoryName().Matches(File.ReadAllText(file)))
            {
                ids.Add($"story/{title}--{Slug(name.Groups[1].Value)}");
            }
        }

        return ids;
    }

    private static string Slug(string text) => NotAlphanumeric().Replace(text.ToLowerInvariant(), "-").Trim('-');

    [GeneratedRegex(@"\]\(\./\?path=/(story|docs|custom)/([a-z0-9-]+)(?:#[^)]*)?\)")]
    private static partial Regex Link();

    [GeneratedRegex("""<Story Name="([^"]+)">""")]
    private static partial Regex StoryName();

    [GeneratedRegex("[^a-z0-9]+")]
    private static partial Regex NotAlphanumeric();
}
