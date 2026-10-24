using AndroidX.Activity;
using WorkoutLog.UI.Navigation;

namespace WorkoutLog.App.Navigation;

/// <summary>
/// Passes the Android back button to <see cref="IBackButton"/> while a handler is registered.
/// Without one, the callback is off and Android handles back: the app closes.
/// </summary>
internal sealed class AndroidBackButton(IBackButton backButton)
{
    private ComponentActivity? current;
    private Callback? callback;

    /// <summary>Adds the back callback to a created activity, which replaces any earlier one.</summary>
    public void Attach(ComponentActivity activity)
    {
        if (current is null)
        {
            backButton.Changed += Update;
        }

        current = activity;
        callback = new Callback(backButton, activity.OnBackPressedDispatcher);
        activity.OnBackPressedDispatcher.AddCallback(activity, callback);
        backButton.Attach(activity);
        Update();
    }

    /// <summary>
    /// Stops following the handlers when the current activity is destroyed, and removes them: its pages are not disposed with it.
    /// An earlier activity is ignored. The dispatcher removes the callback itself.
    /// </summary>
    public void Detach(Android.App.Activity activity)
    {
        if (!ReferenceEquals(activity, current))
        {
            return;
        }

        backButton.Changed -= Update;
        current = null;
        callback = null;
        backButton.Detach(activity);
    }

    private void Update() => MainThread.BeginInvokeOnMainThread(() =>
    {
        if (callback is not null)
        {
            callback.Enabled = backButton.HasHandler;
        }
    });

    private sealed class Callback(IBackButton backButton, OnBackPressedDispatcher dispatcher) : OnBackPressedCallback(false)
    {
        // The handler can be removed between the gesture starting and back being pressed; Android then handles it.
        public override void HandleOnBackPressed()
        {
            if (backButton.HasHandler)
            {
                _ = backButton.HandleAsync();
                return;
            }

            Enabled = false;
            dispatcher.OnBackPressed();
        }
    }
}
