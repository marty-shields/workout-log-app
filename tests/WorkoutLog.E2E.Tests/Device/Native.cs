using OpenQA.Selenium;
using OpenQA.Selenium.Appium.Android;
using OpenQA.Selenium.Appium.Android.Enums;
using OpenQA.Selenium.Appium.Enums;
using WorkoutLog.E2E.Tests.Screens;

namespace WorkoutLog.E2E.Tests.Device;

/// <summary>
/// Android itself: back, the soft keyboard, the share sheet, the file picker and the app's state. Appium runs these on the device from either context.
/// </summary>
public sealed class Native(WebView web)
{
    internal const string NativeApp = "NATIVE_APP";
    internal const string WebView = $"WEBVIEW_{AppiumSetup.Package}";

    /// <summary>The package that shows Android's share sheet.</summary>
    private const string ShareSheetPackage = "com.android.intentresolver";

    /// <summary>How long the file picker's first list may take to show a file before Downloads is opened.</summary>
    private static readonly TimeSpan FirstListTimeout = TimeSpan.FromSeconds(5);

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
            web.WaitUntil(() => App.CurrentPackage == AppiumSetup.Package, "the app didn't come back from the share sheet", LaunchTimeout);
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
            if (!Shows(file, FirstListTimeout) && App.FindElements(roots).Count > 0)
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

    /// <summary>Whether an element matching <paramref name="by"/> shows within <paramref name="timeout"/>.</summary>
    private static bool Shows(By by, TimeSpan timeout)
    {
        var deadline = DateTime.UtcNow + timeout;
        while (App.FindElements(by).Count == 0)
        {
            if (DateTime.UtcNow > deadline)
            {
                return false;
            }

            Thread.Sleep(TimeSpan.FromMilliseconds(200));
        }

        return true;
    }

    /// <summary>
    /// Waits for the file picker, whichever app shows it: any package other than the app's in front. Runs <paramref name="act"/>
    /// in the native context, then waits for the app's WebView.
    /// </summary>
    private void InFilePicker(Action act)
    {
        App.Context = NativeApp;
        try
        {
            web.WaitUntil(() => App.CurrentPackage is { Length: > 0 } package && package != AppiumSetup.Package, "the file picker didn't open", LaunchTimeout);
            act();
            web.WaitUntil(() => App.CurrentPackage == AppiumSetup.Package, "the app didn't come back from the file picker", LaunchTimeout);
        }
        finally
        {
            App.Context = WebView;
        }
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
