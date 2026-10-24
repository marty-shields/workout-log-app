using OpenQA.Selenium.Appium.Android;
using OpenQA.Selenium.Appium.Android.Enums;
using OpenQA.Selenium.Appium.Enums;
using OpenQA.Selenium.Interactions;
using WorkoutLog.E2E.Tests.Screens;

namespace WorkoutLog.E2E.Tests.Device;

/// <summary>
/// Android itself: back, the soft keyboard and the app's state. Appium runs these on the device from either context;
/// the back gesture switches to the native context and returns to the context it started in.
/// </summary>
public sealed class Native(WebView web)
{
    internal const string NativeApp = "NATIVE_APP";
    internal const string WebView = $"WEBVIEW_{AppiumSetup.Package}";

    private static readonly TimeSpan KeyboardTimeout = TimeSpan.FromSeconds(5);
    private static readonly TimeSpan LaunchTimeout = TimeSpan.FromSeconds(30);

    private static AndroidDriver App => AppiumSetup.Driver;

    /// <summary>The back gesture: a swipe in from the left edge at mid-height.</summary>
    public void BackGesture() => InNative(() =>
    {
        var size = App.Manage().Window.Size;
        var finger = new PointerInputDevice(PointerKind.Touch, "finger");
        var swipe = new ActionSequence(finger);
        var y = size.Height / 2;
        swipe.AddAction(finger.CreatePointerMove(CoordinateOrigin.Viewport, 1, y, TimeSpan.Zero));
        swipe.AddAction(finger.CreatePointerDown(MouseButton.Touch));
        swipe.AddAction(finger.CreatePointerMove(CoordinateOrigin.Viewport, size.Width * 2 / 5, y, TimeSpan.FromMilliseconds(250)));
        swipe.AddAction(finger.CreatePointerUp(MouseButton.Touch));
        App.PerformActions([swipe]);
    });

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
    }

    private static void InNative(Action action)
    {
        var context = App.Context;
        App.Context = NativeApp;
        try
        {
            action();
        }
        finally
        {
            if (context != NativeApp && App.Contexts.Contains(context))
            {
                App.Context = context;
            }
        }
    }
}
