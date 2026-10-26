using System.ComponentModel;
using System.Diagnostics;
using System.IO.Compression;
using OpenQA.Selenium;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Android;
using OpenQA.Selenium.Appium.Service;
using OpenQA.Selenium.Appium.Service.Options;
using WorkoutLog.E2E.Tests.Device;

[assembly: NonParallelizable]

namespace WorkoutLog.E2E.Tests;

/// <summary>
/// Starts the Appium server from this project's node_modules and opens one session for the run,
/// installing the Debug APK and locking the screen in portrait. At the end it closes the Debug app, leaving it installed,
/// puts back the rotation settings and removes Appium's helper apps from the device.
/// Writes the server log to <c>TestResults/E2E/&lt;run&gt;/appium.log</c>.
/// Run settings: optional <c>DeviceUdid</c>; without it the first device adb lists is used.
/// Optional <c>ChromedriverExecutable</c>, a Chromedriver binary to use; without it Chromedriver downloads itself.
/// </summary>
[SetUpFixture]
[Explicit("Needs a device")]
public class AppiumSetup
{
    internal const string Package = "uk.martinshields.workoutlog.debug";

    /// <summary>An assembly entry in an APK that embeds its assemblies; a Fast Deployment APK has none.</summary>
    private const string EmbeddedAssembly = "lib_WorkoutLog.App.dll.so";

    /// <summary>Apps the UiAutomator2 driver installs on the device; removed at the end of a run.</summary>
    private static readonly string[] HelperApps = ["io.appium.uiautomator2.server", "io.appium.uiautomator2.server.test", "io.appium.settings"];

    /// <summary>Device settings that set the screen's rotation; saved at the start of a run and put back at the end.</summary>
    private static readonly string[] RotationSettings = ["accelerometer_rotation", "user_rotation"];

    private static AndroidDriver? driver;
    private static Adb? adb;
    private static Dictionary<string, string>? rotation;
    private AppiumLocalService? service;

    internal static AndroidDriver Driver => driver ?? throw new InvalidOperationException("AppiumSetup has not started a session.");

    /// <summary>adb for the session's device.</summary>
    internal static Adb Adb => adb ?? throw new InvalidOperationException("AppiumSetup has not started a session.");

    /// <summary>The run's results folder, named by its start time.</summary>
    internal static string RunFolder { get; private set; } = "";

    internal static string ServerLog { get; private set; } = "";

    [OneTimeSetUp]
    public void Start()
    {
        var root = RepoRoot();
        var project = Path.Combine(root, "tests", "WorkoutLog.E2E.Tests");
        var appium = Path.Combine(project, "node_modules", "appium", "index.js");
        var apk = Path.Combine(root, "src", "WorkoutLog.App", "bin", "Debug", "net10.0-android", $"{Package}-Signed.apk");
        var buildHint = "build the App: dotnet build src/WorkoutLog.App -f net10.0-android";

        Require(Environment.GetEnvironmentVariable("ANDROID_HOME") is { Length: > 0 }, "ANDROID_HOME is not set (docs/06-dev-setup-build.md).");
        Require(Environment.GetEnvironmentVariable("JAVA_HOME") is { Length: > 0 }, "JAVA_HOME is not set (docs/06-dev-setup-build.md).");
        Require(File.Exists(appium), "Appium is not installed: run npm ci in tests/WorkoutLog.E2E.Tests.");
        Require(File.Exists(apk), $"{apk} is missing: {buildHint}.");
        Require(HasAssemblies(apk), $"{apk} has no assemblies (Fast Deployment): {buildHint}.");
        var node = NodePath();

        RunFolder = Path.Combine(project, "TestResults", "E2E", DateTime.Now.ToString("yyyyMMdd-HHmmss"));
        ServerLog = Path.Combine(RunFolder, "appium.log");
        Directory.CreateDirectory(RunFolder);

        var options = new AppiumOptions
        {
            PlatformName = "Android",
            AutomationName = "UiAutomator2",
        };
        options.AddAdditionalAppiumOption("recreateChromeDriverSessions", true);
        options.AddAdditionalAppiumOption("adbExecTimeout", (int)TimeSpan.FromMinutes(1).TotalMilliseconds);
        var arguments = new OptionCollector();
        var chromedriver = TestContext.Parameters.Get("ChromedriverExecutable");
        if (chromedriver is null)
        {
            var chromedrivers = Directory.CreateDirectory(Path.Combine(project, ".chromedriver")).FullName;
            options.AddAdditionalAppiumOption("chromedriverExecutableDir", chromedrivers);
            arguments.AddArguments(new("--allow-insecure", "uiautomator2:chromedriver_autodownload"));
        }
        else
        {
            Require(File.Exists(chromedriver), $"ChromedriverExecutable {chromedriver} is missing.");
            options.AddAdditionalAppiumOption("chromedriverExecutable", chromedriver);
        }

        var udid = TestContext.Parameters.Get("DeviceUdid");
        adb = new Adb(udid);
        if (udid is not null)
        {
            options.AddAdditionalAppiumOption("udid", udid);
        }

        try
        {
            service = new AppiumServiceBuilder()
                .UsingDriverExecutable(new FileInfo(node))
                .WithAppiumJS(new FileInfo(appium))
                .WithIPAddress("127.0.0.1")
                .UsingAnyFreePort()
                .WithEnvironment(new Dictionary<string, string> { ["APPIUM_HOME"] = project })
                .WithArguments(arguments)
                .WithLogFile(new FileInfo(ServerLog))
                .Build();
            service.Start();

            driver = new AndroidDriver(service, options, TimeSpan.FromMinutes(3));
            driver.InstallApp(apk, timeout: (int)TimeSpan.FromMinutes(3).TotalMilliseconds, replace: true, checkVersion: false);
            LockPortrait();
        }
        catch
        {
            Stop();
            throw;
        }
    }

    [OneTimeTearDown]
    public void Stop()
    {
        var started = driver is not null;
        CloseApp();
        driver?.Dispose();
        driver = null;
        service?.Dispose();
        service = null;
        if (started)
        {
            RestoreRotation();
            RemoveHelperApps();
        }
    }

    /// <summary>Turns auto-rotate off and holds the screen in portrait, saving both rotation settings.</summary>
    private static void LockPortrait()
    {
        rotation = RotationSettings.ToDictionary(name => name, name => Adb.Shell("settings", "get", "system", name));
        Adb.Shell("settings", "put", "system", "accelerometer_rotation", "0");
        Adb.Shell("settings", "put", "system", "user_rotation", "0");
    }

    /// <summary>Puts back the saved rotation settings, skipping any that were unset; a failure is written to the test output.</summary>
    private static void RestoreRotation()
    {
        foreach (var (name, value) in rotation ?? [])
        {
            if (value != "null" && !Adb.TryRun(out var output, "shell", "settings", "put", "system", name, value))
            {
                TestContext.Progress.WriteLine($"Couldn't restore {name}: {output}");
            }
        }

        rotation = null;
    }

    /// <summary>Stops the Debug app and leaves it installed; a failure is written to the test output.</summary>
    private static void CloseApp()
    {
        try
        {
            driver?.TerminateApp(Package);
        }
        catch (WebDriverException e)
        {
            TestContext.Progress.WriteLine($"Couldn't close {Package}: {e.Message}");
        }
    }

    /// <summary>Uninstalls <see cref="HelperApps"/> with adb; a failure is written to the test output.</summary>
    private static void RemoveHelperApps()
    {
        foreach (var app in HelperApps)
        {
            if (!Adb.TryRun(out var output, "uninstall", app))
            {
                TestContext.Progress.WriteLine($"Couldn't remove {app}: {output}");
            }
        }
    }

    /// <summary>The folder that holds WorkoutLog.slnx, found from the test output folder.</summary>
    private static string RepoRoot()
    {
        var directory = new DirectoryInfo(TestContext.CurrentContext.TestDirectory);
        while (!File.Exists(Path.Combine(directory.FullName, "WorkoutLog.slnx")))
        {
            directory = directory.Parent ?? throw new InvalidOperationException("WorkoutLog.slnx not found above the test output folder.");
        }

        return directory.FullName;
    }

    /// <summary>
    /// The node binary itself, not a shim such as Volta's, so that stopping the service stops Appium.
    /// </summary>
    private static string NodePath()
    {
        var message = "Node.js is not on the PATH (docs/06-dev-setup-build.md).";
        try
        {
            using var node = Process.Start(new ProcessStartInfo("node", "-p process.execPath") { RedirectStandardOutput = true })!;
            var path = node.StandardOutput.ReadToEnd().Trim();
            node.WaitForExit();
            Require(node.ExitCode == 0 && Path.IsPathFullyQualified(path) && File.Exists(path), message);
            return path;
        }
        catch (Win32Exception e)
        {
            throw new InvalidOperationException(message, e);
        }
    }

    private static bool HasAssemblies(string apk)
    {
        using var zip = ZipFile.OpenRead(apk);
        return zip.Entries.Any(e => e.Name == EmbeddedAssembly);
    }

    private static void Require(bool condition, string message)
    {
        if (!condition)
        {
            throw new InvalidOperationException(message);
        }
    }
}
