using Reqnroll;
using WorkoutLog.E2E.Tests.Screens;
using WorkoutLog.E2E.Tests.Support;

namespace WorkoutLog.E2E.Tests.Steps;

/// <summary>The Workout Summary shown after saving a new workout.</summary>
[Binding]
public sealed class SummarySteps(WebView web, Journeys journeys, WorkoutScreen workout, SummaryScreen summary)
{
    [When(@"^I save the new workout$")]
    public void WhenISaveTheNewWorkout()
    {
        workout.Save();
        journeys.WaitFor(Journeys.Screen.Summary);
        summary.WaitUntilLoaded();
    }

    [When(@"^I tap Done$")]
    public void WhenITapDone() => summary.Done();

    [Then(@"^I am on the Workout Summary$")]
    public void ThenIAmOnTheWorkoutSummary()
    {
        journeys.WaitFor(Journeys.Screen.Summary);
        summary.WaitUntilLoaded();
    }

    [Then(@"^the summary shows (\d+) exercises? and (\d+) sets?$")]
    public void ThenTheSummaryShows(string exercises, string sets)
    {
        web.Eventually(summary.Exercises, Is.EqualTo(exercises));
        web.Eventually(summary.Sets, Is.EqualTo(sets));
    }

    [Then(@"^the summary says it is my first workout$")]
    public void ThenTheSummarySaysFirstWorkout() => web.Eventually(summary.HasFirstWorkoutNote, Is.True);

    [Then(@"^the summary lists a record for ""(.*)""$")]
    public void ThenTheSummaryListsARecord(string name) =>
        web.Eventually(() => summary.Record(Exercises.Named(name).Id), Is.Not.Null);

    [Then(@"^the summary lists no record for ""(.*)""$")]
    public void ThenTheSummaryListsNoRecord(string name) =>
        web.Eventually(() => summary.Record(Exercises.Named(name).Id), Is.Null);

    [Then(@"^""(.*)"" changed by ""(.*)"" since last time$")]
    public void ThenChangedSinceLastTime(string name, string difference) =>
        web.Eventually(() => summary.Change(Exercises.Named(name).Id) ?? "", Does.Contain(difference));
}
