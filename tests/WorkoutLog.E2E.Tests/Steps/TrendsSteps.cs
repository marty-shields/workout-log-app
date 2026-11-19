using System.Globalization;
using Reqnroll;
using WorkoutLog.E2E.Tests.Device;
using WorkoutLog.E2E.Tests.Screens;
using WorkoutLog.E2E.Tests.Seeding;
using WorkoutLog.E2E.Tests.Support;

namespace WorkoutLog.E2E.Tests.Steps;

/// <summary>Trends: its views and periods, and the days, week rows and bars that open Workouts.</summary>
[Binding]
public sealed class TrendsSteps(WebView web, Journeys journeys, TrendsScreen trends, TabBar tabs, DeviceClock clock)
{
    private string? noted;

    [Given(@"^I have opened Trends$")]
    [When(@"^I open Trends$")]
    public void OpenTrends() => journeys.GoToTrends();

    [Given(@"^I have chosen the (Week|Month|Year) view on Trends$")]
    [When(@"^I choose the (Week|Month|Year) view on Trends$")]
    public void WhenIChooseTheView(string kind)
    {
        trends.ChooseKind(kind);
        trends.WaitUntilLoaded();
    }

    [Then(@"^Trends is on the (Week|Month|Year) view$")]
    public void ThenTrendsShowsTheView(string kind) => web.Eventually(trends.Kind, Is.EqualTo(kind));

    [Given(@"^I have noted the period shown on Trends$")]
    [When(@"^I note the period shown on Trends$")]
    public void WhenINoteThePeriod() => noted = trends.Label();

    [Then(@"^Trends is on the period I noted$")]
    public void ThenTrendsShowsThePeriodINoted() => web.Eventually(trends.Label, Is.EqualTo(noted ?? throw new InvalidOperationException("No period was noted.")));

    [Then(@"^Trends is on a different period than the one I noted$")]
    public void ThenTrendsShowsADifferentPeriod() => web.Eventually(trends.Label, Is.Not.EqualTo(noted ?? throw new InvalidOperationException("No period was noted.")));

    [When(@"^I step back (\d+) periods? on Trends$")]
    public void WhenIStepBack(int times) => trends.StepBack(times);

    [When(@"^I step forward on Trends$")]
    public void WhenIStepForward()
    {
        var before = trends.Label();
        trends.Next();
        web.WaitUntil(() => trends.Label() != before, "the period didn't change");
    }

    [Then(@"^Trends is on the current period$")]
    public void ThenTrendsShowsTheCurrentPeriod() => web.Eventually(trends.CanGoNext, Is.False, "next arrow enabled");

    [Then(@"^Trends is on an earlier period$")]
    public void ThenTrendsShowsAnEarlierPeriod() => web.Eventually(trends.CanGoNext, Is.True, "next arrow enabled");

    /// <summary>Opens the Month view and steps back to the month holding the day, e.g. "40 days ago".</summary>
    [When(@"^I show the month of (.+) on Trends$")]
    public void WhenIShowTheMonthOf(string day) => OpenMonth(SeedTime.ResolveDay(day, clock.LocalNow()));

    [When(@"^I tap ""(.*)"" in Most trained on Trends$")]
    public void WhenITapMostTrained(string name)
    {
        web.WaitUntil(() => trends.HasMostTrained(name), $"{name} isn't among the most trained");
        trends.OpenMostTrained(name);
    }

    [Then(@"^the tab bar is (shown|hidden)$")]
    public void ThenTheTabBarIs(string shown) => web.Eventually(() => tabs.Has("Trends") && tabs.HasPlus(), Is.EqualTo(shown == "shown"), "tab bar shown");

    [Then(@"^Trends counts (\d+) workouts?$")]
    public void ThenTrendsCounts(int workouts) =>
        web.Eventually(trends.Workouts, Is.EqualTo(workouts.ToString(CultureInfo.InvariantCulture)), "Trends workouts");

    [Then(@"^no workout is saved$")]
    public void ThenNoWorkoutIsSaved()
    {
        journeys.GoToTrends();
        trends.ChooseKind("Year");
        trends.WaitUntilLoaded();
        Assert.Multiple(() =>
        {
            Assert.That(trends.Workouts(), Is.EqualTo("0"), "workouts this year");
            Assert.That(trends.OpenMonthColumns(), Is.Empty, "months with workouts");
        });
    }

    /// <summary>Taps the day in the Month view's calendar, stepping back to its month first.</summary>
    [When(@"^I open the workouts of (.+) from the Trends calendar$")]
    public void WhenIOpenADay(string day)
    {
        var date = SeedTime.ResolveDay(day, clock.LocalNow());
        OpenMonth(date);
        trends.OpenDay(date.Day);
    }

    /// <summary>Taps the row of the Weeks card that holds the day, stepping back to its month first.</summary>
    [When(@"^I open the workouts of the week of (.+) from the Trends weeks$")]
    public void WhenIOpenAWeek(string day)
    {
        var date = SeedTime.ResolveDay(day, clock.LocalNow());
        OpenMonth(date);
        trends.OpenWeekRow(WeekRow(date));
    }

    /// <summary>Taps the day's column in the Week view, stepping back to its week first.</summary>
    [When(@"^I open the workouts of (.+) from the Trends week view$")]
    public void WhenIOpenADayColumn(string day)
    {
        var date = SeedTime.ResolveDay(day, clock.LocalNow());
        Show("Week", Monday(clock.LocalNow()), Monday(date), 7);
        trends.OpenDayColumn(((int)date.DayOfWeek + 6) % 7 + 1);
    }

    /// <summary>Taps the month's column in the Year view, stepping back to its year first.</summary>
    [When(@"^I open the workouts of the month of (.+) from the Trends year view$")]
    public void WhenIOpenAMonthColumn(string day)
    {
        var date = SeedTime.ResolveDay(day, clock.LocalNow());
        trends.ChooseKind("Year");
        trends.WaitUntilLoaded();
        trends.StepBack(clock.LocalNow().Year - date.Year);
        trends.WaitUntilLoaded();
        trends.OpenMonthColumn(date.Month);
    }

    [Then(@"^the Trends calendar lets me open only the days (.+)$")]
    public void ThenTheCalendarOpens(string days)
    {
        var now = clock.LocalNow();
        var expected = days.Split(", ").Select(d => SeedTime.ResolveDay(d, now).Day).Order().ToList();
        web.Eventually(() => trends.OpenDays().Order().ToList(), Is.EqualTo(expected));
    }

    [Then(@"^the Trends week view lets me open only (.+)$")]
    public void ThenTheWeekViewOpens(string days)
    {
        var now = clock.LocalNow();
        var expected = days.Split(", ").Select(d => ((int)SeedTime.ResolveDay(d, now).DayOfWeek + 6) % 7 + 1).Order().ToList();
        web.Eventually(() => trends.OpenDayColumns().Order().ToList(), Is.EqualTo(expected));
    }

    [When(@"^I tap Settings on Trends$")]
    public void WhenITapSettingsOnTrends()
    {
        journeys.WaitFor(Journeys.Screen.Trends);
        trends.OpenSettings();
    }

    /// <summary>Opens the Month view and steps back from this month to the month holding <paramref name="date"/>; does not step when it already shows that month.</summary>
    private void OpenMonth(DateTime date)
    {
        var now = clock.LocalNow();
        trends.ChooseKind("Month");
        trends.WaitUntilLoaded();
        if (trends.Label() != date.ToString("MMMM yyyy", CultureInfo.InvariantCulture))
        {
            trends.StepBack((now.Year - date.Year) * 12 + now.Month - date.Month);
        }

        trends.WaitUntilLoaded();
    }

    private void Show(string kind, DateTime current, DateTime wanted, int daysPerStep)
    {
        trends.ChooseKind(kind);
        trends.WaitUntilLoaded();
        trends.StepBack((int)Math.Round((current - wanted).TotalDays / daysPerStep));
        trends.WaitUntilLoaded();
    }

    /// <summary>The row of the Weeks card for the day: one row per week of the month, the first cut at the 1st.</summary>
    private static int WeekRow(DateTime date) =>
        1 + Enumerable.Range(2, date.Day - 1).Count(d => new DateTime(date.Year, date.Month, d).DayOfWeek == DayOfWeek.Monday);

    private static DateTime Monday(DateTime day) => day.Date.AddDays(-(((int)day.DayOfWeek + 6) % 7));
}
