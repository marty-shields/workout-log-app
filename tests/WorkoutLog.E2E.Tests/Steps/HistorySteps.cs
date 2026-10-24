using System.Globalization;
using Reqnroll;
using WorkoutLog.E2E.Tests.Device;
using WorkoutLog.E2E.Tests.Screens;
using WorkoutLog.E2E.Tests.Seeding;
using WorkoutLog.E2E.Tests.Support;

namespace WorkoutLog.E2E.Tests.Steps;

[Binding]
public sealed class HistorySteps(WebView web, Journeys journeys, HistoryScreen history, WorkoutScreen workout, DeviceClock clock, ScenarioState state)
{
    [Given(@"^I have opened History$")]
    [When(@"^I open History$")]
    public void OpenHistory() => journeys.GoToHistory();

    [Then(@"^History shows ""(.*)""$")]
    public void ThenHistoryShows(string text) => web.Eventually(history.EmptyText, Is.EqualTo(text));

    [Then(@"^History is empty$")]
    public void ThenHistoryIsEmpty()
    {
        journeys.GoToHistory();
        ThenHistoryShows("No workouts yet");
    }

    [Then(@"^the rows read, in order:$")]
    public void ThenTheRowsRead(DataTable rows)
    {
        var now = clock.LocalNow();
        var expected = new[] { rows.Header.Single() }.Concat(rows.Rows.Select(r => r[0])).Select(t => DateAndTime(SeedTime.Resolve(t, now)));
        web.Eventually(history.Rows, Is.EqualTo(expected));
    }

    [Then(@"^workout (\d+) is above workout (\d+)$")]
    public void ThenWorkoutIsAbove(int above, int below)
    {
        var ids = history.RowIds().ToList();
        Assert.That(ids.IndexOf(above), Is.LessThan(ids.IndexOf(below)).And.Not.Negative, $"rows {string.Join(", ", ids)}");
    }

    [Then(@"^History lists (\d+) workouts?$")]
    public void ThenHistoryLists(int count)
    {
        journeys.GoToHistory();
        web.Eventually(() => history.Rows().Count, Is.EqualTo(count), "History rows");
    }

    [Then(@"^History lists (\d+) workouts, newest first$")]
    public void ThenHistoryListsNewestFirst(int count)
    {
        ThenHistoryLists(count);
        Assert.That(history.RowIds(), Is.Ordered.Descending, "newest saved first");
    }

    [Then(@"^History lists 1 workout, today at the time I started$")]
    public void ThenHistoryListsTodayAtTheStart()
    {
        ThenHistoryLists(1);
        var (from, to) = state.Started ?? throw new InvalidOperationException("No workout was started.");
        Assert.That(history.Rows()[0], Is.AnyOf(DateAndTime(from), DateAndTime(to)));
    }

    [Then(@"^History lists only workout (\d+)$")]
    public void ThenHistoryListsOnly(int id)
    {
        journeys.GoToHistory();
        web.Eventually(history.RowIds, Is.EqualTo(new[] { id }));
    }

    [Then(@"^History still lists (\d+) workouts$")]
    public void ThenHistoryStillLists(int count) => ThenHistoryLists(count);

    [Then(@"^History still shows workout (\d+) at (.+)$")]
    public void ThenHistoryStillShowsWorkoutAt(int id, string time)
    {
        journeys.GoToHistory();
        var row = history.RowIds().ToList().IndexOf(id);
        Assert.That(row, Is.Not.Negative, $"no row for workout {id}");
        Assert.That(history.Rows()[row], Is.EqualTo(DateAndTime(SeedTime.Resolve(time, clock.LocalNow()))));
    }

    [When(@"^I tap the row for (.+)$")]
    public void WhenITapTheRowFor(string time)
    {
        var text = DateAndTime(SeedTime.Resolve(time, clock.LocalNow()));
        var index = history.Rows().ToList().IndexOf(text);
        Assert.That(index, Is.Not.Negative, $"no row reads {text}");
        history.Open(history.RowIds()[index]);
    }

    [Then(@"^I am on Edit Workout titled with yesterday's date and day$")]
    public void ThenOnEditWorkoutTitledYesterday()
    {
        journeys.WaitFor(Journeys.Screen.Workout);
        Assert.That(workout.IsEdit(), Is.True, "Edit Workout");
        web.Eventually(workout.Title, Is.EqualTo(DashboardSteps.Date(clock.LocalNow().Date.AddDays(-1))));
    }

    private static string DateAndTime(DateTime value) => value.ToString("ddd d MMM yyyy · HH:mm", CultureInfo.InvariantCulture);
}
