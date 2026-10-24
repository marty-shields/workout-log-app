using OpenQA.Selenium;

namespace WorkoutLog.E2E.Tests;

public class LaunchTests : AppTest
{
    [Test]
    public void Launch_ShowsTheFreshInstallDashboard()
    {
        var empty = WaitForTestId("last-workout-empty");

        Assert.Multiple(() =>
        {
            Assert.That(empty.Text, Is.EqualTo("No workouts yet"));
            Assert.That(FindByTestId("count-week").FindElement(By.CssSelector(".value")).Text, Is.EqualTo("0"));
            Assert.That(FindByTestId("count-month").FindElement(By.CssSelector(".value")).Text, Is.EqualTo("0"));
            Assert.That(App.FindElements(ByTestId("history-link")), Has.Count.EqualTo(1));
            Assert.That(App.FindElements(ByTestId("new-workout")), Has.Count.EqualTo(1));
            Assert.That(App.FindElements(ByTestId("startup-error")), Is.Empty);
        });
    }
}
