using Android.App;
using Android.Content;
using Microsoft.Extensions.Logging;
using WorkoutLog.App.Logging;

namespace WorkoutLog.App.Diagnostics;

/// <summary>Logs the process start, the previous exit, activity lifecycle, trim-memory requests and unhandled exceptions.</summary>
internal sealed partial class AndroidDiagnostics(ILogger<AndroidDiagnostics> logger)
{
    private static readonly ApplicationExitInfoReason[] AbnormalExits =
    [
        ApplicationExitInfoReason.Crash,
        ApplicationExitInfoReason.CrashNative,
        ApplicationExitInfoReason.Anr,
        ApplicationExitInfoReason.LowMemory,
        ApplicationExitInfoReason.InitializationFailure,
    ];

    /// <summary>Exit records read, newest first. They include the WebView's renderer processes as well as the app's own.</summary>
    private const int RecentExits = 16;

#if DEBUG
    private const string Build = "Debug";
#else
    private const string Build = "Release";
#endif

    /// <summary>Logs unhandled exceptions from now on, then logs the start and the previous exit.</summary>
    public void ProcessStarted(Context context)
    {
        AppDomain.CurrentDomain.UnhandledException += (_, e) => Log.UnhandledException(logger, e.ExceptionObject as Exception);
        TaskScheduler.UnobservedTaskException += (_, e) => Log.UnobservedTaskException(logger, e.Exception);

        Log.ProcessStarted(logger, AppInfo.Current.VersionString, AppInfo.Current.BuildString, Build, (int)Android.OS.Build.VERSION.SdkInt);
        LogPreviousExit(context);
    }

    public void ActivityCreated() => Log.ActivityCreated(logger);

    public void ActivityStopped() => Log.ActivityStopped(logger);

    public void ActivityDestroyed() => Log.ActivityDestroyed(logger);

    public void ActivityStarted() => Log.ActivityStarted(logger);

    /// <summary>Logs a trim-memory request. On API 34+ it is only <c>UiHidden</c> or <c>Background</c>.</summary>
    public void TrimMemory(TrimMemory level) => Log.TrimMemory(logger, level);

    private void LogPreviousExit(Context context)
    {
        var activityManager = (ActivityManager?)context.GetSystemService(Context.ActivityService);
        var exit = activityManager?.GetHistoricalProcessExitReasons(null, 0, RecentExits)
            .FirstOrDefault(e => e.ProcessName == context.PackageName);
        if (exit is null)
        {
            Log.NoPreviousExit(logger);
            return;
        }

        var reason = (ApplicationExitInfoReason)exit.Reason;
        var at = DateTimeOffset.FromUnixTimeMilliseconds(exit.Timestamp);
        if (AbnormalExits.Contains(reason))
        {
            Log.PreviousExitAbnormal(logger, reason, exit.Status, at, exit.Description);
        }
        else
        {
            Log.PreviousExit(logger, reason, exit.Status, at, exit.Description);
        }
    }

    private static partial class Log
    {
        [LoggerMessage((int)AppEvent.ProcessStarted, LogLevel.Information, "App {Version} ({VersionCode}) {Build} started on Android API {Api}")]
        public static partial void ProcessStarted(ILogger logger, string version, string versionCode, string build, int api);

        [LoggerMessage((int)AppEvent.PreviousExit, LogLevel.Information, "Previous exit: {Reason}, status {Status}, at {At:O}, {Description}")]
        public static partial void PreviousExit(ILogger logger, ApplicationExitInfoReason reason, int status, DateTimeOffset at, string? description);

        [LoggerMessage((int)AppEvent.PreviousExitAbnormal, LogLevel.Warning, "Previous exit: {Reason}, status {Status}, at {At:O}, {Description}")]
        public static partial void PreviousExitAbnormal(ILogger logger, ApplicationExitInfoReason reason, int status, DateTimeOffset at, string? description);

        [LoggerMessage((int)AppEvent.NoPreviousExit, LogLevel.Information, "No previous exit recorded")]
        public static partial void NoPreviousExit(ILogger logger);

        [LoggerMessage((int)AppEvent.ActivityCreated, LogLevel.Information, "Activity created")]
        public static partial void ActivityCreated(ILogger logger);

        [LoggerMessage((int)AppEvent.ActivityStopped, LogLevel.Information, "Activity stopped")]
        public static partial void ActivityStopped(ILogger logger);

        [LoggerMessage((int)AppEvent.ActivityDestroyed, LogLevel.Information, "Activity destroyed")]
        public static partial void ActivityDestroyed(ILogger logger);

        [LoggerMessage((int)AppEvent.ActivityStarted, LogLevel.Information, "Activity started")]
        public static partial void ActivityStarted(ILogger logger);

        [LoggerMessage((int)AppEvent.TrimMemory, LogLevel.Information, "Trim memory: {Level}")]
        public static partial void TrimMemory(ILogger logger, TrimMemory level);

        [LoggerMessage((int)AppEvent.UnhandledException, LogLevel.Critical, "Unhandled exception")]
        public static partial void UnhandledException(ILogger logger, Exception? exception);

        [LoggerMessage((int)AppEvent.UnobservedTaskException, LogLevel.Error, "Unobserved task exception")]
        public static partial void UnobservedTaskException(ILogger logger, Exception exception);
    }
}
