using System.Globalization;
using Reqnroll;
using WorkoutLog.E2E.Tests.Device;
using WorkoutLog.E2E.Tests.Screens;
using WorkoutLog.E2E.Tests.Support;

namespace WorkoutLog.E2E.Tests.Steps;

/// <summary>
/// Starting, filling in and saving a workout. "I have …" steps end with the soft keyboard hidden, so a later Android back acts
/// on the page; the others leave it as a user would.
/// </summary>
[Binding]
public sealed class WorkoutSteps(WebView web, Native native, Journeys journeys, WorkoutScreen workout, DeviceClock clock, ScenarioState state)
{
    [Given(@"^I have started a new workout$")]
    [When(@"^I start a new workout$")]
    public void StartANewWorkout() => journeys.StartNewWorkout();

    [When(@"^I add ""(.*)""$")]
    public void WhenIAdd(string name) => journeys.AddExercise(name);

    [Given(@"^I have added ""(.*)""$")]
    public void GivenIHaveAdded(string name)
    {
        journeys.AddExercise(name);
        native.HideKeyboard();
    }

    [When(@"^I log (.+?)((?: set \d+| sets)?): (.+)$")]
    public void WhenILog(string name, string sets, string values) => Log(name, sets, values);

    [Given(@"^I have logged (.+?)((?: set \d+| sets)?): (.+)$")]
    public void GivenIHaveLogged(string name, string sets, string values)
    {
        Log(name, sets, values);
        native.HideKeyboard();
    }

    [Given(@"^I have logged:$")]
    public void GivenIHaveLoggedTable(DataTable table)
    {
        foreach (var row in table.Rows)
        {
            journeys.LogAll(ExerciseValues.Parse($"{row["exercise"]}: {row["values"]}"));
        }

        native.HideKeyboard();
    }

    [Given(@"^I have saved the workout$")]
    public void GivenIHaveSavedTheWorkout()
    {
        native.HideKeyboard();
        journeys.Save();
        web.Settle();
    }

    [When(@"^I save the workout$")]
    public void WhenISaveTheWorkout()
    {
        journeys.Save();
        web.Settle();
    }

    [When(@"^I tap Save twice quickly$")]
    public void WhenITapSaveTwice()
    {
        native.HideKeyboard();
        var save = web.Find("save-workout");
        save.Click();
        try
        {
            save.Click();
        }
        catch (OpenQA.Selenium.WebDriverException)
        {
            // The Dashboard replaced the page before the second tap.
        }

        journeys.WaitFor(Journeys.Screen.Dashboard);
    }

    [When(@"^I discard the workout$")]
    public void WhenIDiscardTheWorkout()
    {
        journeys.Discard();
        journeys.WaitFor(Journeys.Screen.Dashboard);
    }

    [When(@"^I discard the changes$")]
    public void WhenIDiscardTheChanges()
    {
        journeys.Discard();
        journeys.WaitFor(Journeys.Screen.Workouts);
    }

    [When(@"^I delete the workout$")]
    public void WhenIDeleteTheWorkout() => journeys.Delete();

    [Given(@"^(\d+) seconds have passed since I started$")]
    public void GivenSecondsHavePassed(int seconds)
    {
        var started = state.Started ?? throw new InvalidOperationException("No workout was started.");
        var wait = started.To.AddSeconds(seconds) - clock.LocalNow();
        if (wait > TimeSpan.Zero)
        {
            Thread.Sleep(wait);
        }
    }

    [Then(@"^the title is today's date and day$")]
    public void ThenTheTitleIsToday()
    {
        var (from, to) = state.Started ?? throw new InvalidOperationException("No workout was started.");
        Assert.That(workout.Title(), Is.AnyOf(DashboardSteps.Date(from), DashboardSteps.Date(to)));
    }

    [Then(@"^the timer shows ""Elapsed"" and counts up$")]
    public void ThenTheTimerCountsUp()
    {
        var first = Elapsed();
        web.Eventually(Elapsed, Is.GreaterThan(first), "Elapsed time");
    }

    [Then(@"^the timer shows the time since I started$")]
    public void ThenTheTimerShowsTimeSinceStart()
    {
        var (from, to) = state.Started ?? throw new InvalidOperationException("No workout was started.");
        var before = clock.LocalNow();
        var elapsed = Elapsed();
        var after = clock.LocalNow();
        var second = TimeSpan.FromSeconds(1);
        Assert.That(elapsed, Is.InRange(before - to - second, after - from + second), "Elapsed time");
    }

    [Then(@"^the workout shows ""(.*)""$")]
    public void ThenTheWorkoutShows(string text) => web.Eventually(() => WorkoutMessage(text), Is.EqualTo(text));

    [Then(@"^the workout error is gone$")]
    public void ThenTheWorkoutErrorIsGone() => web.Eventually(workout.Error, Is.Null);

    /// <summary>Waits for the last add's focus and keyboard to settle, scrolls to the bottom and checks it stays there.</summary>
    [Given(@"^I have scrolled to the bottom of the workout$")]
    public void GivenIHaveScrolledToTheBottom()
    {
        web.Eventually(web.Focused, Does.StartWith("exercise-"), "focus on the added exercise");
        native.HideKeyboard();
        web.Settle();
        Assert.That(web.ScrollToBottom(), Is.True, "the workout didn't scroll");
        web.Settle();
        Assert.That(web.AtBottom(), Is.True, "the workout scrolled away from the bottom");
    }

    [Then(@"^the exercises are, in order: (.+)$")]
    public void ThenTheExercisesAreInOrder(string names) =>
        web.Eventually(workout.ExerciseNames, Is.EqualTo(names.Split(", ")));

    [Then(@"^there (?:is|are) (\d+) exercises?$")]
    public void ThenThereAreExercises(int count) => web.Eventually(() => workout.ExerciseNames().Count, Is.EqualTo(count));

    [Then(@"^(.+) shows the worked-out ""(.*)""$")]
    public void ThenShowsTheWorkedOut(string name, string text) => web.Eventually(() => workout.WorkedOut(name), Is.EqualTo(text), $"{name} worked-out line");

    [Then(@"^Add set on (.+) is off with ""(.*)""$")]
    public void ThenAddSetIsOff(string name, string note)
    {
        web.Eventually(() => workout.SetsFull(name), Is.EqualTo(note), $"{name} sets note");
        Assert.That(workout.AddSetEnabled(name), Is.False, $"{name} Add set");
    }

    [Then(@"^Add exercise is off with ""(.*)""$")]
    public void ThenAddExerciseIsOff(string note)
    {
        Assert.That(web.ScrollToBottom(), Is.True, "the workout didn't scroll");
        web.Eventually(workout.ExercisesFull, Is.EqualTo(note), "exercises note");
        Assert.That(workout.AddExerciseEnabled(), Is.False, "Add exercise");
    }

    [Then(@"^(.+) has last and PB ""(.*)""$")]
    public void ThenHasLastAndPb(string name, string text) => web.Eventually(() => workout.Weights(name), Is.EqualTo(text), $"{name} last and PB");

    /// <summary>Waits for the card, then the settle time, then checks it has no last and PB line.</summary>
    [Then(@"^(.+) has no last and PB$")]
    public void ThenHasNoLastAndPb(string name)
    {
        workout.Card(name);
        web.Settle();
        Assert.That(workout.Weights(name), Is.Null, $"{name} last and PB");
    }

    [When(@"^I remove every exercise$")]
    public void WhenIRemoveEveryExercise()
    {
        foreach (var name in workout.ExerciseNames())
        {
            workout.Remove(name);
            web.WaitUntil(() => !workout.Has(name), $"{name} wasn't removed");
        }
    }

    /// <summary>Logs " set N", every set (" sets") or, with neither, set 1 or the cardio entry.</summary>
    private void Log(string name, string sets, string values)
    {
        if (sets == " sets")
        {
            journeys.LogAll(new ExerciseValues(name, EntryValues.ParseSets(values)));
            return;
        }

        var number = sets.Length > 0 ? int.Parse(sets[" set ".Length..], CultureInfo.InvariantCulture) : 1;
        journeys.Log(name, number, EntryValues.Parse(values));
    }

    /// <summary>The time in the subtitle, e.g. Elapsed 0:01:05.</summary>
    private TimeSpan Elapsed()
    {
        var text = workout.Subtitle();
        Assert.That(text, Does.StartWith("Elapsed "));
        return TimeSpan.ParseExact(text["Elapsed ".Length..], @"h\:mm\:ss", CultureInfo.InvariantCulture);
    }

    /// <summary>The workout error, or the "No exercises yet" message when that is what's asked for.</summary>
    private string? WorkoutMessage(string expected) =>
        expected == "No exercises yet" ? (workout.ShowsNoExercises() ? web.Text("no-exercises") : null) : workout.Error();
}
