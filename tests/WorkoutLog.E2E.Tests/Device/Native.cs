using System.Globalization;
using System.Text.RegularExpressions;
using OpenQA.Selenium;
using OpenQA.Selenium.Appium.Android;
using OpenQA.Selenium.Appium.Android.Enums;
using OpenQA.Selenium.Appium.Enums;
using OpenQA.Selenium.Interactions;
using WorkoutLog.E2E.Tests.Screens;

namespace WorkoutLog.E2E.Tests.Device;

/// <summary>
/// Android itself: back, the soft keyboard, the share sheet, the file picker, the app's state, the phone's dark theme and the status bar.
/// Appium runs these on the device from either context.
/// </summary>
public sealed partial class Native(WebView web)
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

    /// <summary>Sets the phone's dark theme to a <c>cmd uimode night</c> value (yes, no or auto) and returns the value it replaced.</summary>
    public static string SetDarkTheme(string night)
    {
        var replaced = AppiumSetup.Adb.Shell("cmd", "uimode", "night").Replace("Night mode: ", "", StringComparison.Ordinal);
        AppiumSetup.Adb.Shell("cmd", "uimode", "night", night);
        return replaced;
    }

    /// <summary>
    /// The colour of the status bar's icons over the app: dark when a status bar appearance region in <c>dumpsys window</c>
    /// asks for LIGHT_STATUS_BARS (icons for a light surface), otherwise light. Android prints the regions in every theme,
    /// unlike <c>mLastAppearance</c>, which it leaves out when no flag is set; a dump without them is not read as light.
    /// </summary>
    public static string StatusBarIcons()
    {
        var window = AppiumSetup.Adb.Shell("dumpsys", "window");
        if (!HasFocus(window))
        {
            return "not over the app";
        }

        var regions = window.Split('\n')
            .SkipWhile(line => !line.Contains("mLastStatusBarAppearanceRegions=", StringComparison.Ordinal))
            .Skip(1)
            .TakeWhile(line => line.TrimStart().StartsWith("AppearanceRegion{", StringComparison.Ordinal))
            .ToList();
        if (regions.Count == 0)
        {
            return "unknown: dumpsys window has no status bar appearance regions";
        }

        return regions.Any(region => region.Contains("LIGHT_STATUS_BARS", StringComparison.Ordinal)) ? "dark" : "light";
    }

    /// <summary>
    /// The screen rows the system bars cover, in device pixels, from the visible inset sources in <c>dumpsys window</c>:
    /// the row where the status bar and camera cutout end at the top, and the row where the navigation bar starts at the bottom.
    /// Null when the dump lacks either.
    /// </summary>
    public static (int TopEnd, int BottomStart)? SystemBars()
    {
        var sources = AppiumSetup.Adb.Shell("dumpsys", "window").Split('\n')
            .Select(line => InsetsSource().Match(line))
            .Where(match => match.Success)
            .Select(match => (Type: match.Groups["type"].Value, Top: int.Parse(match.Groups["top"].Value, CultureInfo.InvariantCulture),
                Bottom: int.Parse(match.Groups["bottom"].Value, CultureInfo.InvariantCulture)))
            .ToList();
        var top = sources.Where(s => s.Type != "navigationBars" && s.Top == 0).Select(s => s.Bottom).DefaultIfEmpty().Max();
        var bottom = sources.Where(s => s.Type == "navigationBars" && s.Top > 0).Select(s => s.Top).DefaultIfEmpty().Min();
        return top > 0 && bottom > 0 ? (top, bottom) : null;
    }

    [GeneratedRegex(@"InsetsSource id=\S+ type=(?<type>statusBars|displayCutout|navigationBars) frame=\[\d+,(?<top>\d+)\]\[\d+,(?<bottom>\d+)\] visible=true")]
    private static partial Regex InsetsSource();

    /// <summary>Whether the app's window has the focus, in <c>dumpsys window</c>'s output.</summary>
    private static bool HasFocus(string window) =>
        window.Split('\n').Any(line => line.Contains("mCurrentFocus", StringComparison.Ordinal) && line.Contains(AppiumSetup.App.Package, StringComparison.Ordinal));

    /// <summary>
    /// Turns the screen off with the power key, wakes it and dismisses the keyguard, as a user locks and unlocks the phone.
    /// Waits until the app's window lost the focus, so the app really paused, and until it has the focus again; the page then settles.
    /// No PIN is entered.
    /// </summary>
    public void LockAndUnlock()
    {
        var adb = AppiumSetup.Adb;
        static bool AppHasFocus() => HasFocus(AppiumSetup.Adb.Shell("dumpsys", "window"));

        adb.Shell("input", "keyevent", "KEYCODE_SLEEP");
        web.WaitUntil(() => adb.Shell("dumpsys", "power") is var power && (power.Contains("mWakefulness=Asleep", StringComparison.Ordinal) || power.Contains("mWakefulness=Dozing", StringComparison.Ordinal)), "the screen didn't turn off", LaunchTimeout);
        web.WaitUntil(() => !AppHasFocus(), "the app's window kept the focus while the screen was off", LaunchTimeout);
        adb.Shell("input", "keyevent", "KEYCODE_WAKEUP");
        adb.Shell("wm", "dismiss-keyguard");
        web.WaitUntil(AppHasFocus, "the app's window didn't get the focus", LaunchTimeout);
        web.Settle();
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
