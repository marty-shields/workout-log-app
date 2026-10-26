using OpenQA.Selenium.Appium.Android;
using OpenQA.Selenium.Appium.Android.Enums;
using OpenQA.Selenium.Appium.Enums;
using WorkoutLog.E2E.Tests.Screens;

namespace WorkoutLog.E2E.Tests.Device;

/// <summary>
/// Android itself: back, the soft keyboard and the app's state. Appium runs these on the device from either context.
/// </summary>
public sealed class Native(WebView web)
{
    internal const string NativeApp = "NATIVE_APP";
    internal const string WebView = $"WEBVIEW_{AppiumSetup.Package}";

    private static readonly TimeSpan KeyboardTimeout = TimeSpan.FromSeconds(5);
    private static readonly TimeSpan LaunchTimeout = TimeSpan.FromSeconds(30);

    private static AndroidDriver App => AppiumSetup.Driver;

    /// <summary>The Android back key.</summary>
    public void BackKey() => App.PressKeyCode(AndroidKeyCode.Back);

    public bool IsKeyboardShown() => App.IsKeyboardShown();

    /// <summary>
    /// Hides the soft keyboard with the back key when a WebView input has focus and the keyboard is shown, then waits until it
    /// is hidden. Focus stays on the input.
    /// </summary>
    public void HideKeyboard()
    {
        if (App.ExecuteScript(Screens.WebView.InputHasFocus) is not true || !App.IsKeyboardShown())
        {
            return;
        }

        App.PressKeyCode(AndroidKeyCode.Back);
        web.WaitUntil(() => !App.IsKeyboardShown(), "the soft keyboard didn't hide", KeyboardTimeout);
    }

    /// <summary>Sends the app to the background for <paramref name="time"/>, then brings it back.</summary>
    public void Background(TimeSpan time) => App.BackgroundApp(time);

    public AppState State() => App.GetAppState(AppiumSetup.Package);

    /// <summary>Whether <see cref="Launch"/> has run in this scenario.</summary>
    public bool Launched { get; private set; }

    /// <summary>Stops the app, then starts it and switches to its WebView.</summary>
    public void Restart()
    {
        App.TerminateApp(AppiumSetup.Package);
        Launch();
    }

    /// <summary>Starts or brings back the app and switches to its WebView.</summary>
    public void Launch()
    {
        App.Context = NativeApp;
        App.ActivateApp(AppiumSetup.Package);
        web.WaitUntil(() => App.Contexts.Contains(WebView), $"{WebView} didn't appear", LaunchTimeout);
        App.Context = WebView;
        Launched = true;
    }
}
