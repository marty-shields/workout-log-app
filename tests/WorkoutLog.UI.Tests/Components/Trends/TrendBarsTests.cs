using WorkoutLog.UI.Components.Trends;
using WorkoutLog.UI.Models.Workouts;
using WorkoutLog.UI.Stories.Samples;

namespace WorkoutLog.UI.Tests.Components.Trends;

public class TrendBarsTests : ComponentTestBase
{
    [Test]
    public void Paired_DrawsTheEarlierBarLeftOfThisOneAndALegend()
    {
        var cut = RenderDays();

        var monday = cut.FindAll(".bar").Take(2).ToList();
        Assert.Multiple(() =>
        {
            Assert.That(monday[0].ClassList, Does.Contain("past"));
            Assert.That(monday[1].ClassList, Does.Contain("now"));
            Assert.That(double.Parse(monday[0].GetAttribute("x")!), Is.LessThan(double.Parse(monday[1].GetAttribute("x")!)));
            Assert.That(cut.FindAll("[data-testid=bars-legend] li").Select(li => li.TextContent), Is.EqualTo(new[] { "This week", "Last week" }));
        });
    }

    [Test]
    public void Paired_SkipsABarWithNoValue()
    {
        var cut = RenderDays();

        // Mon 2, Tue 1 (this week only), Wed 2, Fri 1 and Sat 1 (last week only).
        Assert.That(cut.FindAll(".bar"), Has.Count.EqualTo(7));
    }

    [Test]
    public void Paired_BarHeightsFollowTheMax()
    {
        var cut = RenderDays();

        var heights = cut.FindAll(".bar.now").Select(b => double.Parse(b.GetAttribute("height")!)).ToList();
        Assert.That(heights[0], Is.EqualTo(62 / 90.0 * 100).Within(0.01));
    }

    [Test]
    public void Chart_IsNamedAndHasATextAlternativeSayingBothValues()
    {
        var cut = RenderDays();

        Assert.Multiple(() =>
        {
            Assert.That(cut.Find("figure").GetAttribute("aria-label"), Is.EqualTo("Time trained each day this week beside the same day last week"));
            Assert.That(cut.Find("svg").GetAttribute("aria-hidden"), Is.EqualTo("true"));
            Assert.That(cut.FindAll("[data-testid=bars-values] li").Select(li => li.TextContent).Take(4),
                Is.EqualTo(new[]
                {
                    "Monday: 1h 02m (last week 1h 05m)",
                    "Tuesday: 35m (last week rest day)",
                    "Wednesday: 58m (last week 55m)",
                    "Thursday: still to come (last week rest day)",
                }));
            Assert.That(cut.FindAll("[data-testid=bars-values] li").Last().TextContent, Is.EqualTo("Sunday: still to come (last week rest day)"));
        });
    }

    [Test]
    public void Paired_CurrentColumnHasABoldAxisLabel()
    {
        var cut = RenderDays();

        Assert.That(cut.FindAll(".axis .current").Select(c => c.TextContent), Is.EqualTo(new[] { "Wed" }));
    }

    [Test]
    public void Single_CurrentMonthIsAccentAndTheRestPast()
    {
        var cut = RenderMonths(SampleTrends.YearMonths);

        Assert.Multiple(() =>
        {
            Assert.That(cut.FindAll(".bar"), Has.Count.EqualTo(11));
            Assert.That(cut.FindAll(".bar.now"), Has.Count.EqualTo(1));
            Assert.That(cut.FindAll("[data-testid=bars-legend]"), Is.Empty);
        });
    }

    [Test]
    public void Single_LabelsTheCurrentAndHighestMonthsOnly()
    {
        var cut = RenderMonths(SampleTrends.YearMonths);

        Assert.That(cut.FindAll(".value").Select(v => v.TextContent), Is.EqualTo(new[] { "17", "11" }));
    }

    [Test]
    public void Single_WithNoCurrentMonth_LabelsTheHighestAndHasNoAccentBar()
    {
        var cut = RenderMonths(SampleTrends.PastYearMonths);

        Assert.Multiple(() =>
        {
            Assert.That(cut.FindAll(".value").Select(v => v.TextContent), Is.EqualTo(new[] { "15" }));
            Assert.That(cut.FindAll(".bar.now"), Is.Empty);
            Assert.That(cut.FindAll(".axis .current"), Is.Empty);
        });
    }

    [Test]
    public void ValueLabel_SitsAtTheBarsHeight()
    {
        var cut = RenderMonths(SampleTrends.YearMonths);

        Assert.That(cut.Find(".value").GetAttribute("style"), Does.Contain("--share: 1"));
    }

    [Test]
    public void Single_TextAlternativeNamesEveryMonth()
    {
        var items = RenderMonths(SampleTrends.YearMonths).FindAll("[data-testid=bars-values] li").Select(li => li.TextContent).ToList();

        Assert.Multiple(() =>
        {
            Assert.That(items, Has.Count.EqualTo(12));
            Assert.That(items[4], Is.EqualTo("May: 17 workouts"));
            Assert.That(items[11], Is.EqualTo("December: no workouts"));
        });
    }

    [Test]
    public void AllEmpty_ShowsTheMessageInPlaceOfTheChart()
    {
        var cut = RenderMonths(SampleTrends.EmptyYearMonths);

        Assert.Multiple(() =>
        {
            Assert.That(cut.Find("[data-testid=bars-none]").TextContent, Is.EqualTo("No workouts this year"));
            Assert.That(cut.FindAll("svg, figure"), Is.Empty);
        });
    }

    [Test]
    public void LabelledBarAtZero_PrintsNoValueButANonZeroOneDoes()
    {
        TrendBar[] bars = [new("A", "April", 0, "no workouts", Current: true, Labelled: true), new("M", "May", 5, "5 workouts", Labelled: true)];

        var cut = Context.Render<TrendBars>(p => p
            .Add(x => x.Bars, bars)
            .Add(x => x.AriaLabel, "Workouts")
            .Add(x => x.EmptyText, "None")
            .Add(x => x.TestId, "bars"));

        Assert.That(cut.FindAll(".values .value").Select(v => v.TextContent), Is.EqualTo(new[] { "5" }));
    }

    [Test]
    public void MaxOfZero_DrawsWithoutDividingByZero()
    {
        TrendBar[] bars = [new("A", "April", 3, "3 workouts")];

        var cut = Context.Render<TrendBars>(p => p
            .Add(x => x.Bars, bars)
            .Add(x => x.Max, 0)
            .Add(x => x.AriaLabel, "Workouts")
            .Add(x => x.EmptyText, "None")
            .Add(x => x.TestId, "bars"));

        Assert.That(double.Parse(cut.Find(".bar").GetAttribute("height")!), Is.LessThanOrEqualTo(300).And.GreaterThan(0));
    }

    [Test]
    public void NoOnOpen_NoColumnIsTappable() =>
        Assert.That(RenderMonths([new TrendBar("J", "January", 3, "3 workouts", Span: Jan)]).FindAll("button"), Is.Empty);

    [Test]
    public void WithOnOpen_OnlyColumnsWithDatesAreTappableAndRaiseTheirDates()
    {
        var opened = new List<DateSpan>();
        var cut = Context.Render<TrendBars>(p => p
            .Add(x => x.Bars, new[]
            {
                new TrendBar("J", "January", 3, "3 workouts", Span: Jan),
                new TrendBar("F", "February", 0, "no workouts"),
                new TrendBar("M", "March", 2, "2 workouts"),
                new TrendBar("A", "April", 0, "under a minute", Span: Apr),
            })
            .Add(x => x.AriaLabel, "Workouts in each month of 2026")
            .Add(x => x.EmptyText, "No workouts this year")
            .Add(x => x.OnOpen, opened.Add)
            .Add(x => x.TestId, "bars"));

        cut.Find("[data-testid=bars-1-open]").Click();

        Assert.Multiple(() =>
        {
            Assert.That(cut.FindAll("button").Select(b => b.GetAttribute("data-testid")), Is.EqualTo(new[] { "bars-1-open", "bars-4-open" }));
            Assert.That(opened, Is.EqualTo(new[] { Jan }));
            Assert.That(cut.Find("[data-testid=bars-1-open] .visually-hidden").TextContent, Is.EqualTo("Open workouts, January"));
        });
    }

    private static readonly DateSpan Apr = new(new DateOnly(2026, 4, 1), new DateOnly(2026, 4, 30));

    private static readonly DateSpan Jan = new(new DateOnly(2026, 1, 1), new DateOnly(2026, 1, 31));

    private IRenderedComponent<TrendBars> RenderDays() =>
        Context.Render<TrendBars>(p => p
            .Add(x => x.Bars, SampleTrends.WeekDays)
            .Add(x => x.NowLabel, "This week")
            .Add(x => x.EarlierLabel, "Last week")
            .Add(x => x.Max, 90)
            .Add(x => x.AriaLabel, "Time trained each day this week beside the same day last week")
            .Add(x => x.EmptyText, "No workouts this week")
            .Add(x => x.TestId, "bars"));

    private IRenderedComponent<TrendBars> RenderMonths(IReadOnlyList<TrendBar> bars) =>
        Context.Render<TrendBars>(p => p
            .Add(x => x.Bars, bars)
            .Add(x => x.AriaLabel, "Workouts in each month of 2026")
            .Add(x => x.EmptyText, "No workouts this year")
            .Add(x => x.TestId, "bars"));
}
