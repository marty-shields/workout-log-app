using WorkoutLog.UI.Components.Workout;

namespace WorkoutLog.UI.Tests.Components.Workout;

public class LastNoteTests : ComponentTestBase
{
    private IRenderedComponent<LastNote> Render(string text, bool expanded = false) =>
        Context.Render<LastNote>(p => p
            .Add(x => x.Text, text)
            .Add(x => x.Expanded, expanded)
            .Add(x => x.TestId, "last-note"));

    [Test]
    public void ShowsTheNoteIconAndTheText()
    {
        var cut = Render("Belt on from set 3");

        var line = cut.Find("[data-testid=last-note]");
        Assert.Multiple(() =>
        {
            Assert.That(line.QuerySelectorAll("svg.icon.muted.small"), Has.Count.EqualTo(1));
            Assert.That(line.QuerySelector(".text")!.TextContent, Is.EqualTo("Belt on from set 3"));
            Assert.That(TestText.Normalise(line.TextContent), Is.EqualTo("Last note: Belt on from set 3"));
        });
    }

    [Test]
    public void Collapsed_IsOneLine()
    {
        var cut = Render("Belt on from set 3");

        var line = cut.Find("[data-testid=last-note]");
        Assert.Multiple(() =>
        {
            Assert.That(line.ClassList, Does.Not.Contain("open"));
            Assert.That(line.GetAttribute("aria-expanded"), Is.EqualTo("false"));
        });
    }

    [Test]
    public void Expanded_ShowsTheWholeNote()
    {
        var cut = Render("Belt on from set 3", expanded: true);

        var line = cut.Find("[data-testid=last-note]");
        Assert.Multiple(() =>
        {
            Assert.That(line.ClassList, Does.Contain("open"));
            Assert.That(line.GetAttribute("aria-expanded"), Is.EqualTo("true"));
        });
    }

    [Test]
    public void Tap_TogglesBetweenOneLineAndTheWholeNote()
    {
        var cut = Render("Belt on from set 3");

        cut.Find("[data-testid=last-note]").Click();
        var expanded = cut.Find("[data-testid=last-note]").ClassList.Contains("open");
        cut.Find("[data-testid=last-note]").Click();

        Assert.Multiple(() =>
        {
            Assert.That(expanded, Is.True);
            Assert.That(cut.Find("[data-testid=last-note]").ClassList, Does.Not.Contain("open"));
        });
    }

    [Test]
    public void LongNote_KeepsTheWholeTextInTheMarkup()
    {
        var text = new string('a', 400);

        var cut = Render(text);

        Assert.That(cut.Find(".text").TextContent, Is.EqualTo(text));
    }
}
