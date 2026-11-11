using OpenQA.Selenium;
using OpenQA.Selenium.Appium.Android;
using OpenQA.Selenium.Appium.Android.Enums;
using OpenQA.Selenium.Appium.Enums;
using OpenQA.Selenium.Interactions;
using WorkoutLog.E2E.Tests.Screens;

namespace WorkoutLog.E2E.Tests.Device;

/// <summary>
/// Android itself: back, the soft keyboard, the share sheet, the file picker and the app's state. Appium runs these on the device from either context.
/// </summary>
public sealed class Native(WebView web)
{
    internal const string NativeApp = "NATIVE_APP";
    internal static string WebView => AppiumSetup.App.WebViewContext;

    /// <summary>The package that shows Android's share sheet.</summary>
    private const string ShareSheetPackage = "com.android.intentresolver";

    /// <summary>How long the file picker's first list may take to show a file before Downloads is opened.</summary>
    private static readonly TimeSpan FirstListTimeout = TimeSpan.FromSeconds(5);

    private static readonly TimeSpan KeyboardTimeout = TimeSpan.FromSeconds(5);
    private static readonly TimeSpan LaunchTimeout = TimeSpan.FromSeconds(30);

    /// <summary>Why the app's WebView isn't debuggable, once a launch in this run has found it.</summary>
    private static string? notDebuggable;

    private static AndroidDriver App => AppiumSetup.Driver;

    public void BackKey() => App.PressKeyCode(AndroidKeyCode.Back);

    /// <summary>Goes to the home screen and leaves the app running; Android backs up no app that was force-stopped, as Appium stops it.</summary>
    public void EnterKey() => App.PressKeyCode(AndroidKeyCode.Enter);

    public void Home() => App.PressKeyCode(AndroidKeyCode.Home);

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

    /// <summary>
    /// Waits until Android's share sheet is in front, then closes it with the back key and switches back to the app's WebView.
    /// Fails if it doesn't open.
    /// </summary>
    public void CloseShareSheet()
    {
        App.Context = NativeApp;
        try
        {
            web.WaitUntil(() => App.CurrentPackage == ShareSheetPackage, "the share sheet didn't open", LaunchTimeout);
            App.PressKeyCode(AndroidKeyCode.Back);
            web.WaitUntil(() => App.CurrentPackage == AppiumSetup.App.Package, "the app didn't come back from the share sheet", LaunchTimeout);
        }
        finally
        {
            App.Context = WebView;
        }
    }

    /// <summary>
    /// Waits until the file picker is in front and taps the item showing <paramref name="name"/>. When the picker doesn't list it
    /// at first and has a roots menu, it opens Downloads. Then waits for the app and switches back to its WebView.
    /// Fails if the picker or the file doesn't show.
    /// </summary>
    public void PickFile(string name)
    {
        InFilePicker(() =>
        {
            var file = By.XPath($"//*[@text='{name}']");
            var roots = By.XPath("//*[@content-desc='Show roots']");
            if (!web.Holds(() => App.FindElements(file).Count > 0, FirstListTimeout) && App.FindElements(roots).Count > 0)
            {
                App.FindElement(roots).Click();
                var downloads = By.XPath("//*[@text='Downloads']");
                web.WaitUntil(() => App.FindElements(downloads).Count > 0, "the picker's Downloads root didn't show", LaunchTimeout);
                App.FindElement(downloads).Click();
            }

            web.WaitUntil(() => App.FindElements(file).Count > 0, $"{name} didn't show in the picker", LaunchTimeout);
            App.FindElement(file).Click();
        });
    }

    /// <summary>Waits until the file picker is in front, closes it with the back key and switches back to the app's WebView.</summary>
    public void CancelFilePicker() => InFilePicker(() => App.PressKeyCode(AndroidKeyCode.Back));

    /// <summary>
    /// Waits for the file picker, whichever app shows it: any package other than the app's in front. Runs <paramref name="act"/>
    /// in the native context, then waits for the app's WebView.
    /// </summary>
    private void InFilePicker(Action act)
    {
        App.Context = NativeApp;
        try
        {
            web.WaitUntil(() => App.CurrentPackage is { Length: > 0 } package && package != AppiumSetup.App.Package, "the file picker didn't open", LaunchTimeout);
            act();
            web.WaitUntil(() => App.CurrentPackage == AppiumSetup.App.Package, "the app didn't come back from the file picker", LaunchTimeout);
        }
        finally
        {
            App.Context = WebView;
        }
    }

    /// <summary>Drags one finger down the middle of the screen, as a user pulls a list to refresh it.</summary>
    public void PullDown()
    {
        App.Context = NativeApp;
        try
        {
            var size = App.Manage().Window.Size;
            var x = size.Width / 2;
            var belowTheToolbars = size.Height * 3 / 10;
            var pastTheThreshold = size.Height * 7 / 10;
            var finger = new PointerInputDevice(PointerKind.Touch, "finger");
            var pull = new ActionSequence(finger);
            pull.AddAction(finger.CreatePointerMove(CoordinateOrigin.Viewport, x, belowTheToolbars, TimeSpan.Zero));
            pull.AddAction(finger.CreatePointerDown(MouseButton.Touch));
            pull.AddAction(finger.CreatePointerMove(CoordinateOrigin.Viewport, x, pastTheThreshold, TimeSpan.FromMilliseconds(600)));
            pull.AddAction(finger.CreatePointerUp(MouseButton.Touch));
            App.PerformActions([pull]);
        }
        finally
        {
            App.Context = WebView;
        }
    }

    /// <summary>Sends the app to the background for <paramref name="time"/>, then brings it back.</summary>
    public void Background(TimeSpan time) => App.BackgroundApp(time);

    /// <summary>Turns the screen off with the power key, wakes it and dismisses the keyguard, as a user locks and unlocks the phone.</summary>
    public void LockAndUnlock()
    {
        var adb = AppiumSetup.Adb;
        adb.Shell("input", "keyevent", "KEYCODE_SLEEP");
        web.WaitUntil(() => adb.Shell("dumpsys", "power").Contains("mWakefulness=Asleep", StringComparison.Ordinal), "the screen didn't turn off", LaunchTimeout);
        adb.Shell("input", "keyevent", "KEYCODE_WAKEUP");
        adb.Shell("wm", "dismiss-keyguard");
        web.WaitUntil(() => App.GetAppState(AppiumSetup.App.Package) == AppState.RunningInForeground, "the app didn't come back to the front", LaunchTimeout);
    }

    public AppState State() => App.GetAppState(AppiumSetup.App.Package);

    /// <summary>Whether <see cref="Launch"/> has run in this scenario.</summary>
    public bool Launched { get; private set; }

    /// <summary>Stops the app, then starts it and switches to its WebView.</summary>
    public void Restart()
    {
        App.TerminateApp(AppiumSetup.App.Package);
        Launch();
    }

    /// <summary>
    /// Starts or brings back the app and switches to its WebView. Once a launch finds the WebView isn't debuggable, every
    /// later launch in the run fails at once with the reason.
    /// </summary>
    public void Launch()
    {
        if (notDebuggable is not null)
        {
            Assert.Fail(notDebuggable);
        }

        App.Context = NativeApp;
        App.ActivateApp(AppiumSetup.App.Package);
        if (!web.Holds(() => App.Contexts.Contains(WebView), LaunchTimeout))
        {
            notDebuggable = NotDebuggable();
            Assert.Fail(notDebuggable ?? $"Timed out: {WebView} didn't appear.");
        }

        App.Context = WebView;
        Launched = true;
    }

    /// <summary>Why the tests can't reach the app's WebView when the app shows one with no DevTools socket; otherwise null.</summary>
    private static string? NotDebuggable()
    {
        var package = AppiumSetup.App.Package;
        if (!AppiumSetup.Adb.TryRun(out var pids, "shell", "pidof", package) || App.FindElements(By.ClassName("android.webkit.WebView")).Count == 0)
        {
            return null;
        }

        var socket = $"webview_devtools_remote_{pids.Split(' ')[0]}";
        if (AppiumSetup.Adb.Shell("cat", "/proc/net/unix").Contains(socket, StringComparison.Ordinal))
        {
            return null;
        }

        return $"{WebView} didn't appear: {package} shows a WebView but has no DevTools socket ({socket}), so its WebView isn't debuggable. "
            + "The Release app keeps WebView debugging off and relies on a userdebug image making every WebView debuggable; "
            + "a new emulator image or WebView may have changed that (docs/e2e-tests.md).";
    }
}
