using System.Globalization;
using Reqnroll;
using WorkoutLog.E2E.Tests.Device;
using WorkoutLog.E2E.Tests.Screens;
using WorkoutLog.E2E.Tests.Seeding;
using WorkoutLog.E2E.Tests.Support;

namespace WorkoutLog.E2E.Tests.Steps;

/// <summary>An exercise's Exercise page, opened from the Exercises list.</summary>
[Binding]
public sealed class ExerciseSteps(WebView web, Journeys journeys, ExercisesScreen exercises, ExerciseScreen exercise, DeviceClock clock)
{
    [Then(@"^the tappable rows on Exercises are: (.+)$")]
    public void ThenTheTappableRowsAre(string names) => web.Eventually(exercises.TappableNames, Is.EqualTo(names.Split(", ")));

    [Given(@"^I have opened the Exercise page for ""(.*)""$")]
    [When(@"^I open ""(.*)"" on Exercises$")]
    public void OpenExercise(string name)
    {
        if (journeys.Current() != Journeys.Screen.Exercises)
        {
            journeys.GoToExercises();
        }

        exercises.Open(Exercises.Named(name).Id);
        web.WaitUntil(exercise.IsShown, "the Exercise page didn't show");
        exercise.WaitUntilLoaded();
    }

    [Then(@"^the Exercise page is titled ""(.*)"" with ""(.*)""$")]
    public void ThenTheTitleIs(string title, string subtitle)
    {
        web.Eventually(exercise.Title, Is.EqualTo(title));
        Assert.That(exercise.Subtitle(), Is.EqualTo(subtitle));
    }

    [When(@"^I choose the (Best set|Total|Top set|e1RM|Duration|Weight|Distance) tab$")]
    public void WhenIChooseTheTab(string tab) => exercise.Tab(tab.ToLowerInvariant().Replace(' ', '-'));

    [Then(@"^the Exercise page has no tab switcher$")]
    public void ThenNoTabSwitcher() => web.Eventually(exercise.HasTabs, Is.False);

    [Then(@"^the Exercise chart is titled ""(.*)""$")]
    public void ThenTheChartIsTitled(string title) => web.Eventually(exercise.ChartTitle, Is.EqualTo(title));

    [Then(@"^the Exercise tabs are: (.+)$")]
    public void ThenTheTabsAre(string labels) => web.Eventually(exercise.Tabs, Is.EqualTo(labels.Split(", ")));

    [When(@"^I choose the (3M|6M|1Y|All) range$")]
    public void WhenIChooseTheRange(string range) => exercise.Range(range.ToLowerInvariant());

    /// <summary>Points with columns day, as seed steps write it, and value, e.g. 70kg ×3; oldest first.</summary>
    [Then(@"^the Exercise chart reads:$")]
    public void ThenTheChartReads(DataTable table)
    {
        var now = clock.LocalNow();
        web.Eventually(exercise.ChartValues, Is.EqualTo(table.Rows.Select(r => $"{Day(r["day"], now)}: {r["value"]}")));
    }

    [Then(@"^the Exercise chart says ""(.*)""$")]
    public void ThenTheChartSays(string text) => web.Eventually(exercise.NoDataText, Is.EqualTo(text));

    [Then(@"^the Exercise caption is ""(.*)""$")]
    public void ThenTheCaptionIs(string text) => web.Eventually(exercise.Caption, Is.EqualTo(text));

    /// <summary>Records with columns record, value and day; — and an empty day for none.</summary>
    [Then(@"^the Exercise records read:$")]
    public void ThenTheRecordsRead(DataTable table)
    {
        var now = clock.LocalNow();
        web.Eventually(exercise.Records,
            Is.EqualTo(table.Rows.Select(r => (r["record"], r["value"], r["day"].Length == 0 ? "" : Day(r["day"], now)))));
    }

    /// <summary>Sessions with columns day, headline and sets; newest first.</summary>
    [Then(@"^the Exercise sessions read:$")]
    public void ThenTheSessionsRead(DataTable table)
    {
        var now = clock.LocalNow();
        web.Eventually(exercise.Sessions, Is.EqualTo(table.Rows.Select(r => (ShortDate(r["day"], now), r["headline"], r["sets"]))));
    }

    [Then(@"^the Exercise page lists no sessions$")]
    public void ThenNoSessions() => web.Eventually(exercise.Sessions, Is.Empty);

    /// <summary>The day as the chart and records show it, e.g. 20 Oct, with the year only when it isn't this year.</summary>
    private static string Day(string text, DateTime now)
    {
        var day = SeedTime.ResolveDay(text, now);
        return day.ToString(day.Year == now.Year ? "d MMM" : "d MMM yyyy", CultureInfo.InvariantCulture);
    }

    /// <summary>The day as the sessions show it, e.g. Tue 20 Oct, with the year only when it isn't this year.</summary>
    private static string ShortDate(string text, DateTime now)
    {
        var day = SeedTime.ResolveDay(text, now);
        return day.ToString(day.Year == now.Year ? "ddd d MMM" : "ddd d MMM yyyy", CultureInfo.InvariantCulture);
    }
}
