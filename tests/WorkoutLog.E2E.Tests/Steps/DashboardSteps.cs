using System.Globalization;
using Reqnroll;
using WorkoutLog.E2E.Tests.Device;
using WorkoutLog.E2E.Tests.Screens;
using WorkoutLog.E2E.Tests.Seeding;
using WorkoutLog.E2E.Tests.Support;

namespace WorkoutLog.E2E.Tests.Steps;

[Binding]
public sealed class DashboardSteps(WebView web, DashboardScreen dashboard, Journeys journeys, DeviceClock clock, ScenarioState state)
{
    [Then(@"^I am on the Dashboard titled ""(.*)""$")]
    public void ThenOnTheDashboardTitled(string title) => web.Eventually(() => web.Text("screen-title"), Is.EqualTo(title));

    [Then(@"^the last workout shows ""(.*)""$")]
    public void ThenTheLastWorkoutShows(string text)
    {
        journeys.WaitFor(Journeys.Screen.Dashboard);
        web.Eventually(dashboard.LastWorkout, Is.EqualTo(text));
    }

    [Then(@"^the last workout shows the date and day of ""(.*)""$")]
    public void ThenTheLastWorkoutShowsTheDateOf(string day) =>
        ThenTheLastWorkoutShows(Date(SeedTime.ResolveDay(day, clock.LocalNow())));

    [Then(@"^the last workout shows today's date and day$")]
    public void ThenTheLastWorkoutShowsToday()
    {
        journeys.WaitFor(Journeys.Screen.Dashboard);
        var (from, to) = state.Started ?? throw new InvalidOperationException("No workout was started.");
        web.Eventually(dashboard.LastWorkout, Is.AnyOf(Date(from), Date(to)));
    }

    [Then(@"^the last workout's time shows ""(.*)""$")]
    public void ThenTheLastWorkoutsTimeShows(string text)
    {
        dashboard.LastWorkout();
        Assert.That(dashboard.LastWorkoutWhen(), Is.EqualTo(text));
    }

    [Then(@"^the last workout's exercises show ""(.*)""$")]
    public void ThenTheLastWorkoutsExercisesShow(string text)
    {
        dashboard.LastWorkout();
        Assert.That(dashboard.LastWorkoutExercises(), Is.EqualTo(text));
    }

    [When(@"^I tap the last workout on the Dashboard$")]
    public void WhenITapTheLastWorkout()
    {
        journeys.WaitFor(Journeys.Screen.Dashboard);
        dashboard.LastWorkout();
        dashboard.OpenLastWorkout();
    }

    [Then(@"^this week shows (\d+) workouts?$")]
    public void ThenThisWeekShows(int workouts)
    {
        journeys.GoToDashboard();
        dashboard.LastWorkout();
        Assert.That(dashboard.WeekRow("workouts").ThisWeek, Is.EqualTo(workouts.ToString(CultureInfo.InvariantCulture)), "This week");
    }

    [Then(@"^This week vs last week shows:$")]
    public void ThenTheComparisonShows(DataTable table)
    {
        journeys.GoToDashboard();
        dashboard.LastWorkout();
        Assert.Multiple(() =>
        {
            foreach (var row in table.Rows)
            {
                Assert.That(dashboard.WeekRow(row["row"].ToLowerInvariant()), Is.EqualTo((row["this week"], row["last week"], row["change"])), row["row"]);
            }
        });
    }

    [Then(@"^the last 12 weeks show:$")]
    public void ThenTheWeeksShow(DataTable table)
    {
        journeys.GoToDashboard();
        dashboard.LastWorkout();
        var bars = dashboard.WeekBars();
        Assert.Multiple(() =>
        {
            Assert.That(bars, Has.Count.EqualTo(12), "weeks");
            foreach (var row in table.Rows)
            {
                Assert.That(bars, Does.Contain($"{row["week"]}: {row["workouts"]}"));
            }
        });
    }

    [Then(@"^the streak shows ""(.*)""$")]
    public void ThenTheStreakShows(string streak)
    {
        journeys.GoToDashboard();
        dashboard.LastWorkout();
        Assert.That(dashboard.Streak(), Is.EqualTo(streak));
    }

    [Then(@"^no streak is shown$")]
    public void ThenNoStreak()
    {
        journeys.GoToDashboard();
        dashboard.LastWorkout();
        Assert.That(dashboard.Streak(), Is.Null);
    }

    [Then(@"^I can open Workouts and Exercises and start a New Workout$")]
    public void ThenWorkoutsExercisesAndNewWorkoutShow() => Assert.Multiple(() =>
    {
        Assert.That(dashboard.HasWorkoutsLink(), Is.True, "Workouts");
        Assert.That(dashboard.HasExercisesLink(), Is.True, "Exercises");
        Assert.That(dashboard.HasNewWorkout(), Is.True, "New Workout");
    });

    [Then(@"^no startup error is shown$")]
    public void ThenNoStartupError() => Assert.That(dashboard.HasStartupError(), Is.False);

    [When(@"^I tap (Workouts|New Workout) on the Dashboard$")]
    public void WhenITapOnTheDashboard(string link)
    {
        journeys.WaitFor(Journeys.Screen.Dashboard);
        if (link == "Workouts")
        {
            dashboard.OpenWorkouts();
        }
        else
        {
            dashboard.NewWorkout();
        }
    }

    [When(@"^I go back to the Dashboard$")]
    public void WhenIGoBackToTheDashboard() => journeys.GoToDashboard();

    [Then(@"^on the Dashboard the last workout shows ""(.*)"" and this week shows (\d+) workouts?$")]
    public void ThenOnTheDashboard(string lastWorkout, int workouts)
    {
        journeys.GoToDashboard();
        ThenTheLastWorkoutShows(lastWorkout);
        ThenThisWeekShows(workouts);
    }

    internal static string Date(DateTime value) => value.ToString("ddd d MMM yyyy", CultureInfo.InvariantCulture);
}
