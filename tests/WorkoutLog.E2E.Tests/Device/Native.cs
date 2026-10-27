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

    /// <summary>The package that shows Android's file picker.</summary>
    private const string FilePickerPackage = "com.google.android.documentsui";

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
    /// Waits until Android's file picker is in front, opens its Downloads list and taps <paramref name="name"/>,
    /// then waits for the app and switches back to its WebView. Fails if the picker or the file doesn't show.
    /// </summary>
    public void PickFile(string name)
    {
        InFilePicker(() =>
        {
            var file = By.XPath($"//*[@resource-id='android:id/title' and @text='{name}']");
            if (App.FindElements(file).Count == 0)
            {
                App.FindElement(By.XPath("//*[@content-desc='Show roots']")).Click();
                web.WaitUntil(() => App.FindElements(By.XPath("//*[@text='Downloads']")).Count > 0, "the picker's Downloads root didn't show", LaunchTimeout);
                App.FindElement(By.XPath("//*[@text='Downloads']")).Click();
            }

            web.WaitUntil(() => App.FindElements(file).Count > 0, $"{name} didn't show in the picker", LaunchTimeout);
            App.FindElement(file).Click();
        });
    }

    /// <summary>Waits until Android's file picker is in front, closes it with the back key and switches back to the app's WebView.</summary>
    public void CancelFilePicker() => InFilePicker(() => App.PressKeyCode(AndroidKeyCode.Back));

    /// <summary>Waits for the file picker, runs <paramref name="act"/> in the native context, then waits for the app's WebView.</summary>
    private void InFilePicker(Action act)
    {
        App.Context = NativeApp;
        try
        {
            web.WaitUntil(() => App.CurrentPackage == FilePickerPackage, "the file picker didn't open", LaunchTimeout);
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
