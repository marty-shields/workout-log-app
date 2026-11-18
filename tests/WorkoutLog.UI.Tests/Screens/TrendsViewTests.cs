using WorkoutLog.Core.Domain.Workouts.Periods;
using WorkoutLog.UI.Components.Actions;
using WorkoutLog.UI.Navigation;
using WorkoutLog.UI.Screens;
using WorkoutLog.UI.Stories.Samples;
using WorkoutLog.UI.Tests.Stories;

namespace WorkoutLog.UI.Tests.Screens;

public class TrendsViewTests : ComponentTestBase
{
    private const string Cards = "[data-testid=trends-tiles], [data-testid=trends-days], [data-testid=trends-muscles], [data-testid=trends-records], "
        + "[data-testid=trends-recent-weeks], [data-testid=trends-calendar], [data-testid=trends-weeks], [data-testid=trends-most-trained], "
        + "[data-testid=trends-months], [data-testid=trends-streak], [data-testid=trends-records-by-month]";

    [Test]
    public void Frame_ShowsTitleSettingsKindControlStepperAndTabBar()
    {
        var cut = Render(SampleTrends.Week);

        Assert.Multiple(() =>
        {
            Assert.That(cut.Find("[data-testid=screen-title]").TextContent, Is.EqualTo("Trends"));
            Assert.That(cut.Find("[data-testid=settings]").GetAttribute("aria-label"), Is.EqualTo("Settings"));
            Assert.That(cut.FindAll("[data-testid=back]"), Is.Empty);
            Assert.That(cut.FindAll("[data-testid=trends-kind] .text").Select(t => t.TextContent), Is.EqualTo(new[] { "Week", "Month", "Year" }));
            Assert.That(cut.Find("[data-testid=trends-week]").HasAttribute("checked"), Is.True);
            Assert.That(cut.Find("[data-testid=trends-stepper-label]").TextContent, Is.EqualTo("16 – 22 Nov"));
            Assert.That(cut.Find("[aria-current=page]").GetAttribute("data-testid"), Is.EqualTo("trends-link"));
        });
    }

    [Test]
    public void Week_ShowsTheTilesThenItsFourCardsInOrder()
    {
        var cut = Render(SampleTrends.Week);

        Assert.That(cut.FindAll(Cards).Select(e => e.GetAttribute("data-testid")),
            Is.EqualTo(new[] { "trends-tiles", "trends-days", "trends-muscles", "trends-records", "trends-recent-weeks" }));
    }

    [Test]
    public void Week_ShowsPairedDayBarsWithALegend()
    {
        var cut = Render(SampleTrends.Week);

        Assert.Multiple(() =>
        {
            Assert.That(cut.Find("[data-testid=trends-days] .title").TextContent, Is.EqualTo("Time trained by day"));
            Assert.That(cut.FindAll("[data-testid=trends-days-chart-legend] li").Select(li => li.TextContent), Is.EqualTo(new[] { "This week", "Last week" }));
            Assert.That(cut.Find("[data-testid=trends-days-chart]").GetAttribute("aria-label"), Is.EqualTo("Time trained each day this week beside the same day last week"));
        });
    }

    [Test]
    public void Week_RecordsAndMuscleCardsSayThisWeek()
    {
        var cut = Render(SampleTrends.Week);

        Assert.Multiple(() =>
        {
            Assert.That(cut.Find("[data-testid=trends-records-aside]").TextContent, Is.EqualTo("1 this week"));
            Assert.That(cut.Find("[data-testid=trends-muscles] .aside").TextContent, Is.EqualTo("this week"));
        });
    }

    [Test]
    public void Week_ShowsTheLastTwelveWeeksWithTheStreak()
    {
        var cut = Render(SampleTrends.Week);

        Assert.Multiple(() =>
        {
            Assert.That(cut.Find("[data-testid=trends-recent-weeks] .title").TextContent, Is.EqualTo("Last 12 weeks"));
            Assert.That(cut.Find("[data-testid=trends-recent-weeks-aside]").TextContent, Is.EqualTo("23-week streak"));
            Assert.That(cut.Find("[data-testid=trends-recent-weeks-chart]").GetAttribute("aria-label"), Is.EqualTo("Workouts per week, last 12 weeks"));
        });
    }

    [Test]
    public void Week_WithNoStreak_HidesTheStreakText()
    {
        var cut = Context.Render<TrendsView>(p => p.Sample(With(SampleTrends.Week, nameof(TrendsView.Streak), 0)));

        Assert.That(cut.FindAll("[data-testid=trends-recent-weeks-aside]"), Is.Empty);
    }

    [Test]
    public void Month_ShowsTheTilesThenItsFourCardsInOrder()
    {
        var cut = Render(SampleTrends.Month);

        Assert.Multiple(() =>
        {
            Assert.That(cut.FindAll(Cards).Select(e => e.GetAttribute("data-testid")),
                Is.EqualTo(new[] { "trends-tiles", "trends-calendar", "trends-weeks", "trends-records", "trends-most-trained" }));
            Assert.That(cut.Find("[data-testid=trends-stepper-label]").TextContent, Is.EqualTo("November 2026"));
            Assert.That(cut.Find("[data-testid=trends-month]").HasAttribute("checked"), Is.True);
        });
    }

    [Test]
    public void Month_ShowsTheCalendarWeeksRecordsAndMostTrained()
    {
        var cut = Render(SampleTrends.Month);

        Assert.Multiple(() =>
        {
            Assert.That(cut.FindAll("[data-testid=trends-calendar-days] .day"), Has.Count.EqualTo(30));
            Assert.That(cut.FindAll("[data-testid=trends-weeks-rows] .week"), Has.Count.EqualTo(4));
            Assert.That(cut.Find("[data-testid=trends-records-aside]").TextContent, Is.EqualTo("4 this month"));
            Assert.That(cut.Find("[data-testid=trends-most-trained-aside]").TextContent, Is.EqualTo("sessions"));
            Assert.That(cut.FindAll("[data-testid=trends-most-trained-rows] .row"), Has.Count.EqualTo(5));
            Assert.That(cut.Find("[data-testid=trends-most-trained-rows]").GetAttribute("aria-label"), Is.EqualTo("Sessions of the most trained exercises this month"));
        });
    }

    [Test]
    public void Year_ShowsTheTilesThenItsThreeCardsInOrder()
    {
        var cut = Render(SampleTrends.Year);

        Assert.Multiple(() =>
        {
            Assert.That(cut.FindAll(Cards).Select(e => e.GetAttribute("data-testid")),
                Is.EqualTo(new[] { "trends-tiles", "trends-months", "trends-streak", "trends-records-by-month" }));
            Assert.That(cut.Find("[data-testid=trends-stepper-label]").TextContent, Is.EqualTo("2026"));
            Assert.That(cut.Find("[data-testid=trends-tiles-volume] .value").TextContent, Is.EqualTo("548 t"));
        });
    }

    [Test]
    public void Year_ShowsMonthBarsThePerWeekRateTheStreakAndRecordsByMonth()
    {
        var cut = Render(SampleTrends.Year);

        Assert.Multiple(() =>
        {
            Assert.That(cut.Find("[data-testid=trends-months-aside]").TextContent, Is.EqualTo("3.2 a week"));
            Assert.That(cut.Find("[data-testid=trends-months-chart]").GetAttribute("aria-label"), Is.EqualTo("Workouts in each month of 2026"));
            Assert.That(cut.Find("[data-testid=trends-streak-row] .aside").TextContent, Is.EqualTo("23 weeks"));
            Assert.That(cut.Find("[data-testid=trends-streak-row] .detail").TextContent, Is.EqualTo("Still going"));
            Assert.That(cut.Find("[data-testid=trends-records-by-month-aside]").TextContent, Is.EqualTo("37 this year"));
            Assert.That(cut.FindAll("[data-testid=trends-records-by-month-rows] .row"), Has.Count.EqualTo(11));
            Assert.That(cut.Find("[data-testid=trends-records-by-month-rows]").GetAttribute("aria-label"), Is.EqualTo("Records set in each month of 2026"));
        });
    }

    [Test]
    public void Year_WithNoPerWeekRate_HidesIt()
    {
        var cut = Context.Render<TrendsView>(p => p.Sample(With(SampleTrends.Year, nameof(TrendsView.PerWeek), 0.0)));

        Assert.That(cut.FindAll("[data-testid=trends-months-aside]"), Is.Empty);
    }

    [Test]
    public void CurrentPeriod_DisablesTheNextArrow()
    {
        Assert.Multiple(() =>
        {
            foreach (var state in new[] { SampleTrends.Week, SampleTrends.Month, SampleTrends.Year })
            {
                var cut = Render(state);
                Assert.That(cut.Find("[data-testid=trends-stepper-next]").HasAttribute("disabled"), Is.True);
                Assert.That(cut.Find("[data-testid=trends-stepper-previous]").HasAttribute("disabled"), Is.False);
            }
        });
    }

    [Test]
    public void PastWeek_EnablesBothArrowsAndSaysThatWeek()
    {
        var cut = Render(SampleTrends.PastWeek);

        Assert.Multiple(() =>
        {
            Assert.That(cut.Find("[data-testid=trends-stepper-next]").HasAttribute("disabled"), Is.False);
            Assert.That(cut.Find("[data-testid=trends-stepper-previous]").HasAttribute("disabled"), Is.False);
            Assert.That(cut.Find("[data-testid=trends-records-aside]").TextContent, Is.EqualTo("2 that week"));
            Assert.That(cut.Find("[data-testid=trends-muscles] .aside").TextContent, Is.EqualTo("that week"));
            Assert.That(cut.FindAll("[data-testid=trends-days-chart-legend] li").Select(li => li.TextContent), Is.EqualTo(new[] { "That week", "The week before" }));
            Assert.That(cut.Find("[data-testid=trends-tiles-against]").TextContent, Is.EqualTo("Against the week before"));
            Assert.That(cut.Find("[data-testid=trends-days-chart]").GetAttribute("aria-label"),
                Is.EqualTo("Time trained each day that week beside the same day the week before"));
        });
    }

    [Test]
    public void PastMonth_HasNoTodayRingAndSaysThatMonth()
    {
        var cut = Render(SampleTrends.PastMonth);

        Assert.Multiple(() =>
        {
            Assert.That(cut.FindAll("[data-testid=trends-calendar-days] .today"), Is.Empty);
            Assert.That(cut.FindAll("[data-testid=trends-weeks-rows] .week"), Has.Count.EqualTo(5));
            Assert.That(cut.Find("[data-testid=trends-records-aside]").TextContent, Is.EqualTo("2 that month"));
        });
    }

    [Test]
    public void PastYear_SaysThatYearAndHasNoAccentBar()
    {
        var cut = Render(SampleTrends.PastYear);

        Assert.Multiple(() =>
        {
            Assert.That(cut.Find("[data-testid=trends-records-by-month-aside]").TextContent, Is.EqualTo("33 that year"));
            Assert.That(cut.FindAll("[data-testid=trends-months-chart] .bar.now"), Is.Empty);
            Assert.That(cut.FindAll("[data-testid=trends-streak-row] .detail"), Is.Empty);
        });
    }

    [Test]
    public void EmptyWeek_ShowsZerosAndSaysWhatIsMissing()
    {
        var cut = Render(SampleTrends.EmptyWeek);

        Assert.Multiple(() =>
        {
            Assert.That(cut.FindAll("[data-testid=trends-tiles] .value").Select(v => v.TextContent), Is.EqualTo(new[] { "0", "0m", "0", "0 kg" }));
            Assert.That(cut.Find("[data-testid=trends-days-chart-none]").TextContent, Is.EqualTo("No workouts that week"));
            Assert.That(cut.Find("[data-testid=trends-records-none]").TextContent, Is.EqualTo("No new records that week"));
            Assert.That(cut.FindAll("[data-testid=trends-muscles] .sets").Select(s => s.TextContent), Is.All.EqualTo("Not trained"));
            Assert.That(cut.Find("[data-testid=trends-recent-weeks-chart-none]").TextContent, Is.EqualTo("No workouts in the last 12 weeks"));
            Assert.That(cut.Find("[data-testid=trends-stepper-previous]").HasAttribute("disabled"), Is.True);
        });
    }

    [Test]
    public void EmptyMonth_ShowsAnEmptyCalendarAndMessages()
    {
        var cut = Render(SampleTrends.EmptyMonth);

        Assert.Multiple(() =>
        {
            Assert.That(cut.FindAll("[data-testid=trends-calendar-days] .done"), Is.Empty);
            Assert.That(cut.FindAll("[data-testid=trends-weeks-rows] .totals").Select(t => t.TextContent), Is.All.EqualTo("0 workouts · 0m"));
            Assert.That(cut.Find("[data-testid=trends-records-none]").TextContent, Is.EqualTo("No new records that month"));
            Assert.That(cut.Find("[data-testid=trends-most-trained-rows-none]").TextContent, Is.EqualTo("No exercises that month"));
        });
    }

    [Test]
    public void EmptyYear_ShowsMessagesInPlaceOfTheCharts()
    {
        var cut = Render(SampleTrends.EmptyYear);

        Assert.Multiple(() =>
        {
            Assert.That(cut.Find("[data-testid=trends-months-chart-none]").TextContent, Is.EqualTo("No workouts that year"));
            Assert.That(cut.Find("[data-testid=trends-streak-row] .aside").TextContent, Is.EqualTo("None"));
            Assert.That(cut.Find("[data-testid=trends-records-by-month-rows-none]").TextContent, Is.EqualTo("No records that year"));
        });
    }

    [Test]
    public void Loading_ShowsASpinnerAndKeepsTheControls()
    {
        var cut = Render(SampleTrends.Loading);

        Assert.Multiple(() =>
        {
            Assert.That(cut.FindAll("[data-testid=trends-loading]"), Has.Count.EqualTo(1));
            Assert.That(cut.FindAll(Cards), Is.Empty);
            Assert.That(cut.FindAll("[data-testid=trends-kind]"), Has.Count.EqualTo(1));
            Assert.That(cut.FindAll("[data-testid=trends-stepper]"), Has.Count.EqualTo(1));
            Assert.That(cut.FindAll("[data-testid=tab-bar]"), Has.Count.EqualTo(1));
        });
    }

    [Test]
    public void LoadFailed_ShowsTheErrorAndTryAgainAndKeepsTheControls()
    {
        var retried = 0;
        var cut = Context.Render<TrendsView>(p => p.Sample(SampleTrends.LoadFailed).Add(x => x.OnRetry, () => retried++));

        cut.Find("[data-testid=trends-retry]").Click();

        Assert.Multiple(() =>
        {
            Assert.That(cut.Find("[data-testid=trends-error]").TextContent, Is.EqualTo("Couldn't load Trends"));
            Assert.That(cut.FindAll(Cards), Is.Empty);
            Assert.That(cut.FindAll("[data-testid=trends-loading]"), Is.Empty);
            Assert.That(cut.FindAll("[data-testid=trends-kind]"), Has.Count.EqualTo(1));
            Assert.That(retried, Is.EqualTo(1));
        });
    }

    [Test]
    public void Callbacks_AreRaised()
    {
        var kinds = new List<PeriodKind>();
        var calls = new List<string>();
        var records = new List<int>();
        var cut = Context.Render<TrendsView>(p => p.Sample(SampleTrends.Month)
            .Add(x => x.OnKindChanged, kinds.Add)
            .Add(x => x.OnPrevious, () => calls.Add("previous"))
            .Add(x => x.OnSettings, () => calls.Add("settings"))
            .Add(x => x.OnNew, () => calls.Add("new"))
            .Add(x => x.OnNavigate, (MainTab tab) => calls.Add(tab.ToString()))
            .Add(x => x.OnOpenRecord, records.Add));

        cut.Find("[data-testid=trends-year]").Change(true);
        cut.Find("[data-testid=trends-stepper-previous]").Click();
        cut.Find("[data-testid=settings]").Click();
        cut.Find("[data-testid=new-link]").Click();
        cut.Find("[data-testid=this-week-link]").Click();
        cut.Find("[data-testid=trends-records-row-22]").Click();

        Assert.Multiple(() =>
        {
            Assert.That(kinds, Is.EqualTo(new[] { PeriodKind.Year }));
            Assert.That(calls, Is.EqualTo(new[] { "previous", "settings", "new", "ThisWeek" }));
            Assert.That(records, Is.EqualTo(new[] { 22 }));
        });
    }

    [Test]
    public void Settings_UsesTheGearIcon() =>
        Assert.That(Render(SampleTrends.Week).FindComponents<IconButton>().Single().Instance.Icon, Is.EqualTo(IconName.Settings));

    private static Dictionary<string, object?> With(IReadOnlyDictionary<string, object?> state, string name, object value) =>
        new(state) { [name] = value };

    private IRenderedComponent<TrendsView> Render(IReadOnlyDictionary<string, object?> state) =>
        Context.Render<TrendsView>(p => p.Sample(state));
}
