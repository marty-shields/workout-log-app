using Shields.UI.Components.Actions;
using Shields.UI.Components.Charts;
using WorkoutLog.Core.Domain.Workouts.Periods;
using WorkoutLog.UI.Components.Trends;
using WorkoutLog.UI.Models.Workouts;
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
    public void Kinds_OnlyWeek_LeavesTheKindControlOut()
    {
        var cut = Render(SampleTrends.Week.Append(new KeyValuePair<string, object?>(nameof(TrendsView.Kinds), new[] { PeriodKind.Week })).ToDictionary());

        Assert.Multiple(() =>
        {
            Assert.That(cut.FindAll("[data-testid=trends-kind]"), Is.Empty);
            Assert.That(cut.FindAll("[data-testid=trends-stepper]"), Has.Count.EqualTo(1));
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
    public void Week_RecentWeeks_ListsTheWeeksOldestFirstWithStartAndEndLabels()
    {
        var cut = Render(SampleTrends.Week);

        var values = cut.FindAll("[data-testid=trends-recent-weeks-chart-values] li").Select(li => li.TextContent).ToList();
        Assert.Multiple(() =>
        {
            Assert.That(values, Has.Count.EqualTo(12));
            Assert.That(values[0], Does.StartWith("11 weeks ago: "));
            Assert.That(values[1], Does.StartWith("10 weeks ago: "));
            Assert.That(values[^2], Does.StartWith("Last week: "));
            Assert.That(values[^1], Does.StartWith("This week: "));
            Assert.That(values[0], Is.EqualTo($"11 weeks ago: {DisplayFormat.Workouts(SampleTrends.RecentWeeks[0])}"));
            Assert.That(values[^1], Is.EqualTo($"This week: {DisplayFormat.Workouts(SampleTrends.RecentWeeks[^1])}"));
            Assert.That(cut.Find("[data-testid=trends-recent-weeks-chart] .axis").TextContent, Does.Contain("11 weeks ago").And.Contain("This week"));
        });
    }

    [Test]
    public void Week_RecentWeeks_AllZero_ShowsTheEmptyText()
    {
        var state = SampleTrends.Week.ToDictionary();
        state[nameof(TrendsView.RecentWeeks)] = new int[12];

        var cut = Render(state);

        Assert.Multiple(() =>
        {
            Assert.That(cut.Find("[data-testid=trends-recent-weeks-chart-none]").TextContent, Is.EqualTo("No workouts in the last 12 weeks"));
            Assert.That(cut.FindAll("[data-testid=trends-recent-weeks-chart-values]"), Is.Empty);
        });
    }

    [Test]
    public void Week_ShowsPairedDayBarsWithALegend()
    {
        var cut = Render(SampleTrends.Week);

        Assert.Multiple(() =>
        {
            Assert.That(cut.Find("[data-testid=trends-days] .title").TextContent, Is.EqualTo("Time trained by day"));
            Assert.That(cut.FindAll("[data-testid=trends-days-chart-legend] li").Select(li => li.TextContent), Is.EqualTo(new[] { "This week", "Last week" }));
            Assert.That(cut.FindAll("[data-testid=trends-days-chart-legend] .swatch").Select(s => s.ClassList.Last()), Is.EqualTo(new[] { "accent", "past" }));
            Assert.That(cut.Find("[data-testid=trends-days-chart]").GetAttribute("aria-label"), Is.EqualTo("Time trained each day this week beside the same day last week"));
            Assert.That(cut.Find("[data-testid=trends-days-chart-bar-2026-11-16]").GetAttribute("aria-label"), Is.EqualTo("Mon 16 Nov, 62 min, last week 65 min"));
            Assert.That(cut.FindAll("[data-testid=trends-days-chart-earlier-2026-11-16], [data-testid=trends-days-chart-column-2026-11-16]"), Has.Count.EqualTo(2));
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
    public void Month_CalendarLegend_NamesTrainedNoWorkoutAndToday()
    {
        var cut = Render(SampleTrends.Month);

        var items = cut.FindAll("[data-testid=trends-calendar-days-legend] .item");

        Assert.Multiple(() =>
        {
            Assert.That(items.Select(i => i.QuerySelector(".text")!.TextContent), Is.EqualTo(new[] { "Trained", "No workout", "Today" }));
            Assert.That(items.Select(i => i.QuerySelector(".swatch")!.ClassList.Contains(i.QuerySelector(".text")!.TextContent switch
            {
                "Trained" => "primary",
                "No workout" => "outline",
                _ => "today",
            })), Is.All.True);
        });
    }

    [Test]
    public void Month_CalendarWeekdayInitials_AreMondayFirstInEnglish()
    {
        var cut = Render(SampleTrends.Month);

        Assert.That(cut.FindAll("[data-testid=trends-calendar-days] .weekdays span").Select(d => d.TextContent),
            Is.EqualTo(new[] { "M", "T", "W", "T", "F", "S", "S" }));
    }

    [Test]
    public void Month_TrainedDay_IsRecordedAndAnUntrainedDayIsNot()
    {
        var cut = Render(SampleTrends.Month);

        var trained = cut.Find("[data-testid=trends-calendar-days-day-2026-11-04] .visually-hidden").TextContent;
        var untrained = cut.Find("[data-testid=trends-calendar-days-day-2026-11-03] .visually-hidden").TextContent;

        Assert.Multiple(() =>
        {
            Assert.That(trained, Does.StartWith("Wed 4 Nov").And.EndWith("recorded"));
            Assert.That(untrained, Is.EqualTo("Tue 3 Nov, not recorded"));
        });
    }

    [Test]
    public void Month_ShowsTheCalendarWeeksRecordsAndMostTrained()
    {
        var cut = Render(SampleTrends.Month);

        Assert.Multiple(() =>
        {
            Assert.That(cut.FindAll("[data-testid=trends-calendar-days] .cell"), Has.Count.EqualTo(30));
            Assert.That(cut.FindAll("[data-testid=trends-weeks-rows] .item"), Has.Count.EqualTo(4));
            Assert.That(cut.Find("[data-testid=trends-records-aside]").TextContent, Is.EqualTo("4 this month"));
            Assert.That(cut.Find("[data-testid=trends-most-trained-aside]").TextContent, Is.EqualTo("sessions"));
            Assert.That(cut.FindAll("[data-testid=trends-most-trained-rows] li"), Has.Count.EqualTo(5));
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
            Assert.That(cut.FindAll("[data-testid=trends-records-by-month-rows] li"), Has.Count.EqualTo(11));
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
            Assert.That(cut.FindAll("[data-testid=trends-weeks-rows] .item"), Has.Count.EqualTo(5));
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
            Assert.That(cut.Find("[data-testid=trends-days-chart-empty]").TextContent, Is.EqualTo("No workouts that week"));
            Assert.That(cut.Find("[data-testid=trends-records-none]").TextContent, Is.EqualTo("No new records that week"));
            Assert.That(cut.FindAll("[data-testid=trends-muscles] .value").Select(s => s.TextContent), Is.All.EqualTo("Not trained"));
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
            Assert.That(cut.FindAll("[data-testid=trends-calendar-days] .marked"), Is.Empty);
            Assert.That(cut.FindAll("[data-testid=trends-weeks-rows] .item .value").Select(t => t.TextContent), Is.All.EqualTo("0 workouts · 0m"));
            Assert.That(cut.Find("[data-testid=trends-records-none]").TextContent, Is.EqualTo("No new records that month"));
            Assert.That(cut.Find("[data-testid=trends-most-trained-rows-empty]").TextContent, Is.EqualTo("No exercises that month"));
        });
    }

    [Test]
    public void EmptyYear_ShowsMessagesInPlaceOfTheCharts()
    {
        var cut = Render(SampleTrends.EmptyYear);

        Assert.Multiple(() =>
        {
            Assert.That(cut.Find("[data-testid=trends-months-chart-empty]").TextContent, Is.EqualTo("No workouts that year"));
            Assert.That(cut.Find("[data-testid=trends-streak-row] .aside").TextContent, Is.EqualTo("None"));
            Assert.That(cut.Find("[data-testid=trends-records-by-month-rows-all-zero]").TextContent, Is.EqualTo("No records that year"));
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
            Assert.That(cut.FindAll("[data-testid=bottom-nav]"), Has.Count.EqualTo(1));
        });
    }

    [Test]
    public void BottomNav_MarksTrendsCurrentAndLabelsTheRaisedButtonNew()
    {
        var cut = Render(SampleTrends.Loading);

        Assert.Multiple(() =>
        {
            Assert.That(cut.FindAll("[data-testid=bottom-nav] [aria-current=page]").Select(e => e.GetAttribute("data-testid")), Is.EqualTo(new[] { "trends-link" }));
            Assert.That(cut.Find("[data-testid=add-button] .label").TextContent, Is.EqualTo("New"));
            Assert.That(cut.Find("[data-testid=add-button]").HasAttribute("aria-current"), Is.False);
            Assert.That(cut.FindAll("[data-testid=add-button] svg.icon"), Has.Count.EqualTo(1));
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
            Assert.That(cut.Find("[data-testid=trends-error]").TextContent.Trim(), Is.EqualTo("Couldn't load Trends"));
            Assert.That(cut.FindAll(Cards), Is.Empty);
            Assert.That(cut.FindAll("[data-testid=trends-loading]"), Is.Empty);
            Assert.That(cut.FindAll("[data-testid=trends-kind]"), Has.Count.EqualTo(1));
            Assert.That(retried, Is.EqualTo(1));
        });
    }

    [TestCase("Week", "week")]
    [TestCase("Month", "month")]
    [TestCase("Year", "year")]
    public void Stepper_NamesItsArrowsAfterTheKind(string sample, string unit)
    {
        var state = sample switch
        {
            "Week" => SampleTrends.Week,
            "Month" => SampleTrends.Month,
            _ => SampleTrends.Year,
        };
        var cut = Render(state);

        Assert.Multiple(() =>
        {
            Assert.That(cut.Find("[data-testid=trends-stepper-previous]").GetAttribute("aria-label"), Is.EqualTo($"Previous {unit}"));
            Assert.That(cut.Find("[data-testid=trends-stepper-next]").GetAttribute("aria-label"), Is.EqualTo($"Next {unit}"));
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
        cut.Find("[data-testid=add-button]").Click();
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
        Assert.That(Render(SampleTrends.Week).FindComponents<IconButton>().Single(b => b.Instance.TestId == "settings").Instance.Icon, Is.EqualTo(IconName.Settings));

    [Test]
    public void Week_DayColumn_RaisesOnOpenWorkoutsWithThatDay()
    {
        var opened = new List<DateSpan>();
        var cut = Context.Render<TrendsView>(p => p.Sample(SampleTrends.Week).Add(x => x.OnOpenWorkouts, opened.Add));

        cut.Find("[data-testid=trends-days-chart-bar-2026-11-18]").Click();

        Assert.That(opened, Is.EqualTo(new[] { new DateSpan(new DateOnly(2026, 11, 18), new DateOnly(2026, 11, 18)) }));
    }

    [Test]
    public void Month_CalendarDayAndWeekRow_RaiseOnOpenWorkoutsWithTheirDates()
    {
        var opened = new List<DateSpan>();
        var cut = Context.Render<TrendsView>(p => p.Sample(SampleTrends.Month).Add(x => x.OnOpenWorkouts, opened.Add));

        cut.Find("[data-testid=trends-calendar-days-day-2026-11-04]").Click();
        cut.Find("[data-testid=trends-weeks-rows-select-3]").Click();

        Assert.That(opened, Is.EqualTo(new[]
        {
            new DateSpan(new DateOnly(2026, 11, 4), new DateOnly(2026, 11, 4)),
            new DateSpan(new DateOnly(2026, 11, 16), new DateOnly(2026, 11, 18)),
        }));
    }

    [Test]
    public void Month_WeekRows_OnlyAWeekWithWorkoutsAndDatesIsTappableAndRaisesItsDates()
    {
        var week = new DateSpan(new DateOnly(2026, 11, 2), new DateOnly(2026, 11, 8));
        var opened = new List<DateSpan>();
        var cut = Context.Render<TrendsView>(p => p
            .Sample(With(SampleTrends.Month, nameof(TrendsView.Weeks), new[]
            {
                new MonthWeek("1 Nov", 0, TimeSpan.Zero, new DateSpan(new DateOnly(2026, 11, 1), new DateOnly(2026, 11, 1))),
                new MonthWeek("2 – 8 Nov", 3, new TimeSpan(2, 0, 0), week),
                new MonthWeek("9 – 15 Nov", 2, new TimeSpan(1, 0, 0)),
            }))
            .Add(x => x.OnOpenWorkouts, opened.Add));

        cut.Find("[data-testid=trends-weeks-rows-select-1]").Click();

        Assert.Multiple(() =>
        {
            Assert.That(cut.FindAll("[data-testid^=trends-weeks-rows-select-]").Select(b => b.GetAttribute("data-testid")), Is.EqualTo(new[] { "trends-weeks-rows-select-1" }));
            Assert.That(opened, Is.EqualTo(new[] { week }));
        });
    }

    [Test]
    public void Month_WeekRows_ShowDatesWorkoutsAndTimeInTheSingularForOne()
    {
        var cut = Context.Render<TrendsView>(p => p
            .Sample(With(SampleTrends.Month, nameof(TrendsView.Weeks), new[] { new MonthWeek("1 Nov", 1, new TimeSpan(0, 45, 0)), new MonthWeek("2 – 8 Nov", 4, new TimeSpan(4, 10, 0)) })));

        Assert.Multiple(() =>
        {
            Assert.That(cut.Find("[data-testid=trends-weeks-rows]").GetAttribute("aria-label"), Is.EqualTo("Workouts and time for each week"));
            Assert.That(cut.FindAll("[data-testid=trends-weeks-rows] .item .name").Select(n => n.TextContent), Is.EqualTo(new[] { "1 Nov", "2 – 8 Nov" }));
            Assert.That(cut.FindAll("[data-testid=trends-weeks-rows] .item .value").Select(v => v.TextContent), Is.EqualTo(new[] { "1 workout · 45m", "4 workouts · 4h 10m" }));
        });
    }

    [Test]
    public void Month_NoWeeks_ShowsTheMessage()
    {
        var cut = Context.Render<TrendsView>(p => p.Sample(With(SampleTrends.Month, nameof(TrendsView.Weeks), Array.Empty<MonthWeek>())));

        Assert.That(cut.Find("[data-testid=trends-weeks-rows-empty]").TextContent, Is.EqualTo("No weeks yet"));
    }

    [Test]
    public void Year_MonthColumn_RaisesOnOpenWorkoutsWithTheMonth()
    {
        var opened = new List<DateSpan>();
        var cut = Context.Render<TrendsView>(p => p.Sample(SampleTrends.Year).Add(x => x.OnOpenWorkouts, opened.Add));

        cut.Find("[data-testid=trends-months-chart-bar-2026-02-01]").Click();

        Assert.That(opened, Is.EqualTo(new[] { new DateSpan(new DateOnly(2026, 2, 1), new DateOnly(2026, 2, 28)) }));
    }

    [Test]
    public void NoOnOpenWorkouts_NothingInTheChartsCalendarOrWeeksIsTappable()
    {
        foreach (var state in new[] { SampleTrends.Week, SampleTrends.Month, SampleTrends.Year })
        {
            var cut = Render(state);

            Assert.That(cut.FindAll("[data-testid$=-open], [data-testid*=-select-], [data-testid*=chart-bar-][role=button]"), Is.Empty);
        }
    }

    [Test]
    public void Week_ChartsShareOneScaleTopAtTheBiggestValueOfBothWeeks() =>
        Assert.That(Render(SampleTrends.Week).FindComponent<TrendBarChart>().Instance.Max, Is.EqualTo(70));

    [Test]
    public void Year_ChartScaleTopsAtTheBusiestMonth() =>
        Assert.That(Render(SampleTrends.Year).FindComponent<TrendBarChart>().Instance.Max, Is.EqualTo(17));

    [Test]
    public void Week_NoValuesAtAll_ScaleTopIsOneAndTheLegendIsLeftOut()
    {
        var cut = Render(SampleTrends.EmptyWeek);

        Assert.Multiple(() =>
        {
            Assert.That(cut.FindComponent<TrendBarChart>().Instance.Max, Is.EqualTo(1));
            Assert.That(cut.FindAll("[data-testid=trends-days-chart-legend]"), Is.Empty);
        });
    }

    [Test]
    public void Month_MostTrainedRows_HaveSlugIdsAndRaiseTheExerciseOnSelect()
    {
        var opened = new List<int>();
        var cut = Context.Render<TrendsView>(p => p.Sample(SampleTrends.Month).Add(x => x.OnOpenRecord, opened.Add));

        cut.Find("[data-testid=trends-most-trained-rows-select-overhead-press]").Click();

        Assert.Multiple(() =>
        {
            Assert.That(cut.FindAll("[data-testid^=trends-most-trained-rows-row-]").Select(r => r.GetAttribute("data-testid")),
                Is.EqualTo(new[] { "bench-press", "squat", "deadlift", "treadmill-run", "overhead-press" }.Select(slug => $"trends-most-trained-rows-row-{slug}")));
            Assert.That(opened, Is.EqualTo(new[] { 9 }));
        });
    }

    [Test]
    public void Month_MostTrainedRowWithoutAnExercise_IsNotSelectable()
    {
        var rows = new MeterRow[] { new("Squat", 4, 22), new("Jan", 3), new("Deadlift", 2, 115) };
        var cut = Context.Render<TrendsView>(p => p.Sample(new Dictionary<string, object?>(SampleTrends.Month) { [nameof(TrendsView.MostTrained)] = rows }).Add(x => x.OnOpenRecord, _ => { }));

        Assert.That(cut.FindAll("[data-testid^=trends-most-trained-rows-select-]").Select(b => b.GetAttribute("data-testid")),
            Is.EqualTo(new[] { "trends-most-trained-rows-select-squat", "trends-most-trained-rows-select-deadlift" }));
    }

    [Test]
    public void Week_BarWithoutSpan_OpensNothing()
    {
        var opened = new List<DateSpan>();
        var cut = Context.Render<TrendsView>(p => p.Sample(SampleTrends.Week).Add(x => x.OnOpenWorkouts, opened.Add));

        cut.Find("[data-testid=trends-days-chart-bar-2026-11-20]").Click();

        Assert.That(opened, Is.Empty);
    }

    [Test]
    public void Month_MostTrainedRows_AreNotTappableWithoutOnOpenRecord() =>
        Assert.That(Render(SampleTrends.Month).FindAll("[data-testid^=trends-most-trained-rows-select-]"), Is.Empty);

    [Test]
    public void Month_MostTrainedAllAtZero_ShowsTheMessageInPlaceOfTheRows()
    {
        var cut = Render(new Dictionary<string, object?>(SampleTrends.Month) { [nameof(TrendsView.MostTrained)] = new MeterRow[] { new("Squat", 0, 22) } });

        Assert.Multiple(() =>
        {
            Assert.That(cut.Find("[data-testid=trends-most-trained-rows-all-zero]").TextContent, Is.EqualTo("No exercises this month"));
            Assert.That(cut.FindAll("[data-testid^=trends-most-trained-rows-row-]"), Is.Empty);
        });
    }

    [Test]
    public void Year_RecordsByMonthRows_HaveSlugIdsAndAreNotTappable()
    {
        var cut = Render(SampleTrends.Year);

        Assert.Multiple(() =>
        {
            Assert.That(cut.FindAll("[data-testid^=trends-records-by-month-rows-row-]").Select(r => r.GetAttribute("data-testid")).Take(3),
                Is.EqualTo(new[] { "jan", "feb", "mar" }.Select(slug => $"trends-records-by-month-rows-row-{slug}")));
            Assert.That(cut.FindAll("[data-testid^=trends-records-by-month-rows-select-]"), Is.Empty);
        });
    }

    private static Dictionary<string, object?> With(IReadOnlyDictionary<string, object?> state, string name, object value) =>
        new(state) { [name] = value };

    private IRenderedComponent<TrendsView> Render(IReadOnlyDictionary<string, object?> state) =>
        Context.Render<TrendsView>(p => p.Sample(state));
}
