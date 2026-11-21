using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Components;
using Shields.UI.Testing;

namespace WorkoutLog.UI.Tests.DesignSystem;

/// <summary>
/// A general component or token belongs in the Shields.UI kit (app-ui-kit), not in this app. These tests fail when the app declares
/// a component or a custom property under the same name as one the kit has, so a local copy cannot creep back in.
/// </summary>
public partial class KitDuplicateTests
{
    private static readonly string[] AppProjects = ["WorkoutLog.UI", "WorkoutLog.App"];

    [Test]
    public void AppComponents_DoNotShareANameWithAKitComponent()
    {
        Assert.That(Shared(AppComponentNames(), KitComponents()), Is.Empty, "move the component to app-ui-kit or rename it");
    }

    [Test]
    public void AppComponentNames_CoverBothProjectsAndBothKindsOfComponent()
    {
        Assert.That(AppComponentNames(), Is.SupersetOf(new[] { "Chart", "Routes", "SystemBarInsets" }));
    }

    [Test]
    public void AppTokens_DoNotShareANameWithAKitToken()
    {
        Assert.That(Shared(AppTokens.Names, DesignTokens.Names), Is.Empty, "use the kit's token or give this one an app-specific name");
    }

    [Test]
    public void KitComponents_ListsTheKitsComponentsFromItsAssembly()
    {
        Assert.That(KitComponents(), Is.SupersetOf(KitNames.Components));
    }

    [Test]
    public void Shared_ReturnsNamesInBothSets()
    {
        Assert.That(Shared(["Spinner", "TrendBars"], ["Spinner", "Icon"]), Is.EqualTo(new[] { "Spinner" }));
    }

    [Test]
    public void Shared_IsEmpty_WhenNoNameIsInBoth()
    {
        Assert.That(Shared(["TrendBars"], ["Spinner", "Icon"]), Is.Empty);
    }

    [Test]
    public void ComponentNames_AreTheFileNamesWithoutTheExtensionAndFolder()
    {
        Assert.That(
            ComponentNames(["/src/WorkoutLog.UI/Components/Spinner.razor", "/src/WorkoutLog.UI/Components/Lists/Row.razor"]),
            Is.EqualTo(new[] { "Spinner", "Row" }));
    }

    [Test]
    public void TypeName_DropsTheGenericArity()
    {
        Assert.That(TypeName(typeof(Shields.UI.Components.Inputs.SegmentedControl<>)), Is.EqualTo("SegmentedControl"));
    }

    private static string[] Shared(IEnumerable<string> app, IEnumerable<string> kit) =>
        app.Intersect(kit).Order().ToArray();

    // A *.razor file, or a C# class deriving ComponentBase, in either project.
    private static IReadOnlySet<string> AppComponentNames() =>
        ComponentNames(AppProjects.SelectMany(project => RepoFiles.Find("*.razor", "src", project)))
            .Concat(AppProjects.SelectMany(project => RepoFiles.Find("*.cs", "src", project))
                .SelectMany(file => ComponentClass().Matches(File.ReadAllText(file)).Select(m => m.Groups[1].Value)))
            .ToHashSet();

    private static IEnumerable<string> ComponentNames(IEnumerable<string> razorFiles) =>
        razorFiles.Select(Path.GetFileNameWithoutExtension)!;

    private static IReadOnlySet<string> KitComponents() =>
        typeof(Shields.UI.Components.Inputs.SegmentedControl<>).Assembly.GetTypes()
            .Where(type => type is { IsAbstract: false, IsPublic: true } && type.IsAssignableTo(typeof(ComponentBase)))
            .Select(TypeName)
            .ToHashSet();

    private static string TypeName(Type type) => type.Name.Split('`')[0];

    [GeneratedRegex(@"\bclass\s+(\w+)\s*:\s*ComponentBase\b")]
    private static partial Regex ComponentClass();
}
