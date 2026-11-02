using System.Text.RegularExpressions;
using Reqnroll;
using WorkoutLog.E2E.Tests.Device;
using WorkoutLog.E2E.Tests.Support;

namespace WorkoutLog.E2E.Tests.Steps;

/// <summary>
/// Installing this build over the last release (<c>@upgrade</c>), and Android's backup and restore across a reinstall
/// (<c>@backup</c>). A scenario that stops with another build installed, or none, gets the tested build back.
/// </summary>
[Binding]
public sealed partial class InstallSteps(Native native, AppLog log, ScenarioState state)
{
    private const string LocalTransport = "com.android.localtransport/.LocalTransport";

    private static Adb Adb => AppiumSetup.Adb;

    private static TestedApp App => AppiumSetup.App;

    [Given(@"^the last release is installed$")]
    public void GivenTheLastReleaseIsInstalled()
    {
        var previous = App.PreviousApk
            ?? throw new InvalidOperationException("PreviousApk is not set: pass the last release's build (.github/scripts/previous-apk.sh, docs/e2e-tests.md).");
        state.TestedAppReplaced = true;
        Adb.Run("uninstall", App.Package);
        Adb.Run("install", previous);
        native.Launch();
    }

    [When(@"^I install this build over it$")]
    public void WhenIInstallThisBuildOverIt()
    {
        Adb.Run("install", "-r", App.Apk);
        state.TestedAppReplaced = false;
        native.Launch();
    }

    /// <summary>Android's "Test backup and restore" steps, on the local transport.</summary>
    [When(@"^Android backs up the app, and it is uninstalled, installed again and restored$")]
    public void WhenAndroidBacksUpAndRestores()
    {
        if (App.IsDebug)
        {
            throw new InvalidOperationException("The Debug app allows no backup: run @backup with AppPackage set to the Release app (docs/e2e-tests.md).");
        }

        state.LogBeforeBackup = log.Read();
        native.Home();
        Adb.Shell("bmgr", "enable", "true");
        Adb.Shell("bmgr", "transport", LocalTransport);
        Assert.That(Adb.Shell("bmgr", "backupnow", App.Package), Does.Contain($"Package {App.Package} with result: Success"), "backup");

        state.TestedAppReplaced = true;
        Adb.Run("uninstall", App.Package);
        Adb.Run("install", App.Apk);
        state.TestedAppReplaced = false;

        var token = CurrentSet().Match(Adb.Shell("dumpsys", "backup"));
        Assert.That(token.Success, Is.True, "dumpsys backup shows no current restore set");
        Assert.That(Adb.Shell("bmgr", "restore", token.Groups[1].Value, App.Package), Does.Contain("restoreFinished: 0"), "restore");
        native.Launch();
    }

    [Then(@"^the log has no line from before the backup$")]
    public void ThenTheLogHasNoLineFromBeforeTheBackup()
    {
        var before = Lines(state.LogBeforeBackup).ToHashSet();
        Assert.That(before, Is.Not.Empty, "log lines before the backup");
        Assert.That(Lines(log.Read()).Where(before.Contains), Is.Empty, "log lines from before the backup");
    }

    /// <summary>Runs after <see cref="Hooks.AppHooks.SaveFailure"/>, so a failure's files show the build the scenario stopped on.</summary>
    [AfterScenario(Order = 20000)]
    public void InstallTheTestedBuildAgain()
    {
        if (state.TestedAppReplaced)
        {
            Adb.TryRun(out _, "uninstall", App.Package);
            Adb.Run("install", "-r", App.Apk);
        }
    }

    private static string[] Lines(string log) => log.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

    [GeneratedRegex(@"^\s*Current:\s+(\w+)", RegexOptions.Multiline)]
    private static partial Regex CurrentSet();
}
