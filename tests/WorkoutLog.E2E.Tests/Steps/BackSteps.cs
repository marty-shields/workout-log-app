using Reqnroll;
using WorkoutLog.E2E.Tests.Device;
using WorkoutLog.E2E.Tests.Screens;
using WorkoutLog.E2E.Tests.Support;

namespace WorkoutLog.E2E.Tests.Steps;

/// <summary>Back, prompts and which screen shows.</summary>
[Binding]
public sealed class BackSteps(WebView web, Native native, Journeys journeys, WorkoutScreen workout, ConfirmDialog dialog)
{
    private const string Way = "(the back arrow|Android back)";

    [Given(@"^I have gone back with " + Way + "$")]
    [When(@"^I go back with " + Way + "$")]
    public void GoBackWith(string how) => journeys.Back(how);

    [When(@"^I press Android back$")]
    public void WhenIPressAndroidBack() => native.BackKey();

    [Then(@"^the prompt ""(.*)"" shows (Discard|Delete) and Cancel$")]
    public void ThenThePromptShows(string title, string confirm)
    {
        ThenThePromptShows(title);
        Assert.Multiple(() =>
        {
            Assert.That(dialog.ConfirmText(), Is.EqualTo(confirm), "confirm button");
            Assert.That(dialog.CancelText(), Is.EqualTo("Cancel"), "cancel button");
        });
    }

    [Then(@"^the prompt ""(.*)"" shows$")]
    public void ThenThePromptShows(string title)
    {
        web.Eventually(dialog.IsOpen, Is.True, "prompt open");
        Assert.That(dialog.Title(), Is.EqualTo(title));
    }

    [Then(@"^no prompt is shown$")]
    [Then(@"^the prompt is closed$")]
    public void ThenNoPromptIsShown()
    {
        web.Settle();
        Assert.That(dialog.IsOpen(), Is.False, "prompt open");
    }

    [When(@"^I tap (Cancel|Discard)$")]
    public void WhenITapOnThePrompt(string button)
    {
        web.WaitUntil(dialog.IsOpen, "the prompt didn't show");
        if (button == "Cancel")
        {
            dialog.Cancel();
        }
        else
        {
            dialog.Confirm();
        }
    }

    [When(@"^I tap Delete on the prompt$")]
    public void WhenITapDeleteOnThePrompt()
    {
        web.WaitUntil(dialog.IsOpen, "the prompt didn't show");
        dialog.Confirm();
    }

    [Given(@"^I have tapped Delete$")]
    [When(@"^I tap Delete$")]
    public void TapDelete() => workout.Delete();

    [Given(@"^I have deleted workout (\d+)$")]
    public void GivenIHaveDeletedWorkout(int id)
    {
        journeys.OpenWorkout(id);
        journeys.Delete();
    }

    [Then(@"^I am ((?:still )?)on (the Dashboard|Workouts|Exercises|Settings|New Workout|Edit Workout)$")]
    public void ThenIAmOn(string still, string screen)
    {
        if (still.Length > 0)
        {
            web.Settle();
        }

        switch (screen)
        {
            case "the Dashboard":
                journeys.WaitFor(Journeys.Screen.Dashboard);
                break;
            case "Workouts":
                journeys.WaitFor(Journeys.Screen.Workouts);
                break;
            case "Exercises":
                journeys.WaitFor(Journeys.Screen.Exercises);
                break;
            case "Settings":
                journeys.WaitFor(Journeys.Screen.Settings);
                break;
            default:
                journeys.WaitFor(Journeys.Screen.Workout);
                Assert.That(workout.IsEdit(), Is.EqualTo(screen == "Edit Workout"), screen);
                break;
        }
    }

    [Then(@"^I am on Workouts with no prompt$")]
    public void ThenIAmOnWorkoutsWithNoPrompt()
    {
        journeys.WaitFor(Journeys.Screen.Workouts);
        Assert.That(dialog.IsOpen(), Is.False, "prompt open");
    }
}
