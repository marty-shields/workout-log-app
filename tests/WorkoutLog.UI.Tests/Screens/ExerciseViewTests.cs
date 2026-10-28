using WorkoutLog.UI.Components.Charts;
using WorkoutLog.UI.Models.Exercises;
using WorkoutLog.UI.Screens;
using WorkoutLog.UI.Stories.Samples;
using WorkoutLog.UI.Tests.Stories;

namespace WorkoutLog.UI.Tests.Screens;

public class ExerciseViewTests : ComponentTestBase
{
    [Test]
    public void TopBar_ShowsTheNameAndSubtitle()
    {
        var cut = Render(SampleExercises.LiftTopSet);

        Assert.Multiple(() =>
        {
            Assert.That(cut.Find("[data-testid=screen-title]").TextContent, Is.EqualTo("Barbell Back Squat"));
            Assert.That(cut.Find("[data-testid=screen-subtitle]").TextContent, Is.EqualTo("Quadriceps · Barbell"));
        });
    }

    [Test]
    public void Tabs_ShowEachTabWithTheChosenOneChecked()
    {
        var cut = Render(SampleExercises.CardioPace);

        Assert.Multiple(() =>
        {
            Assert.That(cut.FindAll("[data-testid=exercise-tabs] .text").Select(t => t.TextContent), Is.EqualTo(new[] { "Distance", "Duration", "Pace", "Calories" }));
            Assert.That(cut.FindAll("[data-testid=exercise-tabs] input[checked]").Select(i => i.GetAttribute("data-testid")), Is.EqualTo(new[] { "exercise-tab-pace" }));
        });
    }

    [Test]
    public void OneChart_HasNoTabsAndShowsTheChartTitle()
    {
        var cut = Render(SampleExercises.Assisted);

        Assert.Multiple(() =>
        {
            Assert.That(cut.FindAll("[data-testid=exercise-tabs]"), Is.Empty);
            Assert.That(cut.Find("[data-testid=exercise-chart-card] h2").TextContent, Is.EqualTo("Least assist per session"));
        });
    }

    [Test]
    public void TappingATab_RaisesTabChanged()
    {
        var changed = new List<string>();
        var cut = Render(SampleExercises.LiftTopSet, p => p.Add(x => x.TabChanged, (string t) => changed.Add(t)));

        cut.Find("[data-testid=exercise-tab-e1rm]").Change(true);

        Assert.That(changed, Is.EqualTo(new[] { "e1rm" }));
    }

    [Test]
    public void Chart_GetsThePointsFormatAndANameWithTheRange()
    {
        var cut = Render(SampleExercises.CardioPace);

        var chart = cut.FindComponent<Chart>().Instance;
        Assert.Multiple(() =>
        {
            Assert.That(chart.Points, Is.SameAs(SampleExercises.CardioPace.Points));
            Assert.That((chart.Flipped, chart.TimeScale), Is.EqualTo((true, true)));
            Assert.That(chart.Format, Is.SameAs(SampleExercises.Clock));
            Assert.That(chart.Name, Is.EqualTo("Pace, last 3 months"));
        });
    }

    [Test]
    public void ChartName_WithATitle_UsesIt()
    {
        var cut = Render(SampleExercises.Assisted, p => p.Add(x => x.Range, ChartRange.Year));

        Assert.That(cut.FindComponent<Chart>().Instance.Name, Is.EqualTo("Least assist per session, last year"));
    }

    [Test]
    public void Caption_ShowsUnderTheRange()
    {
        var cut = Render(SampleExercises.LiftE1rm);

        Assert.That(cut.Find("[data-testid=exercise-chart-caption]").TextContent, Is.EqualTo("Estimated one-rep max"));
    }

    [Test]
    public void NoCaption_ShowsNone()
    {
        var cut = Render(SampleExercises.LiftTopSet);

        Assert.That(cut.FindAll("[data-testid=exercise-chart-caption]"), Is.Empty);
    }

    [TestCase(ChartRange.ThreeMonths, "exercise-range-3m")]
    [TestCase(ChartRange.SixMonths, "exercise-range-6m")]
    [TestCase(ChartRange.Year, "exercise-range-1y")]
    [TestCase(ChartRange.All, "exercise-range-all")]
    public void Range_ChecksItsSegment(ChartRange range, string testId)
    {
        var cut = Render(SampleExercises.LiftTopSet, p => p.Add(x => x.Range, range));

        Assert.That(cut.FindAll("[data-testid=exercise-range] input[checked]").Select(i => i.GetAttribute("data-testid")), Is.EqualTo(new[] { testId }));
    }

    [Test]
    public void TappingARange_RaisesRangeChanged()
    {
        var changed = new List<ChartRange>();
        var cut = Render(SampleExercises.LiftTopSet, p => p.Add(x => x.RangeChanged, (ChartRange r) => changed.Add(r)));

        cut.Find("[data-testid=exercise-range-all]").Change(true);

        Assert.That(changed, Is.EqualTo(new[] { ChartRange.All }));
    }

    [TestCase(ChartRange.ThreeMonths, "Records · last 3 months")]
    [TestCase(ChartRange.SixMonths, "Records · last 6 months")]
    [TestCase(ChartRange.Year, "Records · last year")]
    [TestCase(ChartRange.All, "Records · all time")]
    public void Records_AreTitledWithTheRange(ChartRange range, string title)
    {
        var cut = Render(SampleExercises.LiftTopSet, p => p.Add(x => x.Range, range));

        Assert.That(TestText.Normalise(cut.Find("[data-testid=exercise-records] h2").TextContent), Is.EqualTo(title));
    }

    [Test]
    public void Sessions_ShowARowEachInTheGivenOrder()
    {
        var cut = Render(SampleExercises.LiftTopSet);

        Assert.Multiple(() =>
        {
            Assert.That(cut.Find("h2.heading").TextContent, Is.EqualTo("Sessions"));
            Assert.That(cut.FindAll("[data-testid^=exercise-session-]").Select(s => s.GetAttribute("data-testid")),
                Is.EqualTo(new[] { "exercise-session-3", "exercise-session-2", "exercise-session-1" }));
            Assert.That(TestText.Normalise(cut.Find("[data-testid=exercise-session-3]").TextContent), Is.EqualTo("Tue 20 Oct 70kg × 3 60×8 · 62.5×8 · 70×3"));
        });
    }

    [Test]
    public void NoValuesOnTheTab_ShowsItsMessageInPlaceOfTheChart()
    {
        var cut = Render(SampleExercises.BodyweightTopSetNeverWeighted);

        Assert.Multiple(() =>
        {
            Assert.That(cut.Find("[data-testid=exercise-chart-empty]").TextContent, Is.EqualTo("No weighted sets in the last 3 months"));
            Assert.That(cut.FindComponents<Chart>(), Is.Empty);
            Assert.That(cut.FindAll("[data-testid=exercise-range]"), Has.Count.EqualTo(1));
            Assert.That(cut.FindAll("[data-testid^=exercise-session-]"), Has.Count.EqualTo(2));
        });
    }

    [TestCase(ChartRange.ThreeMonths, "No sessions in the last 3 months")]
    [TestCase(ChartRange.SixMonths, "No sessions in the last 6 months")]
    [TestCase(ChartRange.Year, "No sessions in the last year")]
    [TestCase(ChartRange.All, "No sessions yet")]
    public void NoSessionsInRange_ShowsTheRangeMessageDashedRecordsAndNoSessions(ChartRange range, string message)
    {
        var cut = Render(SampleExercises.NoSessionsInRange, p => p.Add(x => x.Range, range));

        Assert.Multiple(() =>
        {
            Assert.That(cut.Find("[data-testid=exercise-chart-empty]").TextContent, Is.EqualTo(message));
            Assert.That(cut.FindAll("[data-testid=exercise-records] .value").Select(v => v.TextContent), Is.EqualTo(new[] { "—", "—" }));
            Assert.That(cut.FindAll("h2.heading"), Is.Empty);
        });
    }

    [Test]
    public void OneSession_DrawsOneDot()
    {
        var cut = Render(SampleExercises.OneSession);

        Assert.Multiple(() =>
        {
            Assert.That(cut.FindAll("[data-testid=exercise-chart] circle"), Has.Count.EqualTo(1));
            Assert.That(cut.FindAll("[data-testid^=exercise-session-]"), Has.Count.EqualTo(1));
        });
    }

    [Test]
    public void Loading_ShowsTheNameAndSpinnerOnly()
    {
        var cut = Context.Render<ExerciseView>(p => p.Add(x => x.Name, "Barbell Back Squat").Add(x => x.Loading, true).Add(x => x.OnBack, () => { }));

        Assert.Multiple(() =>
        {
            Assert.That(cut.Find("[data-testid=screen-title]").TextContent, Is.EqualTo("Barbell Back Squat"));
            Assert.That(cut.Find("[data-testid=exercise-loading]").TextContent, Does.Contain("Loading…"));
            Assert.That(cut.FindAll("[data-testid=exercise-chart-card]"), Is.Empty);
            Assert.That(cut.FindAll("[data-testid=back]"), Has.Count.EqualTo(1));
        });
    }

    [Test]
    public void Failed_ShowsTheNameErrorAndTryAgainOnly()
    {
        var cut = Context.Render<ExerciseView>(p => p.Add(x => x.Name, "Barbell Back Squat").Add(x => x.Failed, true));

        Assert.Multiple(() =>
        {
            Assert.That(cut.Find("[data-testid=screen-title]").TextContent, Is.EqualTo("Barbell Back Squat"));
            Assert.That(cut.Find("[data-testid=exercise-error]").TextContent, Is.EqualTo("Couldn't load this exercise"));
            Assert.That(cut.Find("[data-testid=exercise-retry]").TextContent.Trim(), Is.EqualTo("Try again"));
            Assert.That(cut.FindAll("[data-testid=exercise-chart-card]"), Is.Empty);
        });
    }

    [Test]
    public void TryAgain_RaisesOnRetry()
    {
        var retries = 0;
        var cut = Context.Render<ExerciseView>(p => p.Add(x => x.Name, "Plank").Add(x => x.Failed, true).Add(x => x.OnRetry, () => retries++));

        cut.Find("[data-testid=exercise-retry]").Click();

        Assert.That(retries, Is.EqualTo(1));
    }

    [Test]
    public void Failed_FocusesTryAgain()
    {
        var cut = Context.Render<ExerciseView>(p => p.Add(x => x.Name, "Plank").Add(x => x.Failed, true));

        cut.WaitForAssertion(() => Context.JSInterop.Invocations["Blazor._internal.domWrapper.focus"].Single().Arguments[0]
            .ShouldBeElementReferenceTo(cut.Find("[data-testid=exercise-retry]")));
    }

    [Test]
    public void Back_RaisesOnBack()
    {
        var back = false;
        var cut = Render(SampleExercises.LiftTopSet, p => p.Add(x => x.OnBack, () => back = true));

        cut.Find("[data-testid=back]").Click();

        Assert.That(back, Is.True);
    }

    private IRenderedComponent<ExerciseView> Render(SampleExercise sample, Action<ComponentParameterCollectionBuilder<ExerciseView>>? more = null) =>
        Context.Render<ExerciseView>(p =>
        {
            ScreenStates.AddExercise(p, sample);
            more?.Invoke(p);
        });
}
