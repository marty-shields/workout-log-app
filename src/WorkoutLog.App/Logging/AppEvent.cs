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

    NoPreviousExit = 5004,

    ActivityCreated = 5011,

    ActivityStopped = 5012,

    ActivityDestroyed = 5013,

    /// <summary>Activity started: shown, or back from the background.</summary>
    ActivityStarted = 5014,

    /// <summary>Android asked the app to trim memory.</summary>
    TrimMemory = 5021,

    /// <summary>The WebView tried to leave the app's own origin; the load was cancelled.</summary>
    UrlBlocked = 5041,

    /// <summary>Unhandled exception; the app crashes.</summary>
    UnhandledException = 5031,

    UnobservedTaskException = 5033,

    /// <summary>Touch feedback for a haptic cue failed; nothing else happens.</summary>
    HapticFailed = 5051,

    /// <summary>Showing the keyboard again after the app returned failed; the keyboard stays as Android left it.</summary>
    KeyboardRestoreFailed = 5061,

    /// <summary>The app paused; records whether the keyboard was wanted and the window had focus.</summary>
    KeyboardPaused = 5062,

    /// <summary>The app resumed; records whether the keyboard was wanted and the window had focus.</summary>
    KeyboardResumed = 5063,

    /// <summary>The window got its focus back while a keyboard restore was waiting for it.</summary>
    KeyboardWindowFocusRegained = 5064,

    /// <summary>The restore asked the WebView whether a text input is focused, and shows the keyboard only if so.</summary>
    KeyboardRestoreDecided = 5065,

    /// <summary>The keyboard became visible or hidden, or whether it is wanted changed; records the window and screen state at that moment.</summary>
    KeyboardWantedChanged = 5066,

    /// <summary>The restore asked Android to show the keyboard; records which attempt it was and whether Android accepted the request.</summary>
    KeyboardShowRequested = 5067,

    /// <summary>A restore ended without asking for the keyboard, with the reason.</summary>
    KeyboardRestoreSkipped = 5068,

    /// <summary>The device manufacturer, model and default keyboard app package, logged once when the activity is created.</summary>
    KeyboardDevice = 5069,
}
