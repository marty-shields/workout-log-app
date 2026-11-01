using Reqnroll;
using WorkoutLog.E2E.Tests.Device;
using WorkoutLog.E2E.Tests.Screens;
using WorkoutLog.E2E.Tests.Support;

namespace WorkoutLog.E2E.Tests.Steps;

/// <summary>The Dashboard's Recent records card.</summary>
[Binding]
public sealed class RecentRecordsSteps(WebView web, Journeys journeys, DashboardScreen dashboard, ExerciseScreen exercise)
{
    [Then(@"^Recent records reads, in order:$")]
    public void ThenRecentRecordsReads(DataTable table)
    {
        journeys.WaitFor(Journeys.Screen.Dashboard);
        web.Eventually(dashboard.Records, Is.EqualTo(table.Rows.Select(r => (r["record"], r["when"])).ToList()));
    }

    [Then(@"^Recent records says ""(.*)""$")]
    public void ThenRecentRecordsSays(string text)
    {
        journeys.WaitFor(Journeys.Screen.Dashboard);
        web.Eventually(dashboard.NoRecordsText, Is.EqualTo(text));
    }

    [When(@"^I open ""(.*)"" in Recent records$")]
    public void WhenIOpenARecord(string name)
    {
        journeys.WaitFor(Journeys.Screen.Dashboard);
        dashboard.Records();
        dashboard.OpenRecord(Exercises.Named(name).Id);
        web.WaitUntil(exercise.IsShown, "the Exercise page didn't show");
        exercise.WaitUntilLoaded();
    }
}
