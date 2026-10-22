using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Components;
using WorkoutLog.UI.Screens;

namespace WorkoutLog.UI.Tests.DesignSystem;

public partial class DesignSystemPageTests
{
    private static readonly Type[] UiTypes = typeof(DashboardView).Assembly.GetExportedTypes();

    private static IEnumerable<TestCaseData> Components() =>
        UiTypes
            .Where(t => typeof(IComponent).IsAssignableFrom(t) && t.Namespace!.StartsWith("WorkoutLog.UI.Components."))
            .Select(t => new TestCaseData(t.Namespace!.Split('.')[^1], t.Name).SetArgDisplayNames(t.Name));

    private static IEnumerable<TestCaseData> Views() =>
        UiTypes
            .Where(t => typeof(IComponent).IsAssignableFrom(t) && t.Namespace == "WorkoutLog.UI.Screens")
            .Select(t => new TestCaseData(t.Name).SetArgDisplayNames(t.Name));

    [TestCaseSource(nameof(Components))]
    public void Component_HasASectionOnItsGroupPage(string group, string name)
    {
        var page = Page("Components", $"{group}.md");

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
        var names = UiTypes.Select(t => t.Name).ToHashSet();
        var pages = Pages("Components").Concat(Pages("Screens")).ToList();

        var named = pages.SelectMany(Headings)
            .Concat(pages.SelectMany(page => Code().Matches(page).Select(m => m.Groups[1].Value)).Where(n => n.EndsWith("View")));

        Assert.That(named.Where(n => !names.Contains(n)), Is.Empty);
    }

    private static string Folder(string folder) => RepoFiles.PathOf("src", "WorkoutLog.UI.Stories", "Stories", "DesignSystem", folder);

    private static string Page(string folder, string file) => File.ReadAllText(Path.Combine(Folder(folder), file));

    private static IEnumerable<string> Pages(string folder) => Directory.EnumerateFiles(Folder(folder), "*.md").Select(File.ReadAllText);

    private static IEnumerable<string> Headings(string page) => Heading().Matches(page).Select(m => m.Groups[1].Value);

    [GeneratedRegex(@"^## `([A-Za-z]+)`", RegexOptions.Multiline)]
    private static partial Regex Heading();

    [GeneratedRegex("`([A-Za-z]+)`")]
    private static partial Regex Code();
}
