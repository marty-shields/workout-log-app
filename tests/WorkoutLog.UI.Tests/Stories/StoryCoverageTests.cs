using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Components;
using WorkoutLog.UI.Components.Workout;
using WorkoutLog.UI.Screens;

namespace WorkoutLog.UI.Tests.Stories;

public partial class StoryCoverageTests
{
    private static readonly Type[] UiTypes = typeof(DashboardView).Assembly.GetExportedTypes();

    /// <summary>Components shown by a screen's stories instead of their own.</summary>
    private static readonly string[] ShownOnScreens = [nameof(ExercisePicker)];

    private static IEnumerable<TestCaseData> Components() =>
        UiTypes
            .Where(t => typeof(IComponent).IsAssignableFrom(t) && t.Namespace!.StartsWith("WorkoutLog.UI.Components.") && !ShownOnScreens.Contains(t.Name))
            .Select(t => new TestCaseData(t.Namespace!.Split('.')[^1], t.Name).SetArgDisplayNames(t.Name));

    private static IEnumerable<TestCaseData> Views() =>
        UiTypes
            .Where(t => typeof(IComponent).IsAssignableFrom(t) && t.Namespace == "WorkoutLog.UI.Screens" && t.Name.EndsWith("View"))
            .Select(t => new TestCaseData(t.Name).SetArgDisplayNames(t.Name));

    [TestCaseSource(nameof(Components))]
    public void Component_HasOneStoriesFileInItsGroup(string group, string name)
    {
        var titles = StoriesFiles("Components").Where(f => f.Component == name).Select(f => f.Title);

        Assert.That(titles, Is.EqualTo(new[] { $"Components/{group}/{name}" }));
    }

    [Test]
    public void ComponentStories_ShowOnlyTheirComponent()
    {
        var shown = StoriesFiles("Components").SelectMany(f => f.Shown.Where(s => s != f.Component).Select(s => $"{f.Title}: {s}"));

        Assert.That(shown, Is.Empty);
    }

    [Test]
    public void ComponentStories_AreForComponentsOnly()
    {
        var names = Components().Select(c => (string)c.Arguments[1]!).ToHashSet();

        Assert.That(StoriesFiles("Components").Select(f => f.Component).Where(c => !names.Contains(c)), Is.Empty);
    }

    [TestCaseSource(nameof(Views))]
    public void View_IsShownByAScreenStory(string name)
    {
        Assert.That(StoriesFiles("Screens").SelectMany(f => f.Shown), Has.Member(name));
    }

    [Test]
    public void ScreenStories_ShowOnlyViews()
    {
        var views = Views().Select(v => (string)v.Arguments[0]!).ToHashSet();

        Assert.That(StoriesFiles("Screens").SelectMany(f => f.Shown).Where(s => !views.Contains(s)), Is.Empty);
    }

    [Test]
    public void Stories_AreInFoundationsComponentsOrScreens()
    {
        var titles = StoriesFiles("").Select(f => f.Title.Split('/')[0]).Distinct();

        Assert.That(titles, Is.SubsetOf(new[] { "Foundations", "Components", "Screens" }));
    }

    /// <summary>The stories files in a folder under Stories: title, TComponent and the component each story's template starts with.</summary>
    private static IEnumerable<(string Title, string Component, IReadOnlyList<string> Shown)> StoriesFiles(string folder) =>
        RepoFiles.Find("*.stories.razor", "src", "WorkoutLog.UI.Stories", "Stories", folder)
            .Select(File.ReadAllText)
            .Select(source => (
                StoriesTitle().Match(source).Groups[1].Value,
                TComponent().Match(source).Groups[1].Value,
                (IReadOnlyList<string>)Shown().Matches(source).Select(m => m.Groups[1].Value).ToList()));

    [GeneratedRegex("""\[Stories\("([^"]+)"\)\]""")]
    private static partial Regex StoriesTitle();

    [GeneratedRegex(@"<Stories TComponent=""([A-Za-z]+)""")]
    private static partial Regex TComponent();

    [GeneratedRegex(@"<Template>\s*(?:@\{[^\n]*\}\s*)?<([A-Za-z]+)")]
    private static partial Regex Shown();
}
