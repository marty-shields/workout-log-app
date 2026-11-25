using Reqnroll;
using WorkoutLog.E2E.Tests.Device;
using WorkoutLog.E2E.Tests.Screens;
using WorkoutLog.E2E.Tests.Support;

namespace WorkoutLog.E2E.Tests.Steps;

/// <summary>A workout left unfinished by a kill: the banner on This week, the sheet + opens, and the app killed the way a crash does.</summary>
[Binding]
public sealed class UnfinishedSteps(WebView web, Native native, Journeys journeys, ThisWeekScreen thisWeek, ConfirmDialog dialog)
{
    [When(@"^I kill the app and open it again$")]
    public void WhenIKillTheAppAndOpenItAgain()
    {
        web.Settle();
        native.Kill();
    }

    [Then(@"^the unfinished workout banner shows ""(.*)""$")]
    public void ThenTheBannerShows(string text)
    {
        journeys.WaitFor(Journeys.Screen.ThisWeek);
        thisWeek.WaitUntilLoaded();
        web.Eventually(thisWeek.UnfinishedDetails, Does.Contain(text), "unfinished workout banner");
    }

    [Then(@"^no unfinished workout banner is shown$")]
    public void ThenNoBannerIsShown()
    {
        journeys.WaitFor(Journeys.Screen.ThisWeek);
        web.Settle();
        Assert.That(thisWeek.HasUnfinishedBanner(), Is.False, "unfinished workout banner");
    }

    [When(@"^I tap (Resume|Discard) on the banner$")]
    public void WhenITapOnTheBanner(string button)
    {
        journeys.WaitFor(Journeys.Screen.ThisWeek);
        thisWeek.TapOnBanner(button);
    }

    [Then(@"^the prompt says ""(.*)""$")]
    public void ThenThePromptSays(string text) => web.Eventually(dialog.Message, Is.EqualTo(text), "prompt text");

    [Then(@"^the sheet offers Resume and Discard and start new$")]
    public void ThenTheSheetOffers()
    {
        web.WaitUntil(thisWeek.HasNewSheet, "the Workout in progress sheet didn't open");
        Assert.Multiple(() =>
        {
            Assert.That(web.Text("new-unfinished-primary"), Is.EqualTo("Resume"));
            Assert.That(web.Text("new-unfinished-danger"), Is.EqualTo("Discard and start new"));
        });
    }

    [When(@"^I tap (Resume|Discard and start new) on the sheet$")]
    public void WhenITapOnTheSheet(string button) => thisWeek.TapOnSheet(button);
}
