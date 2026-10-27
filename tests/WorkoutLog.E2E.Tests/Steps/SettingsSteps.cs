using Reqnroll;
using WorkoutLog.E2E.Tests.Screens;
using WorkoutLog.E2E.Tests.Support;

namespace WorkoutLog.E2E.Tests.Steps;

[Binding]
public sealed class SettingsSteps(DashboardScreen dashboard, Journeys journeys)
{
    [When(@"^I tap Settings on the Dashboard$")]
    public void WhenITapSettingsOnTheDashboard()
    {
        journeys.WaitFor(Journeys.Screen.Dashboard);
        dashboard.OpenSettings();
    }

    [Given(@"^I have opened Settings$")]
    public void GivenIHaveOpenedSettings()
    {
        WhenITapSettingsOnTheDashboard();
        journeys.WaitFor(Journeys.Screen.Settings);
    }
}
