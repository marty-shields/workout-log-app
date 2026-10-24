namespace WorkoutLog.App.Logging;

/// <summary>Event ids of the App's log messages: 5000+, a block of 10 per group. Values never change once logged.</summary>
internal enum AppEvent
{
    /// <summary>Process started: version, version code, build, API level.</summary>
    ProcessStarted = 5001,

    /// <summary>Why the previous process ended.</summary>
    PreviousExit = 5002,

    /// <summary>The previous process ended abnormally.</summary>
    PreviousExitAbnormal = 5003,

    /// <summary>No previous exit recorded.</summary>
    NoPreviousExit = 5004,

    /// <summary>Activity created.</summary>
    ActivityCreated = 5011,

    /// <summary>Activity stopped.</summary>
    ActivityStopped = 5012,

    /// <summary>Activity destroyed.</summary>
    ActivityDestroyed = 5013,

    /// <summary>Activity started: shown, or back from the background.</summary>
    ActivityStarted = 5014,

    /// <summary>Android asked the app to trim memory.</summary>
    TrimMemory = 5021,

    /// <summary>Unhandled exception; the app crashes.</summary>
    UnhandledException = 5031,

    /// <summary>Unobserved task exception.</summary>
    UnobservedTaskException = 5033,
}
