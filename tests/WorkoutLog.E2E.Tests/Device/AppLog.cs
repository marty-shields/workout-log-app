using System.Text;

namespace WorkoutLog.E2E.Tests.Device;

/// <summary>The app's log file on the device.</summary>
public sealed class AppLog
{
    internal const string DevicePath = $"/sdcard/Android/data/{AppiumSetup.Package}/files/logs/workoutlog.log";

    /// <summary>The whole log as it is now.</summary>
    public string Read() => Encoding.UTF8.GetString(AppiumSetup.Driver.PullFile(DevicePath));

    /// <summary>How many lines of <paramref name="log"/> contain <paramref name="text"/>.</summary>
    public static int Count(string log, string text) => log.Split('\n').Count(line => line.Contains(text, StringComparison.Ordinal));
}
