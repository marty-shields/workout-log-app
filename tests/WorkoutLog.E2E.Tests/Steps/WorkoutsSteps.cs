using System.Globalization;
using Reqnroll;
using WorkoutLog.E2E.Tests.Device;
using WorkoutLog.E2E.Tests.Screens;
using WorkoutLog.E2E.Tests.Seeding;
using WorkoutLog.E2E.Tests.Support;

namespace WorkoutLog.E2E.Tests.Steps;

[Binding]
public sealed class WorkoutsSteps(WebView web, Journeys journeys, WorkoutsScreen workouts, WorkoutScreen workout, DeviceClock clock, ScenarioState state)
{
    [Given(@"^I have opened Workouts$")]
    [When(@"^I open Workouts$")]
    public void OpenWorkouts() => journeys.GoToWorkouts();

    [Then(@"^Workouts shows ""(.*)""$")]
    public void ThenWorkoutsShows(string text) => web.Eventually(workouts.EmptyText, Is.EqualTo(text));

    [Then(@"^Workouts is empty$")]
    public void ThenWorkoutsIsEmpty()
    {
        journeys.GoToWorkouts();
        ThenWorkoutsShows("No workouts yet");
    }

    [Then(@"^the rows read, in order:$")]
    public void ThenTheRowsRead(DataTable rows)
    {
        var now = clock.LocalNow();
        var expected = new[] { rows.Header.Single() }.Concat(rows.Rows.Select(r => r[0])).Select(t => DateAndTime(SeedTime.Resolve(t, now)));
        web.Eventually(workouts.Rows, Is.EqualTo(expected));
    }

    [Then(@"^Workouts lists (\d+) workouts?$")]
    public void ThenWorkoutsLists(int count)
    {
        journeys.GoToWorkouts();
        web.Eventually(() => workouts.Rows().Count, Is.EqualTo(count), "Workouts rows");
    }

    [Then(@"^Workouts lists (\d+) workouts, newest first$")]
    public void ThenWorkoutsListsNewestFirst(int count)
    {
        ThenWorkoutsLists(count);
        Assert.That(workouts.RowIds(), Is.Ordered.Descending, "newest saved first");
    }

    [Then(@"^Workouts lists 1 workout, today at the time I started$")]
    public void ThenWorkoutsListsTodayAtTheStart()
    {
        ThenWorkoutsLists(1);
        var (from, to) = state.Started ?? throw new InvalidOperationException("No workout was started.");
        Assert.That(workouts.Rows()[0], Is.AnyOf(DateAndTime(from), DateAndTime(to)));
    }

    [Then(@"^Workouts lists only workout (\d+)$")]
    public void ThenWorkoutsListsOnly(int id)
    {
        journeys.GoToWorkouts();
        web.Eventually(workouts.RowIds, Is.EqualTo(new[] { id }));
    }

    [Then(@"^Workouts still lists (\d+) workouts$")]
    public void ThenWorkoutsStillLists(int count) => ThenWorkoutsLists(count);

    [Then(@"^Workouts still shows workout (\d+) at (.+)$")]
    public void ThenWorkoutsStillShowsWorkoutAt(int id, string time)
    {
        journeys.GoToWorkouts();
        var row = workouts.RowIds().ToList().IndexOf(id);
        Assert.That(row, Is.Not.Negative, $"no row for workout {id}");
        Assert.That(workouts.Rows()[row], Is.EqualTo(DateAndTime(SeedTime.Resolve(time, clock.LocalNow()))));
    }

    [When(@"^I tap the row for (.+)$")]
    public void WhenITapTheRowFor(string time)
    {
        var text = DateAndTime(SeedTime.Resolve(time, clock.LocalNow()));
        var index = workouts.Rows().ToList().IndexOf(text);
        Assert.That(index, Is.Not.Negative, $"no row reads {text}");
        workouts.Open(workouts.RowIds()[index]);
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
