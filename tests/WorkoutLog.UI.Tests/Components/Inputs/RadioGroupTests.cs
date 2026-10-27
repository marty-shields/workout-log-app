using WorkoutLog.UI.Components.Inputs;

namespace WorkoutLog.UI.Tests.Components.Inputs;

public class RadioGroupTests : ComponentTestBase
{
    private static readonly IReadOnlyList<RadioOption<int>> Options =
    [
        new(1, "One", "one"),
        new(2, "Two", "two", "The second"),
    ];

    [Test]
    public void Renders_TheLabelAndEachOptionWithItsHintAndTestId()
    {
        var cut = Render(value: 1);

        Assert.Multiple(() =>
        {
            Assert.That(cut.Find("fieldset").GetAttribute("data-testid"), Is.EqualTo("group"));
            Assert.That(cut.Find("legend").TextContent, Is.EqualTo("Choose"));
            Assert.That(cut.FindAll(".text").Select(t => t.TextContent), Is.EqualTo(new[] { "One", "Two" }));
            Assert.That(cut.FindAll(".hint").Select(t => t.TextContent), Is.EqualTo(new[] { "The second" }));
            Assert.That(cut.FindAll("input[type=radio]").Select(i => i.GetAttribute("data-testid")), Is.EqualTo(new[] { "one", "two" }));
        });
    }

    [Test]
    public void Value_ChecksItsOptionOnly()
    {
        var cut = Render(value: 2);

        Assert.Multiple(() =>
        {
            Assert.That(cut.Find("[data-testid=one]").HasAttribute("checked"), Is.False);
            Assert.That(cut.Find("[data-testid=two]").HasAttribute("checked"), Is.True);
        });
    }

    [Test]
    public void Options_ShareOneNamePerGroup()
    {
        var first = Render(value: 1);
        var second = Render(value: 1);

        var names = first.FindAll("input").Select(i => i.GetAttribute("name")).Distinct().ToList();
        Assert.Multiple(() =>
        {
            Assert.That(names, Has.Count.EqualTo(1));
            Assert.That(second.Find("input").GetAttribute("name"), Is.Not.EqualTo(names[0]));
        });
    }

    [Test]
    public void TappingAnOption_RaisesValueChangedWithItsValue()
    {
        var changed = new List<int>();
        var cut = Context.Render<RadioGroup<int>>(p => p
            .Add(x => x.Label, "Choose")
            .Add(x => x.Options, Options)
            .Add(x => x.Value, 1)
            .Add(x => x.ValueChanged, (int v) => changed.Add(v)));

        cut.Find("[data-testid=two]").Change(true);

        Assert.That(changed, Is.EqualTo(new[] { 2 }));
    }

    [Test]
    public void Disabled_DisablesTheGroup()
    {
        var cut = Context.Render<RadioGroup<int>>(p => p.Add(x => x.Label, "Choose").Add(x => x.Options, Options).Add(x => x.Disabled, true));

        Assert.That(cut.Find("fieldset").HasAttribute("disabled"), Is.True);
    }

    private IRenderedComponent<RadioGroup<int>> Render(int value) =>
        Context.Render<RadioGroup<int>>(p => p
            .Add(x => x.Label, "Choose")
            .Add(x => x.Options, Options)
            .Add(x => x.Value, value)
            .Add(x => x.ValueChanged, (int _) => { })
            .Add(x => x.TestId, "group"));
}
