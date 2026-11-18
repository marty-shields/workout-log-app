using Reqnroll;
using WorkoutLog.E2E.Tests.Device;
using WorkoutLog.E2E.Tests.Screens;
using WorkoutLog.E2E.Tests.Support;

namespace WorkoutLog.E2E.Tests.Steps;

/// <summary>The Records this week card on This week.</summary>
[Binding]
public sealed class RecentRecordsSteps(WebView web, Journeys journeys, ThisWeekScreen thisWeek, ExerciseScreen exercise, DeviceClock clock)
{
    /// <summary>Rows with columns record and day; "today" is the weekday of the device's today.</summary>
    [Then(@"^Records this week reads, in order:$")]
    public void ThenRecordsReads(DataTable table)
    {
        journeys.WaitFor(Journeys.Screen.ThisWeek);
        var expected = table.Rows.Select(r => (r["record"], r["day"] == "today" ? clock.LocalNow().DayOfWeek.ToString() : r["day"])).ToList();
        web.Eventually(thisWeek.Records, Is.EqualTo(expected));
    }

    [Then(@"^Records this week says ""(.*)""$")]
    public void ThenRecordsSays(string text)
    {
        journeys.WaitFor(Journeys.Screen.ThisWeek);
        web.Eventually(thisWeek.NoRecordsText, Is.EqualTo(text));
    }

    [Then(@"^Records this week shows (\d+) records? and ""(.*)""$")]
    public void ThenRecordsShowsAndMore(int rows, string more)
    {
        journeys.WaitFor(Journeys.Screen.ThisWeek);
        web.Eventually(() => (thisWeek.Records().Count, thisWeek.MoreRecordsText()), Is.EqualTo((rows, more)));
    }

    [When(@"^I open ""(.*)"" in Records this week$")]
    public void WhenIOpenARecord(string name)
    {
        journeys.WaitFor(Journeys.Screen.ThisWeek);
        thisWeek.Records();
        thisWeek.OpenRecord(Exercises.Named(name).Id);
        web.WaitUntil(exercise.IsShown, "the Exercise page didn't show");
        exercise.WaitUntilLoaded();
    }
}
