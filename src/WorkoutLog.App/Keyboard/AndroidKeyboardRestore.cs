using Android.App;
using Android.Content;
using Android.Views;
using Android.Views.InputMethods;
using Microsoft.Extensions.Logging;
using WorkoutLog.App.Logging;

namespace WorkoutLog.App.Keyboard;

/// <summary>
/// Brings the soft keyboard back when the app returns with a text input still focused and the user had not hidden the keyboard.
/// Some phones hide the keyboard when the screen locks and nothing asks for it again after the unlock, although the focus survives.
/// Where Android restores the keyboard itself the request is a no-op. A failure is logged and otherwise ignored.
/// </summary>
internal sealed partial class AndroidKeyboardRestore(ILogger<AndroidKeyboardRestore> logger) : IDisposable
{
    private Activity? current;
    private bool resumed;
    private bool keyboardWanted;
    private int generation;
    private ViewTreeObserver? focusObserver;
    private ViewTreeObserver.IOnWindowFocusChangeListener? focusListener;

    /// <summary>Starts following the keyboard of a created activity, which replaces any earlier one.</summary>
    public void Attach(Activity activity)
    {
        try
        {
            Clear();
            current = activity;
            activity.Window?.DecorView?.SetOnApplyWindowInsetsListener(new InsetsListener(this));
        }
        catch (Exception exception)
        {
            Log.RestoreFailed(logger, exception);
        }
    }

    /// <summary>Stops following the keyboard when the current activity is destroyed. An earlier activity is ignored.</summary>
    public void Detach(Activity activity)
    {
        if (!ReferenceEquals(activity, current))
        {
            return;
        }

        try
        {
            Clear();
        }
        catch (Exception exception)
        {
            Log.RestoreFailed(logger, exception);
        }
    }

    public void Paused(Activity activity)
    {
        if (!ReferenceEquals(activity, current))
        {
            return;
        }

        try
        {
            resumed = false;
            generation++;
            RemoveFocusListener();
            Log.Paused(logger, keyboardWanted, activity.Window?.DecorView?.HasWindowFocus ?? false);
        }
        catch (Exception exception)
        {
            Log.RestoreFailed(logger, exception);
        }
    }

    /// <summary>Asks for the keyboard once the WebView has the window focus again, if it was wanted.</summary>
    public void Resumed(Activity activity)
    {
        if (!ReferenceEquals(activity, current))
        {
            return;
        }

        try
        {
            resumed = true;
            var decor = activity.Window?.DecorView;
            Log.Resumed(logger, keyboardWanted, decor?.HasWindowFocus ?? false);
            RemoveFocusListener();
            if (!keyboardWanted || FindWebView(decor) is not { } webView)
            {
                return;
            }

            ShowWhenFocused(webView, ++generation);
        }
        catch (Exception exception)
        {
            Log.RestoreFailed(logger, exception);
        }
    }

    private void ImeChanged(Android.Views.View decor, WindowInsets insets)
    {
        try
        {
            var visible = insets.IsVisible(WindowInsets.Type.Ime());
            var chosenByUser = resumed && decor.HasWindowFocus;
            var wanted = visible || (keyboardWanted && !chosenByUser);
            if (wanted != keyboardWanted)
            {
                keyboardWanted = wanted;
                Log.WantedChanged(logger, wanted, visible, resumed, decor.HasWindowFocus);
            }
        }
        catch (Exception exception)
        {
            Log.RestoreFailed(logger, exception);
        }
    }

    private void ShowWhenFocused(Android.Webkit.WebView webView, int forGeneration)
    {
        if (webView.HasWindowFocus)
        {
            Post(webView, forGeneration);
            return;
        }

        var observer = webView.ViewTreeObserver;
        if (observer is null)
        {
            return;
        }

        focusObserver = observer;
        focusListener = new FocusListener(this, logger, webView, forGeneration);
        observer.AddOnWindowFocusChangeListener(focusListener);
    }

    private void Post(Android.Webkit.WebView webView, int forGeneration) => webView.Post(() =>
    {
        try
        {
            if (forGeneration != generation || !resumed)
            {
                return;
            }

            var textEditor = webView.OnCheckIsTextEditor();
            Log.Decided(logger, textEditor);
            if (textEditor)
            {
                (webView.Context?.GetSystemService(Context.InputMethodService) as InputMethodManager)?.ShowSoftInput(webView, ShowFlags.Implicit);
            }
        }
        catch (Exception exception)
        {
            Log.RestoreFailed(logger, exception);
        }
    });

    public void Dispose() => RemoveFocusListener();

    private void RemoveFocusListener()
    {
        if (focusListener is not null && focusObserver is { IsAlive: true })
        {
            focusObserver.RemoveOnWindowFocusChangeListener(focusListener);
        }

        focusListener = null;
        focusObserver = null;
    }

    private void Clear()
    {
        RemoveFocusListener();
        current?.Window?.DecorView?.SetOnApplyWindowInsetsListener(null);
        current = null;
        resumed = false;
        keyboardWanted = false;
        generation++;
    }

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

    private sealed class InsetsListener(AndroidKeyboardRestore owner) : Java.Lang.Object, Android.Views.View.IOnApplyWindowInsetsListener
    {
        public WindowInsets OnApplyWindowInsets(Android.Views.View v, WindowInsets insets)
        {
            owner.ImeChanged(v, insets);
            return v.OnApplyWindowInsets(insets) ?? insets;
        }
    }

    private sealed class FocusListener(AndroidKeyboardRestore owner, ILogger logger, Android.Webkit.WebView webView, int forGeneration)
        : Java.Lang.Object, ViewTreeObserver.IOnWindowFocusChangeListener
    {
        public void OnWindowFocusChanged(bool hasFocus)
        {
            try
            {
                if (!hasFocus)
                {
                    return;
                }

                Log.FocusRegained(logger);
                owner.RemoveFocusListener();
                owner.Post(webView, forGeneration);
            }
            catch (Exception exception)
            {
                Log.RestoreFailed(logger, exception);
            }
        }
    }

    private static partial class Log
    {
        [LoggerMessage((int)AppEvent.KeyboardRestoreFailed, LogLevel.Warning, "Restoring the keyboard after the app returned failed")]
        public static partial void RestoreFailed(ILogger logger, Exception exception);

        [LoggerMessage((int)AppEvent.KeyboardPaused, LogLevel.Information, "App paused, keyboard wanted {Wanted}, window focus {WindowFocus}")]
        public static partial void Paused(ILogger logger, bool wanted, bool windowFocus);

        [LoggerMessage((int)AppEvent.KeyboardResumed, LogLevel.Information, "App resumed, keyboard wanted {Wanted}, window focus {WindowFocus}")]
        public static partial void Resumed(ILogger logger, bool wanted, bool windowFocus);

        [LoggerMessage((int)AppEvent.KeyboardWindowFocusRegained, LogLevel.Information, "Window focus regained while waiting to restore the keyboard")]
        public static partial void FocusRegained(ILogger logger);

        [LoggerMessage((int)AppEvent.KeyboardRestoreDecided, LogLevel.Information, "Keyboard restore, text input focused {TextInputFocused}")]
        public static partial void Decided(ILogger logger, bool textInputFocused);

        [LoggerMessage((int)AppEvent.KeyboardWantedChanged, LogLevel.Information, "Keyboard wanted {Wanted}, keyboard visible {Visible}, resumed {Resumed}, window focus {WindowFocus}")]
        public static partial void WantedChanged(ILogger logger, bool wanted, bool visible, bool resumed, bool windowFocus);
    }
}
