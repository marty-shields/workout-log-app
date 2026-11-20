using Shields.UI.Components.Actions;
using WorkoutLog.UI.Components.ThisWeek;
using WorkoutLog.UI.Components.Weekly;
using WorkoutLog.UI.Stories.Samples;

namespace WorkoutLog.UI.Tests.Components.ThisWeek;

public class WeekTilesTests : ComponentTestBase
{
    [Test]
    public void Tiles_AreWorkoutsTimeAndSetsWithTheirValues()
    {
        var cut = Render(SampleThisWeek.ThisWeek, SampleThisWeek.LastWeekToDate, DayOfWeek.Wednesday);

        var tiles = cut.FindAll(".tile").Select(t => (t.QuerySelector(".label")!.TextContent, t.QuerySelector(".value")!.TextContent));

        Assert.That(tiles, Is.EqualTo(new[] { ("Workouts", "3"), ("Time", "2h 35m"), ("Sets", "34") }));
    }

    [Test]
    public void Increase_ShowsTheUpIconInSuccessAndTheDifference()
    {
        var cut = Render(SampleThisWeek.ThisWeek, SampleThisWeek.LastWeekToDate, DayOfWeek.Wednesday);

        var change = cut.Find("[data-testid=tiles-time-change]");
        var icon = cut.FindComponents<Icon>().First(i => i.Instance.Name == IconName.Up).Instance;

        Assert.Multiple(() =>
        {
            Assert.That(TestText.Normalise(change.TextContent), Is.EqualTo("up 35m"));
            Assert.That(change.QuerySelector(".diff")!.ClassList, Does.Contain("up"));
            Assert.That(icon.Tone, Is.EqualTo(IconTone.Success));
        });
    }

    [Test]
    public void Decrease_ShowsTheDownIconInErrorAndTheDifference()
    {
        var cut = Render(SampleThisWeek.LastWeekToDate, SampleThisWeek.ThisWeek, DayOfWeek.Wednesday);

        var change = cut.Find("[data-testid=tiles-sets-change]");
        var icon = cut.FindComponents<Icon>().First(i => i.Instance.Name == IconName.Down).Instance;

        Assert.Multiple(() =>
        {
            Assert.That(TestText.Normalise(change.TextContent), Is.EqualTo("down 1"));
            Assert.That(change.QuerySelector(".diff")!.ClassList, Does.Contain("down"));
            Assert.That(icon.Tone, Is.EqualTo(IconTone.Error));
        });
    }

    [Test]
    public void NoChange_ShowsADashAndNoIcon()
    {
        var cut = Render(SampleThisWeek.ThisWeek, SampleThisWeek.ThisWeek, DayOfWeek.Wednesday);

        Assert.Multiple(() =>
        {
            Assert.That(cut.FindAll(".change").Select(c => TestText.Normalise(c.TextContent)), Is.All.EqualTo("—no change"));
            Assert.That(cut.FindAll("svg"), Is.Empty);
            Assert.That(cut.FindAll(".same"), Has.Count.EqualTo(3));
        });
    }

    [Test]
    public void TimeIsComparedInWholeMinutes()
    {
        var cut = Render(new WeekTotals(1, new TimeSpan(0, 10, 59), 1), new WeekTotals(1, new TimeSpan(0, 10, 1), 1), DayOfWeek.Tuesday);

        Assert.That(TestText.Normalise(cut.Find("[data-testid=tiles-time-change]").TextContent), Is.EqualTo("—no change"));
    }

    [TestCase(DayOfWeek.Wednesday, "Against Monday to Wednesday last week")]
    [TestCase(DayOfWeek.Sunday, "Against Monday to Sunday last week")]
    [TestCase(DayOfWeek.Monday, "Against Monday last week")]
    public void UnderTheTiles_SaysWhatTheyAreComparedWith(DayOfWeek today, string expected)
    {
        var cut = Render(SampleThisWeek.Nothing, SampleThisWeek.Nothing, today);

        Assert.That(cut.Find("[data-testid=tiles-against]").TextContent, Is.EqualTo(expected));
    }

    [Test]
    public void NothingLogged_ShowsZeros()
    {
        var cut = Render(SampleThisWeek.Nothing, SampleThisWeek.Nothing, DayOfWeek.Monday);

        Assert.That(cut.FindAll(".value").Select(v => v.TextContent), Is.EqualTo(new[] { "0", "0m", "0" }));
    }

    private IRenderedComponent<WeekTiles> Render(WeekTotals thisWeek, WeekTotals last, DayOfWeek today) =>
        Context.Render<WeekTiles>(p => p
            .Add(x => x.ThisWeek, thisWeek)
            .Add(x => x.LastWeekToDate, last)
            .Add(x => x.Today, today)
            .Add(x => x.TestId, "tiles"));
}
