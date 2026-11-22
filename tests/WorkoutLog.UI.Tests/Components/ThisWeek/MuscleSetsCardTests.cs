using WorkoutLog.UI.Components.ThisWeek;
using WorkoutLog.UI.Models.Muscles;
using WorkoutLog.UI.Stories.Samples;

namespace WorkoutLog.UI.Tests.Components.ThisWeek;

public class MuscleSetsCardTests : ComponentTestBase
{
    [Test]
    public void Header_ShowsTitleAndThisWeek()
    {
        var cut = Render(SampleThisWeek.Muscles);

        Assert.Multiple(() =>
        {
            Assert.That(cut.Find("section.card").GetAttribute("data-testid"), Is.EqualTo("muscles"));
            Assert.That(cut.Find("section.card h2.title").TextContent, Is.EqualTo("Sets per muscle group"));
            Assert.That(cut.Find(".aside").TextContent, Is.EqualTo("this week"));
        });
    }

    [Test]
    public void SixGroups_ShowTheirSets_OrZeroAsNotTrained()
    {
        var cut = Render(SampleThisWeek.Muscles);

        var rows = cut.FindAll("li").Select(g => (g.QuerySelector(".name")!.TextContent, g.QuerySelector(".value")!.TextContent));

        Assert.That(rows, Is.EqualTo(new[]
        {
            ("Legs", "14 sets"), ("Chest", "9 sets"), ("Shoulders", "6 sets"), ("Arms", "3 sets"), ("Core", "2 sets"), ("Back", "Not trained"),
        }));
    }

    [Test]
    public void Groups_AreShownMostSetsFirst()
    {
        var cut = Render([new("Back", 0), new("Core", 2), new("Legs", 14)]);

        Assert.That(cut.FindAll(".name").Select(n => n.TextContent), Is.EqualTo(new[] { "Legs", "Core", "Back" }));
    }

    [Test]
    public void OneSet_IsSingular()
    {
        var cut = Render([new("Back", 1)]);

        Assert.That(cut.Find(".value").TextContent, Is.EqualTo("1 set"));
    }

    [Test]
    public void Bars_AreInProportionToTheBusiestGroup()
    {
        var cut = Render(SampleThisWeek.Muscles);

        var shares = cut.FindAll(".fill").Select(f => f.GetAttribute("style")!.Trim().TrimEnd(';'));

        Assert.That(shares, Is.EqualTo(new[] { "--fill: 1", "--fill: 0.6429", "--fill: 0.4286", "--fill: 0.2143", "--fill: 0.1429", "--fill: 0" }));
    }

    [Test]
    public void NoneTrained_EveryGroupIsNotTrainedWithNoBar()
    {
        var cut = Render(SampleThisWeek.NoneTrained);

        Assert.Multiple(() =>
        {
            Assert.That(cut.FindAll(".value").Select(s => s.TextContent), Is.All.EqualTo("Not trained"));
            Assert.That(cut.FindAll(".fill").Select(f => f.GetAttribute("style")!.Trim().TrimEnd(';')), Is.All.EqualTo("--fill: 0"));
        });
    }

    [Test]
    public void Bars_AreHiddenFromScreenReaders()
    {
        var cut = Render(SampleThisWeek.Muscles);

        Assert.That(cut.FindAll(".track").Select(t => t.GetAttribute("aria-hidden")), Is.All.EqualTo("true"));
    }

    [Test]
    public void Groups_HaveSlugTestIds() =>
        Assert.That(Render(SampleThisWeek.Muscles).FindAll("li").Select(g => g.GetAttribute("data-testid")),
            Is.EqualTo(new[] { "legs", "chest", "shoulders", "arms", "core", "back" }.Select(slug => $"muscles-rows-row-{slug}")));

    [Test]
    public void Groups_AreNotTappable() =>
        Assert.That(Render(SampleThisWeek.Muscles).FindAll("button"), Is.Empty);

    [Test]
    public void NoGroups_ShowsTheMessage() =>
        Assert.That(Render([]).Find("[data-testid=muscles-rows-empty]").TextContent, Is.EqualTo("No sets yet"));

    [Test]
    public void Aside_SaysThisWeekUnlessGivenAPeriod()
    {
        var withPeriod = Context.Render<MuscleSetsCard>(p => p.Add(x => x.Groups, SampleThisWeek.Muscles).Add(x => x.Period, "that month"));

        Assert.Multiple(() =>
        {
            Assert.That(Render(SampleThisWeek.Muscles).Find(".aside").TextContent, Is.EqualTo("this week"));
            Assert.That(withPeriod.Find(".aside").TextContent, Is.EqualTo("that month"));
        });
    }

    private IRenderedComponent<MuscleSetsCard> Render(IReadOnlyList<MuscleGroupSets> groups) =>
        Context.Render<MuscleSetsCard>(p => p.Add(x => x.Groups, groups).Add(x => x.TestId, "muscles"));
}
