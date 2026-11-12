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
/// installing the tested APK and locking the screen in portrait. An upgrade run (<c>PreviousApk</c> set) finds the last
/// release installed with its workouts, and its scenario installs the tested APK over it. At the end it closes the app, leaving it installed,
/// puts back the rotation settings and removes Appium's helper apps from the device.
/// Writes the server log to <c>TestResults/E2E/&lt;run&gt;/appium.log</c>.
/// Run settings: optional <c>DeviceUdid</c>; without it the first device adb lists is used.
/// Required <c>ChromedriverExecutable</c>, the path of a Chromedriver matching the device's WebView; the run fails without it rather than let Appium download an unpinned one.
/// Optional <c>AppPackage</c>, <c>AppApk</c> and <c>PreviousApk</c>: see <see cref="TestedApp"/>.
/// </summary>
[SetUpFixture]
[Explicit("Needs a device")]
public class AppiumSetup
{
    /// <summary>Entries of an APK that embeds its assemblies, one by one (Debug) or in a store (Release); a Fast Deployment APK has neither.</summary>
    private static readonly string[] EmbeddedAssemblies = ["lib_WorkoutLog.App.dll.so", "libassembly-store.so"];

    /// <summary>Apps the UiAutomator2 driver installs on the device; removed at the end of a run.</summary>
    private static readonly string[] HelperApps = ["io.appium.uiautomator2.server", "io.appium.uiautomator2.server.test", "io.appium.settings"];

    /// <summary>Device settings that set the screen's rotation; saved at the start of a run and put back at the end.</summary>
    private static readonly string[] RotationSettings = ["accelerometer_rotation", "user_rotation"];

    private static AndroidDriver? driver;
    private static Adb? adb;
    private static TestedApp? app;
    private static Dictionary<string, string>? rotation;
    private AppiumLocalService? service;

    internal static AndroidDriver Driver => driver ?? throw new InvalidOperationException("AppiumSetup has not started a session.");

    /// <summary>adb for the session's device.</summary>
    internal static Adb Adb => adb ?? throw new InvalidOperationException("AppiumSetup has not started a session.");

    internal static TestedApp App => app ?? throw new InvalidOperationException("AppiumSetup has not started a session.");

    /// <summary>The run's results folder, named by its start time.</summary>
    internal static string RunFolder { get; private set; } = "";

    internal static string ServerLog { get; private set; } = "";

    [OneTimeSetUp]
    public void Start()
    {
        var root = RepoRoot();
        var project = Path.Combine(root, "tests", "WorkoutLog.E2E.Tests");
        var appium = Path.Combine(project, "node_modules", "appium", "index.js");
        var tested = Tested(root);
        var buildHint = tested.IsDebug ? "build the App: dotnet build src/WorkoutLog.App -f net11.0-android" : "build it (docs/e2e-tests.md)";

        Require(Environment.GetEnvironmentVariable("ANDROID_HOME") is { Length: > 0 }, "ANDROID_HOME is not set (docs/dev-setup-windows.md, docs/dev-setup-linux.md).");
        Require(Environment.GetEnvironmentVariable("JAVA_HOME") is { Length: > 0 }, "JAVA_HOME is not set (docs/dev-setup-windows.md, docs/dev-setup-linux.md).");
        Require(File.Exists(appium), "Appium is not installed: run npm ci in tests/WorkoutLog.E2E.Tests.");
        Require(File.Exists(tested.Apk), $"{tested.Apk} is missing: {buildHint}.");
        Require(HasAssemblies(tested.Apk), $"{tested.Apk} has no assemblies (Fast Deployment): {buildHint}.");
        Require(tested.PreviousApk is null || File.Exists(tested.PreviousApk), $"PreviousApk {tested.PreviousApk} is missing.");
        RequirePackage("AppApk", tested.Apk, tested.Package);
        if (tested.PreviousApk is not null)
        {
            RequirePackage("PreviousApk", tested.PreviousApk, tested.Package);
        }

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
        Require(chromedriver is not null, "ChromedriverExecutable is not set: pass the Chromedriver pinned in .github/workflows/qa-pipeline.yml (docs/e2e-tests.md).");
        Require(File.Exists(chromedriver), $"ChromedriverExecutable {chromedriver} is missing.");
        options.AddAdditionalAppiumOption("chromedriverExecutable", chromedriver);

        var udid = TestContext.Parameters.Get("DeviceUdid");
        adb = new Adb(udid);
        if (udid is not null)
        {
            options.AddAdditionalAppiumOption("udid", udid);
        }

        if (!tested.IsDebug)
        {
            var serial = udid ?? adb.Run("get-serialno").Trim();
            Require(serial.StartsWith("emulator-", StringComparison.Ordinal),
                $"AppPackage {tested.Package} runs only on an emulator (emulator-*), not on {serial}: the app on a phone is never touched (docs/e2e-tests.md).");
            var image = adb.Shell("getprop", "ro.build.type");
            Require(image is "userdebug" or "eng",
                $"AppPackage {tested.Package} keeps WebView debugging off, so the tests reach its WebView only on a userdebug or eng image, which makes every WebView debuggable; {serial} runs a {image} image (docs/e2e-tests.md).");
        }

        app = tested;

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
            if (tested.PreviousApk is null)
            {
                driver.InstallApp(tested.Apk, timeout: (int)TimeSpan.FromMinutes(3).TotalMilliseconds, replace: true, checkVersion: false);
            }

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

    /// <summary>Stops the app and leaves it installed; a failure is written to the test output.</summary>
    private static void CloseApp()
    {
        if (driver is null || app is null)
        {
            return;
        }

        try
        {
            driver.TerminateApp(app.Package);
        }
        catch (WebDriverException e)
        {
            TestContext.Progress.WriteLine($"Couldn't close {app.Package}: {e.Message}");
        }
    }

    /// <summary>The tested build from the run settings; the Debug app built in this repo by default.</summary>
    private static TestedApp Tested(string root)
    {
        var package = TestContext.Parameters.Get("AppPackage", TestedApp.DebugPackage);
        var apk = TestContext.Parameters.Get("AppApk");
        Require(apk is not null || package == TestedApp.DebugPackage, $"AppApk is not set: pass the APK of {package} (docs/e2e-tests.md).");
        apk ??= Path.Combine(root, "src", "WorkoutLog.App", "bin", "Debug", "net11.0-android", $"{TestedApp.DebugPackage}-Signed.apk");
        return new TestedApp(package, Path.GetFullPath(apk), TestContext.Parameters.Get("PreviousApk") is { } previous ? Path.GetFullPath(previous) : null);
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

    /// <summary>The node binary itself, not a shim such as Volta's, so that stopping the service stops Appium.</summary>
    private static string NodePath()
    {
        var message = "Node.js is not on the PATH (docs/dev-setup-windows.md, docs/dev-setup-linux.md).";
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

    /// <summary>
    /// Fails unless <paramref name="apk"/> holds <paramref name="package"/>, so the emulator-only rule for a package other
    /// than the Debug app's covers the app that is really installed.
    /// </summary>
    private static void RequirePackage(string setting, string apk, string package)
    {
        var held = ApkPackage(apk);
        Require(held == package, $"{setting} {apk} holds {held}, not AppPackage {package}: set AppPackage to the APK's package (docs/e2e-tests.md).");
    }

    /// <summary>The package an APK holds, from <c>aapt2 dump packagename</c> in the newest build-tools under <c>ANDROID_HOME</c>.</summary>
    private static string ApkPackage(string apk)
    {
        var buildTools = Path.Combine(Environment.GetEnvironmentVariable("ANDROID_HOME")!, "build-tools");
        var newest = (Directory.Exists(buildTools) ? Directory.GetDirectories(buildTools) : [])
            .MaxBy(dir => Version.TryParse(Path.GetFileName(dir), out var version) ? version : new Version())
            ?? throw new InvalidOperationException($"{buildTools} has no build-tools: install them (docs/dev-setup-windows.md, docs/dev-setup-linux.md).");
        var start = new ProcessStartInfo(Path.Combine(newest, "aapt2"))
        {
            ArgumentList = { "dump", "packagename", apk },
            RedirectStandardOutput = true,
            RedirectStandardError = true,
        };
        using var aapt2 = Process.Start(start)!;
        var error = aapt2.StandardError.ReadToEndAsync();
        var package = aapt2.StandardOutput.ReadToEnd().Trim();
        aapt2.WaitForExit();
        Require(aapt2.ExitCode == 0, $"aapt2 can't read the package of {apk}: {error.Result.Trim()}");
        return package;
    }

    private static bool HasAssemblies(string apk)
    {
        using var zip = ZipFile.OpenRead(apk);
        return zip.Entries.Any(e => EmbeddedAssemblies.Contains(e.Name));
    }

    private static void Require(bool condition, string message)
    {
        if (!condition)
        {
            throw new InvalidOperationException(message);
        }
    }
}
