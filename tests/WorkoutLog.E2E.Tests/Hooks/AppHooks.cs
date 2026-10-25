using Reqnroll;
using WorkoutLog.E2E.Tests.Device;

namespace WorkoutLog.E2E.Tests.Hooks;

/// <summary>
/// Before each scenario: fails if the device is locked, clears the Debug app's data, launches the app and switches to its
/// WebView, so each scenario starts as a fresh install. <c>@today</c> scenarios first fail if local midnight is close.
/// A failed scenario saves its files to <c>&lt;run&gt;/&lt;nn&gt;-&lt;scenario&gt;/</c> and attaches them.
/// </summary>
[Binding]
public sealed class AppHooks(ScenarioContext scenario, DeviceClock clock, Native native)
{
    /// <summary>A <c>@today</c> scenario doesn't start this close to local midnight.</summary>
    private static readonly TimeSpan MidnightMargin = TimeSpan.FromMinutes(5);

    private static int scenarioCount;

    private string folder = "";
    private long serverLogStart;

    [BeforeScenario(Order = 0)]
    public void StartApp()
    {
        var number = $"{++scenarioCount:00}";
        var name = FileName(scenario.ScenarioInfo.Title);
        folder = Path.Combine(AppiumSetup.RunFolder, $"{number}-{name}");
        serverLogStart = new FileInfo(AppiumSetup.ServerLog).Length;
        TestContext.Progress.WriteLine($"E2E {Path.GetFileName(AppiumSetup.RunFolder)}/{number} {scenario.ScenarioInfo.Title}");

        var app = AppiumSetup.Driver;
        app.Context = Native.NativeApp;
        if (app.IsLocked())
        {
            throw new InvalidOperationException("The device is locked: unlock it and run again.");
        }

        if (scenario.ScenarioInfo.CombinedTags.Contains("today"))
        {
            RequireAwayFromMidnight(clock.LocalNow());
        }

        app.ExecuteScript("mobile: clearApp", new Dictionary<string, object> { ["appId"] = AppiumSetup.Package });
        native.Launch();
    }

    [AfterScenario]
    public void SaveFailure()
    {
        if (scenario.TestError is null)
        {
            return;
        }

        var app = AppiumSetup.Driver;
        Directory.CreateDirectory(folder);
        Save("appium.log", path => File.WriteAllText(path, ServerLogSinceStart()));
        Save("page source", () => app.Context == Native.WebView ? "webview.html" : "native.xml", path => File.WriteAllText(path, app.PageSource));
        Save("screen.png", path =>
        {
            app.Context = Native.NativeApp;
            app.GetScreenshot().SaveAsFile(path);
        });
        Save("workoutlog.log", path => File.WriteAllBytes(path, app.PullFile(AppLog.DevicePath)));
    }

    private static void RequireAwayFromMidnight(DateTime now)
    {
        var sinceMidnight = now.TimeOfDay;
        if (sinceMidnight < MidnightMargin || sinceMidnight > TimeSpan.FromDays(1) - MidnightMargin)
        {
            throw new InvalidOperationException($"It is {now:HH:mm} on the device: a @today scenario doesn't run within {MidnightMargin.TotalMinutes:0} minutes of midnight.");
        }
    }

    /// <summary>Characters replaced in a failed scenario's folder name, with control characters and spaces: those Windows or Actions artifacts don't allow.</summary>
    private static readonly char[] InvalidFileNameChars = ['"', '<', '>', ':', '|', '?', '*', '\\', '/'];

    private static string FileName(string title)
    {
        var name = new string(title.Select(c => InvalidFileNameChars.Contains(c) || char.IsControl(c) || c == ' ' ? '-' : c).ToArray());
        return name.Length > 60 ? name[..60] : name;
    }

    private void Save(string fileName, Action<string> write) => Save(fileName, () => fileName, write);

    /// <summary>Writes one file and attaches it; a failure is written to the test output instead.</summary>
    private void Save(string description, Func<string> fileName, Action<string> write)
    {
        try
        {
            var path = Path.Combine(folder, fileName());
            write(path);
            TestContext.AddTestAttachment(path);
        }
        catch (Exception e)
        {
            TestContext.Progress.WriteLine($"Couldn't save {description}: {e.Message}");
        }
    }

    private string ServerLogSinceStart()
    {
        using var stream = new FileStream(AppiumSetup.ServerLog, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        stream.Seek(serverLogStart, SeekOrigin.Begin);
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}
