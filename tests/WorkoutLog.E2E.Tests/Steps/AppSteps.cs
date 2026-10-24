using OpenQA.Selenium.Appium.Enums;
using Reqnroll;
using WorkoutLog.E2E.Tests.Device;
using WorkoutLog.E2E.Tests.Screens;
using WorkoutLog.E2E.Tests.Support;

namespace WorkoutLog.E2E.Tests.Steps;

/// <summary>Where a scenario starts, the app's lifecycle, the soft keyboard and the app's log.</summary>
[Binding]
public sealed class AppSteps(
    WebView web, Native native, AppLog log, Journeys journeys, SeedSteps seed, ScenarioState state, DashboardScreen dashboard,
    WorkoutScreen workout, PickerScreen picker, HistoryScreen history)
{
    [Given(@"^I am on (the Dashboard|the picker on Edit Workout)$")]
    public async Task GivenIAmOn(string screen)
    {
        if (screen == "the Dashboard")
        {
            journeys.WaitFor(Journeys.Screen.Dashboard);
            dashboard.LastWorkout();
        }
        else
        {
            await SeedAndOpenAsync();
            OpenPicker();
        }

        state.LogAtStart = log.Read();
    }

    [Given(@"^I have pressed Android back on the Dashboard$")]
    public void GivenIHavePressedBackOnTheDashboard()
    {
        journeys.WaitFor(Journeys.Screen.Dashboard);
        native.BackGesture();
        ThenTheAppIsInTheBackground();
    }

    [Then(@"^the app is in the background$")]
    public void ThenTheAppIsInTheBackground() =>
        web.Eventually(native.State, Is.Not.EqualTo(AppState.RunningInForeground), "app state");

    [When(@"^I open the app again$")]
    public void WhenIOpenTheAppAgain() => native.Launch();

    [When(@"^I close the app and open it again$")]
    public void WhenICloseTheAppAndOpenItAgain() => native.Restart();

    [When(@"^I send the app to the background and bring it back$")]
    public void WhenISendTheAppToTheBackground()
    {
        native.Background(TimeSpan.FromSeconds(3));
        native.Launch();
        dashboard.LastWorkout();
    }

    [When(@"^I open History and go back$")]
    public void WhenIOpenHistoryAndGoBack()
    {
        journeys.GoToHistory();
        history.Back();
        journeys.WaitFor(Journeys.Screen.Dashboard);
    }

    [Then(@"^the soft keyboard is (shown|hidden)$")]
    public void ThenTheSoftKeyboardIs(string shown) =>
        web.Eventually(native.IsKeyboardShown, Is.EqualTo(shown == "shown"), "soft keyboard shown");

    [Then(@"^the field asks for a (numeric|decimal) keypad$")]
    public void ThenTheFieldAsksFor(string keypad) => Assert.That(workout.InputMode(web.Focused()), Is.EqualTo(keypad));

    [Then(@"^the log shows ""(.*)""$")]
    public void ThenTheLogShows(string text) => web.Eventually(log.Read, Does.Contain(text), "app log");

    [Then(@"^the log doesn't show ""(.*)""$")]
    public void ThenTheLogDoesNotShow(string text)
    {
        web.Settle();
        Assert.That(log.Read(), Does.Not.Contain(text), "app log");
    }

    [Then(@"^the log shows ""(.*)"" once more$")]
    public void ThenTheLogShowsOnceMore(string text)
    {
        var before = AppLog.Count(state.LogAtStart, text);
        web.Eventually(() => AppLog.Count(log.Read(), text), Is.EqualTo(before + 1), $"lines with \"{text}\"");
    }

    [Then(@"^the log contains none of: (.+)$")]
    public void ThenTheLogContainsNoneOf(string texts)
    {
        web.Settle();
        var content = log.Read();
        Assert.Multiple(() =>
        {
            foreach (var text in texts.Split(", "))
            {
                Assert.That(content, Does.Not.Contain(text), "app log");
            }
        });
    }

    private void OpenPicker()
    {
        workout.OpenPicker();
        web.WaitUntil(picker.IsOpen, "the picker didn't open");
        native.HideKeyboard();
    }

    private async Task SeedAndOpenAsync()
    {
        var table = new DataTable("started", "exercises");
        table.AddRow("yesterday 18:05", "Push-up: 10 reps");
        await seed.GivenTheseSavedWorkouts(table);
        journeys.OpenWorkout(1);
    }

}
