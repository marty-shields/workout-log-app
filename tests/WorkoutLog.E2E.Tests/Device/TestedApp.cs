namespace WorkoutLog.E2E.Tests.Device;

/// <summary>
/// The build a run tests, from the run settings: <c>AppPackage</c> (the Debug app by default) and <c>AppApk</c>, plus
/// <c>PreviousApk</c>, the last release's build of the same app for <c>@upgrade</c>. A package other than the Debug app's runs
/// only on an emulator, so the app on a phone is never touched.
/// </summary>
internal sealed record TestedApp(string Package, string Apk, string? PreviousApk)
{
    internal const string DebugPackage = "uk.martinshields.workoutlog.debug";

    /// <summary>The debuggable Debug app: adb reaches its files with run-as. Another build's need root, which emulators allow.</summary>
    public bool IsDebug => Package == DebugPackage;

    public string WebViewContext => $"WEBVIEW_{Package}";

    public string LogPath => $"/sdcard/Android/data/{Package}/files/logs/workoutlog.log";

    /// <summary>The app's private folder, which holds <c>files</c> and <c>cache</c>.</summary>
    public string DataFolder => $"/data/data/{Package}";
}
