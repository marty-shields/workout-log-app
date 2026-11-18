using WorkoutLog.UI.Components.Trends;
using WorkoutLog.UI.Stories.Samples;

namespace WorkoutLog.UI.Tests.Components.Trends;

public class MeterRowsTests : ComponentTestBase
{
    [Test]
    public void Rows_KeepTheOrderGivenWithTheirNumbers()
    {
        var cut = Render(SampleTrends.MostTrained, wide: true);

        Assert.That(cut.FindAll(".row").Select(r => (r.QuerySelector(".name")!.TextContent, r.QuerySelector(".count")!.FirstChild!.TextContent)),
            Is.EqualTo(new[] { ("Bench press", "5"), ("Squat", "4"), ("Deadlift", "3"), ("Treadmill run", "3"), ("Overhead press", "3") }));
    }

    [Test]
    public void Bars_AreAsLongAsTheirShareOfTheBiggest()
    {
        var cut = Render(SampleTrends.MostTrained, wide: true);

        Assert.That(cut.FindAll(".fill").Select(f => f.GetAttribute("style")!.Trim().TrimEnd(';')),
            Is.EqualTo(new[] { "--share: 1", "--share: 0.8", "--share: 0.6", "--share: 0.6", "--share: 0.6" }));
    }

    [Test]
    public void Numbers_AreFollowedByTheirUnitForScreenReaders()
    {
        var cut = Render(SampleTrends.MostTrained, wide: true);

        Assert.Multiple(() =>
        {
            Assert.That(TestText.Normalise(cut.Find(".count").TextContent), Is.EqualTo("5 sessions"));
            Assert.That(cut.Find(".track").GetAttribute("aria-hidden"), Is.EqualTo("true"));
        });
    }

    [Test]
    public void Wide_WidensTheNameColumn()
    {
        Assert.Multiple(() =>
        {
            Assert.That(Render(SampleTrends.MostTrained, wide: true).Find("ul").ClassList, Does.Contain("wide"));
            Assert.That(Render(SampleTrends.MostTrained, wide: false).Find("ul").ClassList, Does.Not.Contain("wide"));
        });
    }

    [Test]
    public void ARowAtZero_HasNoBar() =>
        Assert.That(Render([new("Jan", 4), new("Feb", 0)], wide: false).FindAll(".fill")[1].GetAttribute("style")!.Trim().TrimEnd(';'), Is.EqualTo("--share: 0"));

    [Test]
    public void Rows_HaveTestIdsFromTheirLabels() =>
        Assert.That(Render(SampleTrends.MostTrained, wide: true).FindAll(".row").Select(r => r.GetAttribute("data-testid")),
            Is.EqualTo(new[] { "meters-bench-press", "meters-squat", "meters-deadlift", "meters-treadmill-run", "meters-overhead-press" }));

    [Test]
    public void NoRows_ShowsTheMessage()
    {
        var cut = Render([], wide: true);

        Assert.Multiple(() =>
        {
            Assert.That(cut.Find("[data-testid=meters-none]").TextContent, Is.EqualTo("Nothing yet"));
            Assert.That(cut.FindAll("li"), Is.Empty);
        });
    }

    [Test]
    public void AllRowsAtZero_ShowTheMessage() =>
        Assert.That(Render([new("Jan", 0), new("Feb", 0)], wide: false).FindAll("[data-testid=meters-none]"), Has.Count.EqualTo(1));

    private IRenderedComponent<MeterRows> Render(IReadOnlyList<MeterRow> rows, bool wide) =>
        Context.Render<MeterRows>(p => p
            .Add(x => x.Rows, rows)
            .Add(x => x.Unit, "sessions")
            .Add(x => x.EmptyText, "Nothing yet")
            .Add(x => x.Wide, wide)
            .Add(x => x.TestId, "meters"));
}
