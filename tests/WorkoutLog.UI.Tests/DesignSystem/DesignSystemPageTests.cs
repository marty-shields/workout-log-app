using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Components;
using WorkoutLog.UI.Screens;

namespace WorkoutLog.UI.Tests.DesignSystem;

public partial class DesignSystemPageTests
{
    private static readonly Type[] UiTypes = typeof(ThisWeekView).Assembly.GetExportedTypes();

    private static IEnumerable<TestCaseData> Components() =>
        UiTypes
            .Where(t => typeof(IComponent).IsAssignableFrom(t) && t.Namespace!.StartsWith("WorkoutLog.UI.Components."))
            .Select(t => (Group: t.Namespace!.Split('.')[^1], Name: t.Name.Split('`')[0]))
            .Select(c => new TestCaseData(c.Group, c.Name).SetArgDisplayNames(c.Name));

    private static IEnumerable<TestCaseData> Views() =>
        UiTypes
            .Where(t => typeof(IComponent).IsAssignableFrom(t) && t.Namespace == "WorkoutLog.UI.Screens")
            .Select(t => new TestCaseData(t.Name).SetArgDisplayNames(t.Name));

    [TestCaseSource(nameof(Components))]
    public void Component_HasASectionOnItsGroupPage(string group, string name)
    {
        var page = File.ReadAllText(Path.Combine(Folder("Components"), group, $"{group}.md"));

        Assert.That(Headings(page), Has.Member(name));
    }

    [TestCaseSource(nameof(Views))]
    public void View_IsNamedOnAScreenPage(string name)
    {
        var named = Pages("Screens").SelectMany(page => Code().Matches(page).Select(m => m.Groups[1].Value));

        Assert.That(named, Has.Member(name));
    }

    [Test]
    public void Pages_NameOnlyComponentsAndViewsThatExist()
    {
        var names = UiTypes.Select(t => t.Name.Split('`')[0]).ToHashSet();
        var pages = Pages("Components").Concat(Pages("Screens")).ToList();

        var named = pages.SelectMany(Headings)
            .Concat(pages.SelectMany(page => Code().Matches(page).Select(m => m.Groups[1].Value)).Where(n => n.EndsWith("View")));

        Assert.That(named.Where(n => !names.Contains(n)), Is.Empty);
    }

    [Test]
    public void TokensStory_ShowsEveryTokenTheAppDeclaresAfterTheKits()
    {
        var story = File.ReadAllText(Path.Combine(Folder("Foundations"), "Tokens.stories.razor"));

        var listed = Regex.Matches(story, "\"(--[a-z0-9-]+)\"").Select(m => m.Groups[1].Value);

        Assert.That(listed, Is.EquivalentTo(AppTokens.Names));
    }

    [Test]
    public void TokensStory_ListsColoursAndSizesInTheirOwnLists()
    {
        var story = File.ReadAllText(Path.Combine(Folder("Foundations"), "Tokens.stories.razor"));

        var colours = Regex.Match(story, @"AppColours\s*=\s*\[(.*?)\];", RegexOptions.Singleline).Groups[1].Value;
        var sizes = Regex.Match(story, @"AppSizes\s*=\s*\[(.*?)\];", RegexOptions.Singleline).Groups[1].Value;

        Assert.Multiple(() =>
        {
            Assert.That(Names(colours), Is.Not.Empty.And.All.StartsWith("--color-"));
            Assert.That(Names(sizes), Is.Not.Empty.And.All.Not.StartsWith("--color-"));
        });
    }

    [Test]
    public void FoundationsPages_AreTheAppsOwn()
    {
        var pages = Directory.EnumerateFiles(Folder("Foundations")).Select(f => Path.GetFileName(f).Split('.')[0]).Distinct();

        Assert.That(pages, Is.EquivalentTo(new[] { "Introduction", "Haptics", "Navigation", "AppIcon", "Tokens" }));
    }

    private static IEnumerable<string> Names(string list) => Regex.Matches(list, "\"(--[a-z0-9-]+)\"").Select(m => m.Groups[1].Value);

    private static string Folder(string folder) => RepoFiles.PathOf("src", "WorkoutLog.UI.Stories", "Stories", folder);

    private static IEnumerable<string> Pages(string folder) =>
        Directory.EnumerateFiles(Folder(folder), "*.md", SearchOption.AllDirectories).Select(File.ReadAllText);

    private static IEnumerable<string> Headings(string page) => Heading().Matches(page).Select(m => m.Groups[1].Value);

    [GeneratedRegex(@"^## `([A-Za-z]+)`", RegexOptions.Multiline)]
    private static partial Regex Heading();

    [GeneratedRegex("`([A-Za-z]+)`")]
    private static partial Regex Code();
}
