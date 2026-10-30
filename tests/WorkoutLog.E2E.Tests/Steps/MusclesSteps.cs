using Reqnroll;
using WorkoutLog.E2E.Tests.Screens;
using WorkoutLog.E2E.Tests.Support;

namespace WorkoutLog.E2E.Tests.Steps;

/// <summary>Sets per muscle group, opened from the Dashboard.</summary>
[Binding]
public sealed class MusclesSteps(WebView web, Journeys journeys, MusclesScreen muscles)
{
    [Given(@"^I have opened Sets per muscle group from the Dashboard$")]
    [When(@"^I open Sets per muscle group from the Dashboard$")]
    public void OpenMuscles() => journeys.GoToMuscles();

    /// <summary>Rows with columns group and sets, e.g. "12 sets" or "Not trained".</summary>
    [Then(@"^Sets per muscle group reads, in order:$")]
    public void ThenTheRowsRead(DataTable table) =>
        web.Eventually(muscles.Rows, Is.EqualTo(table.Rows.Select(r => (r["group"], r["sets"]))));
}
