using AndroidX.Activity;
using Shields.UI.Navigation;

namespace WorkoutLog.App.Navigation;

/// <summary>
/// Passes the Android back button to the kit's <see cref="IBackButton"/> while a handler is registered.
/// Without one, the callback is off and Android handles back: the app closes.
/// The back button is scoped to the BlazorWebView, so <see cref="Components.ActivityBridge"/> hands it over with <see cref="Connect"/>
/// once the page exists, which is after the activity is created.
/// </summary>
internal sealed class AndroidBackButton
{
    private ComponentActivity? current;
    private Callback? callback;
    private IBackButton? backButton;

    /// <summary>Adds the back callback to a created activity, which replaces any earlier one.</summary>
    public void Attach(ComponentActivity activity)
    {
        current = activity;
        callback = new Callback(this, activity.OnBackPressedDispatcher);
        activity.OnBackPressedDispatcher.AddCallback(activity, callback);
        backButton?.Attach(activity);
        Update();
    }

    /// <summary>Follows the page's back button, which replaces any earlier one, and attaches it to the current activity.</summary>
    public void Connect(IBackButton scoped)
    {
        if (backButton is not null)
        {
            backButton.Changed -= Update;
        }

        backButton = scoped;
        scoped.Changed += Update;
        if (current is not null)
        {
            scoped.Attach(current);
        }

        Update();
    }

    /// <summary>Stops following the back button when its page is disposed. A back button that is not the connected one is ignored.</summary>
    public void Disconnect(IBackButton scoped)
    {
        if (!ReferenceEquals(scoped, backButton))
        {
            return;
        }

        scoped.Changed -= Update;
        backButton = null;
        Update();
    }

    /// <summary>
    /// Removes the handlers when the current activity is destroyed: its pages are not disposed with it.
    /// An earlier activity is ignored. The dispatcher removes the callback itself.
    /// </summary>
    public void Detach(Android.App.Activity activity)
    {
        if (!ReferenceEquals(activity, current))
        {
            return;
        }

        current = null;
        callback = null;
        backButton?.Detach(activity);
    }

    private void Update() => MainThread.BeginInvokeOnMainThread(() =>
    {
        if (callback is not null)
        {
            callback.Enabled = backButton?.HasHandler ?? false;
        }
    });

    private sealed class Callback(AndroidBackButton owner, OnBackPressedDispatcher dispatcher) : OnBackPressedCallback(false)
    {
        // The handler can be removed between the gesture starting and back being pressed; Android then handles it.
        public override void HandleOnBackPressed()
        {
            if (owner.backButton is { HasHandler: true } backButton)
            {
                _ = backButton.HandleAsync();
                return;
            }

            Enabled = false;
            dispatcher.OnBackPressed();
        }
    }
}
