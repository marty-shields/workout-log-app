using Microsoft.AspNetCore.Components;
using Shields.UI.Testing;

namespace WorkoutLog.UI.Tests.DesignSystem;

/// <summary>
/// A general component belongs in the Shields.UI kit (app-ui-kit), not in this app. These tests fail when the app declares
/// a component under the same name as one the kit has, so a local copy cannot creep back in. <see cref="NoAppTokensTests"/> covers tokens.
/// </summary>
public class KitDuplicateTests
{
    [Test]
    public void AppComponents_DoNotShareANameWithAKitComponent()
    {
        Assert.That(Shared(AppComponents.Names(), KitComponents()), Is.Empty, "move the component to app-ui-kit or rename it");
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
    public void TypeName_DropsTheGenericArity()
    {
        Assert.That(TypeName(typeof(Shields.UI.Components.Inputs.SegmentedControl<>)), Is.EqualTo("SegmentedControl"));
    }

    private static string[] Shared(IEnumerable<string> app, IEnumerable<string> kit) =>
        app.Intersect(kit).Order().ToArray();

    private static IReadOnlySet<string> KitComponents() =>
        typeof(Shields.UI.Components.Inputs.SegmentedControl<>).Assembly.GetTypes()
            .Where(type => type is { IsAbstract: false, IsPublic: true } && type.IsAssignableTo(typeof(ComponentBase)))
            .Select(TypeName)
            .ToHashSet();

    private static string TypeName(Type type) => type.Name.Split('`')[0];
}
