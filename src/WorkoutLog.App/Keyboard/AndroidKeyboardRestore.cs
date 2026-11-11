using Android.App;
using Android.Content;
using Android.Views;
using Android.Views.InputMethods;
using Android.Webkit;
using AndroidX.Core.View;
using Microsoft.Extensions.Logging;
using WorkoutLog.App.Logging;
using WorkoutLog.UI.Keyboard;

namespace WorkoutLog.App.Keyboard;

/// <summary>
/// Brings the soft keyboard back when the app returns with a text input still focused and the keyboard having been up.
/// Some phones hide the keyboard when the screen locks and nothing asks for it again after the unlock, although the focus survives.
/// Where Android restores the keyboard itself the request is a no-op. A failure is logged and otherwise ignored.
/// </summary>
internal sealed partial class AndroidKeyboardRestore(ILogger<AndroidKeyboardRestore> logger)
{
    private const string TextInputFocused =
        "(() => { const e = document.activeElement; return !!e && (e.tagName === 'INPUT' || e.tagName === 'TEXTAREA' || e.isContentEditable); })()";

    private Task<KeyboardAtPause>? atPause;

    /// <summary>Starts reading what was true of the keyboard. The page answers asynchronously, so <see cref="Resumed"/> waits for it.</summary>
    public void Paused(Activity activity)
    {
        try
        {
            var decor = activity.Window?.DecorView;
            var shown = decor is not null && ViewCompat.GetRootWindowInsets(decor)?.IsVisible(WindowInsetsCompat.Type.Ime()) == true;
            atPause = shown && FindWebView(decor) is { } webView ? ReadAsync(webView, shown) : Task.FromResult(default(KeyboardAtPause));
        }
        catch (Exception exception)
        {
            atPause = null;
            Log.RestoreFailed(logger, exception);
        }
    }

    /// <summary>Shows the keyboard once the WebView has the window focus again, if <see cref="KeyboardAtPause.ShowOnResume"/>.</summary>
    public async Task Resumed(Activity activity)
    {
        var pending = atPause;
        atPause = null;
        if (pending is null)
        {
            return;
        }

        try
        {
            if (!(await pending).ShowOnResume || FindWebView(activity.Window?.DecorView) is not { } webView)
            {
                return;
            }

            ShowWhenFocused(webView);
        }
        catch (Exception exception)
        {
            Log.RestoreFailed(logger, exception);
        }
    }

    private static Task<KeyboardAtPause> ReadAsync(Android.Webkit.WebView webView, bool shown)
    {
        var result = new TaskCompletionSource<KeyboardAtPause>();
        webView.EvaluateJavascript(TextInputFocused, new Callback(value => result.TrySetResult(new KeyboardAtPause(shown, value == "true"))));
        return result.Task;
    }

    private static void ShowWhenFocused(Android.Webkit.WebView webView)
    {
        if (webView.HasWindowFocus)
        {
            webView.Post(() => Show(webView));
            return;
        }

        var listener = new FocusListener(webView);
        webView.ViewTreeObserver?.AddOnWindowFocusChangeListener(listener);
    }

    private static void Show(Android.Webkit.WebView webView) =>
        (webView.Context?.GetSystemService(Context.InputMethodService) as InputMethodManager)?.ShowSoftInput(webView, ShowFlags.Implicit);

    private static Android.Webkit.WebView? FindWebView(Android.Views.View? view)
    {
        if (view is Android.Webkit.WebView webView)
        {
            return webView;
        }

        if (view is ViewGroup group)
        {
            for (var i = 0; i < group.ChildCount; i++)
            {
                if (FindWebView(group.GetChildAt(i)) is { } found)
                {
                    return found;
                }
            }
        }

        return null;
    }

    private sealed class Callback(Action<string?> onValue) : Java.Lang.Object, IValueCallback
    {
        public void OnReceiveValue(Java.Lang.Object? value) => onValue(value?.ToString());
    }

    private sealed class FocusListener(Android.Webkit.WebView webView) : Java.Lang.Object, ViewTreeObserver.IOnWindowFocusChangeListener
    {
        public void OnWindowFocusChanged(bool hasFocus)
        {
            if (!hasFocus)
            {
                return;
            }

            webView.ViewTreeObserver?.RemoveOnWindowFocusChangeListener(this);
            webView.Post(() => Show(webView));
        }
    }

    private static partial class Log
    {
        [LoggerMessage((int)AppEvent.KeyboardRestoreFailed, LogLevel.Warning, "Restoring the keyboard after the app returned failed")]
        public static partial void RestoreFailed(ILogger logger, Exception exception);
    }
}
