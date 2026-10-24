using NUnit.Framework.Interfaces;
using OpenQA.Selenium;
using OpenQA.Selenium.Appium.Android;
using OpenQA.Selenium.Support.UI;

namespace WorkoutLog.E2E.Tests;

/// <summary>
/// Base class for device tests. Before each test: fails if the device is locked, clears the Debug
/// app's data, launches the app and switches to its WebView. A failed test saves its files to
/// <c>&lt;run&gt;/&lt;nn&gt;-&lt;method&gt;/</c> and attaches them.
/// </summary>
public abstract class AppTest
{
    private const string NativeApp = "NATIVE_APP";
    private const string WebView = $"WEBVIEW_{AppiumSetup.Package}";
    private const string AppLog = $"/sdcard/Android/data/{AppiumSetup.Package}/files/logs/workoutlog.log";
    private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(30);

    private static int testCount;

    private string folder = "";
    private long serverLogStart;

    protected static AndroidDriver App => AppiumSetup.Driver;

    [SetUp]
    public void StartApp()
    {
        var number = $"{++testCount:00}";
        var name = TestContext.CurrentContext.Test.MethodName;
        folder = Path.Combine(AppiumSetup.RunFolder, $"{number}-{name}");
        serverLogStart = new FileInfo(AppiumSetup.ServerLog).Length;
        TestContext.Progress.WriteLine($"E2E {Path.GetFileName(AppiumSetup.RunFolder)}/{number} {name}");

        App.Context = NativeApp;
        if (App.IsLocked())
        {
            throw new InvalidOperationException("The device is locked: unlock it and run again.");
        }

        App.ExecuteScript("mobile: clearApp", new Dictionary<string, object> { ["appId"] = AppiumSetup.Package });
        App.ActivateApp(AppiumSetup.Package);
        Wait().Until(_ => App.Contexts.Contains(WebView));
        App.Context = WebView;
    }

    [TearDown]
    public void SaveFailure()
    {
        if (TestContext.CurrentContext.Result.Outcome.Status != TestStatus.Failed)
        {
            return;
        }

        Directory.CreateDirectory(folder);
        Save("appium.log", path => File.WriteAllText(path, ServerLogSinceStart()));
        Save("page source", () => App.Context == WebView ? "webview.html" : "native.xml", path => File.WriteAllText(path, App.PageSource));
        Save("screen.png", path =>
        {
            App.Context = NativeApp;
            App.GetScreenshot().SaveAsFile(path);
        });
        Save("workoutlog.log", path => File.WriteAllBytes(path, App.PullFile(AppLog)));
    }

    /// <summary>The element with this <c>data-testid</c>; throws if there is none.</summary>
    protected static IWebElement FindByTestId(string testId) => App.FindElement(ByTestId(testId));

    /// <summary>Waits for the element with this <c>data-testid</c>.</summary>
    protected static IWebElement WaitForTestId(string testId) => Wait().Until(_ => App.FindElements(ByTestId(testId)).FirstOrDefault());

    protected static By ByTestId(string testId) => By.CssSelector($"[data-testid=\"{testId}\"]");

    private static WebDriverWait Wait() => new(App, Timeout);

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
