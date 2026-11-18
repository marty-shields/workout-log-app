using WorkoutLog.Core.Domain.Workouts.Periods;
using WorkoutLog.UI.Components.Actions;
using WorkoutLog.UI.Components.Trends;
using WorkoutLog.UI.Stories.Samples;

namespace WorkoutLog.UI.Tests.Components.Trends;

public class TrendTilesTests : ComponentTestBase
{
    [Test]
    public void Tiles_AreWorkoutsTimeSetsAndVolumeInTwoColumns()
    {
        var cut = Render(PeriodKind.Week, SampleTrends.WeekTotals, SampleTrends.WeekEarlier);

        var tiles = cut.FindAll(".tile").Select(t => (t.QuerySelector(".label")!.TextContent, t.QuerySelector(".value")!.TextContent));

        Assert.That(tiles, Is.EqualTo(new[] { ("Workouts", "3"), ("Time", "2h 35m"), ("Sets", "34"), ("Volume", "9,840 kg") }));
    }

    [Test]
    public void Increase_ShowsUpInSuccessAndTheDifference()
    {
        var cut = Render(PeriodKind.Week, SampleTrends.WeekTotals, SampleTrends.WeekEarlier);

        Assert.Multiple(() =>
        {
            Assert.That(TestText.Normalise(cut.Find("[data-testid=tiles-workouts-change]").TextContent), Is.EqualTo("up 1"));
            Assert.That(TestText.Normalise(cut.Find("[data-testid=tiles-time-change]").TextContent), Is.EqualTo("up 35m"));
            Assert.That(TestText.Normalise(cut.Find("[data-testid=tiles-sets-change]").TextContent), Is.EqualTo("up 1"));
            Assert.That(cut.FindComponents<Icon>().Where(i => i.Instance.Name == IconName.Up).Select(i => i.Instance.Tone), Is.All.EqualTo(IconTone.Success));
        });
    }

    [Test]
    public void Decrease_ShowsDownInErrorAndTheDifference()
    {
        var cut = Render(PeriodKind.Week, SampleTrends.WeekEarlier, SampleTrends.WeekTotals);

        Assert.Multiple(() =>
        {
            Assert.That(TestText.Normalise(cut.Find("[data-testid=tiles-time-change]").TextContent), Is.EqualTo("down 35m"));
            Assert.That(TestText.Normalise(cut.Find("[data-testid=tiles-volume-change]").TextContent), Does.StartWith("down 6%"));
            Assert.That(cut.FindComponents<Icon>().Where(i => i.Instance.Name == IconName.Down).Select(i => i.Instance.Tone), Is.All.EqualTo(IconTone.Error));
        });
    }

    [Test]
    public void Volume_ChangesByPercentAndCarriesTheWordStrength()
    {
        var cut = Render(PeriodKind.Week, SampleTrends.WeekTotals, SampleTrends.WeekEarlier);

        Assert.That(TestText.Normalise(cut.Find("[data-testid=tiles-volume-change]").TextContent), Is.EqualTo("up 6% strength"));
    }

    [Test]
    public void Year_ShowsVolumeInTonnes()
    {
        var cut = Render(PeriodKind.Year, SampleTrends.YearTotals, SampleTrends.YearEarlier);

        Assert.Multiple(() =>
        {
            Assert.That(cut.Find("[data-testid=tiles-volume] .value").TextContent, Is.EqualTo("548 t"));
            Assert.That(TestText.Normalise(cut.Find("[data-testid=tiles-volume-change]").TextContent), Is.EqualTo("up 9% strength"));
        });
    }

    [Test]
    public void NothingBefore_ShowsTheVolumeGainedInPlaceOfAPercentage()
    {
        var cut = Render(PeriodKind.Month, SampleTrends.MonthTotals, SampleTrends.Nothing);

        Assert.That(TestText.Normalise(cut.Find("[data-testid=tiles-volume-change]").TextContent), Is.EqualTo("up 41,200 kg strength"));
    }

    [Test]
    public void NoChange_ShowsDashesAndNoArrows()
    {
        var cut = Render(PeriodKind.Week, SampleTrends.WeekTotals, SampleTrends.WeekTotals);

        Assert.Multiple(() =>
        {
            Assert.That(cut.FindAll(".same"), Has.Count.EqualTo(4));
            Assert.That(cut.FindAll("svg"), Is.Empty);
            Assert.That(TestText.Normalise(cut.Find("[data-testid=tiles-volume-change]").TextContent), Is.EqualTo("—no change strength"));
        });
    }

    [Test]
    public void VolumeChangeThatRoundsToAWholePercentOfNothing_ShowsNoArrow()
    {
        var cut = Render(PeriodKind.Week, new TrendTotals(1, TimeSpan.Zero, 1, 10_040), new TrendTotals(1, TimeSpan.Zero, 1, 10_000));

        Assert.Multiple(() =>
        {
            Assert.That(cut.FindAll("[data-testid=tiles-volume-change] .same"), Has.Count.EqualTo(1));
            Assert.That(cut.FindAll("[data-testid=tiles-volume-change] svg"), Is.Empty);
        });
    }

    [Test]
    public void VolumeGainedThatRoundsToZeroTonnes_ShowsNoArrow()
    {
        var cut = Render(PeriodKind.Year, new TrendTotals(1, TimeSpan.Zero, 1, 400), SampleTrends.Nothing);

        Assert.Multiple(() =>
        {
            Assert.That(cut.FindAll("[data-testid=tiles-volume-change] .same"), Has.Count.EqualTo(1));
            Assert.That(cut.FindAll("[data-testid=tiles-volume-change] svg"), Is.Empty);
        });
    }

    [Test]
    public void NothingLogged_ShowsZeros()
    {
        var cut = Render(PeriodKind.Week, SampleTrends.Nothing, SampleTrends.Nothing);

        Assert.That(cut.FindAll(".value").Select(v => v.TextContent), Is.EqualTo(new[] { "0", "0m", "0", "0 kg" }));
    }

    [Test]
    public void UnderTheTiles_SaysWhatTheyAreComparedWith() =>
        Assert.That(Render(PeriodKind.Month, SampleTrends.MonthTotals, SampleTrends.MonthEarlier).Find("[data-testid=tiles-against]").TextContent,
            Is.EqualTo("Against October to the 18th"));

    private IRenderedComponent<TrendTiles> Render(PeriodKind kind, TrendTotals totals, TrendTotals earlier) =>
        Context.Render<TrendTiles>(p => p
            .Add(x => x.Kind, kind)
            .Add(x => x.Totals, totals)
            .Add(x => x.Earlier, earlier)
            .Add(x => x.Against, kind == PeriodKind.Month ? "Against October to the 18th" : "Against the week before")
            .Add(x => x.TestId, "tiles"));
}
