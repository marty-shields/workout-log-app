using Reqnroll;
using WorkoutLog.E2E.Tests.Screens;
using WorkoutLog.E2E.Tests.Support;

namespace WorkoutLog.E2E.Tests.Steps;

/// <summary>Writing the workout note and exercise notes, and checking them and last time's note.</summary>
[Binding]
public sealed class NotesSteps(WebView web, Journeys journeys, WorkoutScreen workout)
{
    [When(@"^I add the workout note ""(.*)""$")]
    public void WhenIAddTheWorkoutNote(string text) => workout.EnterWorkoutNote(text);

    [When(@"^I add the note ""(.*)"" to (.+)$")]
    public void WhenIAddTheNote(string text, string name) => workout.EnterNote(name, text);

    [When(@"^I open the workout note and leave it$")]
    public void WhenIOpenTheWorkoutNoteAndLeaveIt() => workout.OpenNoteAndLeave(null);

    [When(@"^I open the note of (.+) and leave it$")]
    public void WhenIOpenTheNoteAndLeaveIt(string name) => workout.OpenNoteAndLeave(name);

    [Then(@"^the workout note is ""(.*)""$")]
    public void ThenTheWorkoutNoteIs(string text) => web.Eventually(workout.WorkoutNote, Is.EqualTo(text), "workout note");

    [Then(@"^(.+) has the note ""(.*)""$")]
    public void ThenHasTheNote(string name, string text) => web.Eventually(() => workout.Note(name), Is.EqualTo(text), $"{name} note");

    [Then(@"^(.+) has last time's note ""(.*)""$")]
    public void ThenShowsLastTimesNote(string name, string text) =>
        web.Eventually(() => workout.LastNote(name), Is.EqualTo(text), $"{name} last time's note");

    /// <summary>Waits for the card, then the settle time, then checks it has no last time's note.</summary>
    [Then(@"^(.+) has no last time's note$")]
    public void ThenShowsNoLastTimesNote(string name)
    {
        workout.Card(name);
        web.Settle();
        Assert.That(workout.LastNote(name), Is.Null, $"{name} last time's note");
    }

    [Then(@"^reopening workout (\d+) shows the workout note ""(.*)""$")]
    public void ThenReopeningShowsTheWorkoutNote(int id, string text)
    {
        journeys.OpenWorkout(id);
        web.Eventually(workout.WorkoutNote, Is.EqualTo(text), "workout note");
    }

    [Then(@"^reopening workout (\d+) shows the note ""(.*)"" on (.+)$")]
    public void ThenReopeningShowsTheNote(int id, string text, string name)
    {
        journeys.OpenWorkout(id);
        Assert.That(workout.Note(name), Is.EqualTo(text), $"{name} note");
    }
}
