using System.Security.Cryptography;
using Reqnroll;
using WorkoutLog.E2E.Tests.Device;
using WorkoutLog.E2E.Tests.Support;

namespace WorkoutLog.E2E.Tests.Steps;

/// <summary>
/// Installing this build over the last release (<c>@upgrade</c>), and Android's backup and restore across a reinstall
/// (<c>@backup</c>). A scenario that stops with another build installed, or none, gets the tested build back.
/// </summary>
[Binding]
public sealed class InstallSteps(Native native, AppLog log, ScenarioState state)
{
    private const string LocalTransport = "com.android.localtransport/.LocalTransport";

    private static Adb Adb => AppiumSetup.Adb;

    private static TestedApp App => AppiumSetup.App;

    /// <summary>
    /// The upgrade handoff: upgrade-e2e.sh has installed <see cref="TestedApp.PreviousApk"/> and saved the workouts in
    /// UpgradeSeed.feature with the last release's own E2E project. Checks that build is the one installed.
    /// </summary>
    [Given(@"^the last release is installed with the workouts it saved$")]
    public void GivenTheLastReleaseIsInstalledWithItsWorkouts()
    {
        var previous = App.PreviousApk
            ?? throw new InvalidOperationException("PreviousApk is not set: run @upgrade through .github/scripts/upgrade-e2e.sh (docs/e2e-tests.md).");
        state.TestedAppReplaced = true;
        Assert.That(Adb.TryRun(out var path, "shell", "pm", "path", App.Package), Is.True, $"{App.Package} is installed: upgrade-e2e.sh installs the last release first");
        var installed = Adb.Shell("sha256sum", path.Split('\n')[0]["package:".Length..]).Split(' ')[0];
        Assert.That(installed, Is.EqualTo(Convert.ToHexStringLower(SHA256.HashData(File.ReadAllBytes(previous)))),
            "the installed build is PreviousApk, with the workouts upgrade-e2e.sh saved in it");
    }

    [When(@"^I install this build over it$")]
    public void WhenIInstallThisBuildOverIt()
    {
        Adb.Run("install", "-r", App.Apk);
        state.TestedAppReplaced = false;
        native.Launch();
    }

    /// <summary>
    /// Android's "Test backup and restore" steps, on the local transport. No <c>bmgr restore</c> runs: the install itself
    /// restores the backup, as on a new phone.
    /// </summary>
    [When(@"^Android backs up the app, and it is uninstalled and installed again$")]
    public void WhenAndroidBacksUpAndReinstalls()
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

        Assert.That(Adb.AppShell(App, "find", App.DataFolder, "-name", "workoutlog.db3"), Is.Not.Empty, "the install restored the database");
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
}
