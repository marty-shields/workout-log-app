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
            Assert.That(cut.Find(".title").TextContent, Is.EqualTo("Sets per muscle group"));
            Assert.That(cut.Find(".aside").TextContent, Is.EqualTo("this week"));
        });
    }

    [Test]
    public void SixGroups_ShowTheirSets_OrZeroAsNotTrained()
    {
        var cut = Render(SampleThisWeek.Muscles);

        var rows = cut.FindAll(".group").Select(g => (g.QuerySelector(".name")!.TextContent, g.QuerySelector(".sets")!.TextContent));

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

        Assert.That(cut.Find(".sets").TextContent, Is.EqualTo("1 set"));
    }

    [Test]
    public void Bars_AreInProportionToTheBusiestGroup()
    {
        var cut = Render(SampleThisWeek.Muscles);

        var shares = cut.FindAll(".fill").Select(f => f.GetAttribute("style")!.Trim().TrimEnd(';'));

        Assert.That(shares, Is.EqualTo(new[] { "--share: 1", "--share: 0.643", "--share: 0.429", "--share: 0.214", "--share: 0.143", "--share: 0" }));
    }

    [Test]
    public void NoneTrained_EveryGroupIsNotTrainedWithNoBar()
    {
        var cut = Render(SampleThisWeek.NoneTrained);

        Assert.Multiple(() =>
        {
            Assert.That(cut.FindAll(".sets").Select(s => s.TextContent), Is.All.EqualTo("Not trained"));
            Assert.That(cut.FindAll(".fill").Select(f => f.GetAttribute("style")!.Trim().TrimEnd(';')), Is.All.EqualTo("--share: 0"));
        });
    }

    [Test]
    public void Bars_AreHiddenFromScreenReaders()
    {
        var cut = Render(SampleThisWeek.Muscles);

        Assert.That(cut.FindAll(".track").Select(t => t.GetAttribute("aria-hidden")), Is.All.EqualTo("true"));
    }

    [Test]
    public void Groups_HaveTestIds() =>
        Assert.That(Render(SampleThisWeek.Muscles).FindAll(".group").Select(g => g.GetAttribute("data-testid")),
            Is.EqualTo(new[] { "muscles-legs", "muscles-chest", "muscles-shoulders", "muscles-arms", "muscles-core", "muscles-back" }));

    private IRenderedComponent<MuscleSetsCard> Render(IReadOnlyList<MuscleGroupSets> groups) =>
        Context.Render<MuscleSetsCard>(p => p.Add(x => x.Groups, groups).Add(x => x.TestId, "muscles"));
}
