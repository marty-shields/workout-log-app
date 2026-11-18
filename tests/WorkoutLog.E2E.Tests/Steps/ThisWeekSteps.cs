using System.Globalization;
using Reqnroll;
using WorkoutLog.E2E.Tests.Device;
using WorkoutLog.E2E.Tests.Screens;
using WorkoutLog.E2E.Tests.Seeding;
using WorkoutLog.E2E.Tests.Support;

namespace WorkoutLog.E2E.Tests.Steps;

[Binding]
public sealed class ThisWeekSteps(WebView web, ThisWeekScreen thisWeek, NavigationBar navigation, Journeys journeys, DeviceClock clock, ScenarioState state)
{
    private const string NoWorkoutsText = "Nothing yet this week. Tap + to start one.";

    [Then(@"^I am on This week titled ""(.*)""$")]
    public void ThenOnThisWeekTitled(string title) => web.Eventually(() => web.Text("screen-title"), Is.EqualTo(title));

    /// <summary>Rows with columns started, exercises and duration, newest first as the card shows them. A started time is read as the seed steps read it.</summary>
    [Then(@"^this week's workouts read, in order:$")]
    public void ThenTheWorkoutsRead(DataTable table)
    {
        journeys.WaitFor(Journeys.Screen.ThisWeek);
        var expected = table.Rows.Select(r => (When(r["started"]), r["exercises"], r["duration"])).ToList();
        web.Eventually(thisWeek.Workouts, Is.EqualTo(expected));
    }

    [Then(@"^this week shows no workouts$")]
    public void ThenThisWeekShowsNoWorkouts()
    {
        journeys.WaitFor(Journeys.Screen.ThisWeek);
        web.Eventually(thisWeek.NoWorkoutsText, Is.EqualTo(NoWorkoutsText));
    }

    [Then(@"^this week doesn't list the workout started ""(.*)""$")]
    public void ThenThisWeekDoesNotList(string started)
    {
        journeys.WaitFor(Journeys.Screen.ThisWeek);
        thisWeek.WaitUntilLoaded();
        Assert.That(thisWeek.Workouts().Select(w => w.Started), Does.Not.Contain(When(started)));
    }

    [Then(@"^this week's newest workout is from today$")]
    public void ThenTheNewestWorkoutIsFromToday()
    {
        journeys.WaitFor(Journeys.Screen.ThisWeek);
        var (from, to) = state.Started ?? throw new InvalidOperationException("No workout was started.");
        web.Eventually(() => thisWeek.Workouts().Select(w => w.Started).FirstOrDefault()?[..Day(from).Length], Is.AnyOf(Day(from), Day(to)));
    }

    [When(@"^I tap the newest workout on This week$")]
    public void WhenITapTheNewestWorkout()
    {
        journeys.WaitFor(Journeys.Screen.ThisWeek);
        thisWeek.OpenNewestWorkout();
    }

    [Then(@"^this week shows (\d+) workouts?$")]
    public void ThenThisWeekShows(int workouts)
    {
        journeys.GoToThisWeek();
        Assert.That(thisWeek.Tile("workouts").Value, Is.EqualTo(workouts.ToString(CultureInfo.InvariantCulture)), "This week");
    }

    /// <summary>Rows with columns row (Workouts, Time or Sets), this week and change, as the tiles show them against last week up to the same weekday.</summary>
    [Then(@"^This week vs last week shows:$")]
    public void ThenTheComparisonShows(DataTable table)
    {
        journeys.GoToThisWeek();
        Assert.Multiple(() =>
        {
            foreach (var row in table.Rows)
            {
                Assert.That(thisWeek.Tile(row["row"].ToLowerInvariant()), Is.EqualTo((row["this week"], row["change"])), row["row"]);
            }
        });
    }

    [Then(@"^the streak shows ""(.*)""$")]
    public void ThenTheStreakShows(string streak)
    {
        journeys.GoToThisWeek();
        Assert.That(thisWeek.Streak(), Is.EqualTo(streak));
    }

    [Then(@"^no streak is shown$")]
    public void ThenNoStreak()
    {
        journeys.GoToThisWeek();
        Assert.That(thisWeek.Streak(), Is.Null);
    }

    [Then(@"^the day strip marks today as trained$")]
    public void ThenTheStripMarksToday()
    {
        journeys.GoToThisWeek();
        Assert.That(thisWeek.TrainedDays(), Is.EqualTo(new[] { clock.LocalNow().DayOfWeek.ToString() }));
    }

    [Then(@"^the day strip marks no day as trained$")]
    public void ThenTheStripMarksNoDay()
    {
        journeys.GoToThisWeek();
        Assert.That(thisWeek.TrainedDays(), Is.Empty);
    }

    [Then(@"^the Last week row shows ""(.*)""$")]
    public void ThenTheLastWeekRowShows(string totals)
    {
        journeys.GoToThisWeek();
        Assert.That(thisWeek.LastWeek(), Is.EqualTo(totals));
    }

    [Then(@"^no Last week row is shown$")]
    public void ThenNoLastWeekRow()
    {
        journeys.GoToThisWeek();
        Assert.That(thisWeek.LastWeek(), Is.Null);
    }

    /// <summary>Rows with columns group and sets, e.g. "12 sets" or "Not trained", most sets first.</summary>
    [Then(@"^Sets per muscle group reads, in order:$")]
    public void ThenTheMusclesRead(DataTable table)
    {
        journeys.WaitFor(Journeys.Screen.ThisWeek);
        web.Eventually(thisWeek.MuscleRows, Is.EqualTo(table.Rows.Select(r => (r["group"], r["sets"])).ToList()));
    }

    [Then(@"^I can open Workouts and Exercises and start a New Workout$")]
    public void ThenWorkoutsExercisesAndNewWorkoutShow() => Assert.Multiple(() =>
    {
        Assert.That(navigation.Has("Workouts"), Is.True, "Workouts");
        Assert.That(navigation.Has("Exercises"), Is.True, "Exercises");
        Assert.That(thisWeek.HasNewWorkout(), Is.True, "New Workout");
    });

    [Then(@"^no startup error is shown$")]
    public void ThenNoStartupError() => Assert.That(thisWeek.HasStartupError(), Is.False);

    [When(@"^I tap (Workouts|New Workout) on This week$")]
    public void WhenITapOnThisWeek(string link)
    {
        journeys.WaitFor(Journeys.Screen.ThisWeek);
        if (link == "Workouts")
        {
            navigation.Open("Workouts");
        }
        else
        {
            thisWeek.NewWorkout();
        }
    }

    [When(@"^I go back to This week$")]
    public void WhenIGoBackToThisWeek() => journeys.GoToThisWeek();

    private static string Day(DateTime value) => value.ToString("ddd d MMM", CultureInfo.InvariantCulture);

    internal static string Date(DateTime value) => value.ToString("ddd d MMM yyyy", CultureInfo.InvariantCulture);

    /// <summary>A seed time as the workouts card shows it, e.g. "today 00:00" as "Wed 18 Nov · 00:00".</summary>
    private string When(string time) => SeedTime.Resolve(time, clock.LocalNow()).ToString("ddd d MMM · HH:mm", CultureInfo.InvariantCulture);
}
