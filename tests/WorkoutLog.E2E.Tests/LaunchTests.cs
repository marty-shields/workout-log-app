using OpenQA.Selenium;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Android;
using OpenQA.Selenium.Support.UI;

namespace WorkoutLog.E2E.Tests;

/// <summary>
/// Runs against the Debug app on a device through an Appium server with the UiAutomator2 driver.
/// Run settings: <c>AppiumServerUrl</c> (default http://127.0.0.1:4723/) and optional <c>DeviceUdid</c>.
/// </summary>
[Explicit("Needs an Appium server and a device (M6)")]
public class LaunchTests
{
    private const string Package = "uk.martinshields.workoutlog.debug";

    private AndroidDriver? driver;

    [SetUp]
    public void StartSession()
    {
        var options = new AppiumOptions
        {
            PlatformName = "Android",
            AutomationName = "UiAutomator2",
        };
        if (TestContext.Parameters.Get("DeviceUdid") is { } udid)
        {
            options.AddAdditionalAppiumOption("udid", udid);
        }

        var server = new Uri(TestContext.Parameters.Get("AppiumServerUrl", "http://127.0.0.1:4723/"));
        driver = new AndroidDriver(server, options);
        driver.ActivateApp(Package);
    }

    [TearDown]
    public void EndSession() => driver?.Dispose();

    [Test]
    public void Launch_ShowsTheDashboardAfterStartup()
    {
        var app = driver!;
        var context = $"WEBVIEW_{Package}";
        var wait = new WebDriverWait(app, TimeSpan.FromSeconds(30));

        wait.Until(_ => app.Contexts.Contains(context));
        app.Context = context;
        wait.Until(d => d.FindElements(By.CssSelector("[data-testid=history-link]")).Count == 1);

        Assert.That(app.FindElements(By.CssSelector("[data-testid=startup-error]")), Is.Empty);
    }
}
