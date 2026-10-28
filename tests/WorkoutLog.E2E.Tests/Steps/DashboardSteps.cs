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

    [Then(@"^This week shows (\d+) and This month shows (\d+)$")]
    public void ThenTheCounts(int week, int month)
    {
        journeys.WaitFor(Journeys.Screen.Dashboard);
        dashboard.LastWorkout();
        Assert.Multiple(() =>
        {
            Assert.That(dashboard.ThisWeek(), Is.EqualTo(week.ToString(CultureInfo.InvariantCulture)), "This week");
            Assert.That(dashboard.ThisMonth(), Is.EqualTo(month.ToString(CultureInfo.InvariantCulture)), "This month");
        });
    }

    [Then(@"^This week shows (\d+)$")]
    public void ThenThisWeekShows(int week)
    {
        journeys.GoToDashboard();
        Assert.That(dashboard.ThisWeek(), Is.EqualTo(week.ToString(CultureInfo.InvariantCulture)), "This week");
    }

    [Then(@"^This month counts it only if it started this month$")]
    public void ThenThisMonthCountsItIfThisMonth()
    {
        var now = clock.LocalNow();
        var started = state.Seeded[1];
        var expected = started.Year == now.Year && started.Month == now.Month ? "1" : "0";
        Assert.That(dashboard.ThisMonth(), Is.EqualTo(expected), $"This month, for a workout on {started:d MMM}");
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

    [Then(@"^on the Dashboard the last workout shows ""(.*)"", This week (\d+) and This month (\d+)$")]
    public void ThenOnTheDashboard(string lastWorkout, int week, int month)
    {
        journeys.GoToDashboard();
        ThenTheLastWorkoutShows(lastWorkout);
        ThenTheCounts(week, month);
    }

    internal static string Date(DateTime value) => value.ToString("ddd d MMM yyyy", CultureInfo.InvariantCulture);
}
