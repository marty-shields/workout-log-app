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
    private readonly BackButtonLink link;
    private Callback? callback;

    public AndroidBackButton() => link = new BackButtonLink(Update);

    /// <summary>Adds the back callback to a created activity, which replaces any earlier one.</summary>
    public void Attach(ComponentActivity activity)
    {
        callback = new Callback(this, activity.OnBackPressedDispatcher);
        activity.OnBackPressedDispatcher.AddCallback(activity, callback);
        link.Attach(activity);
    }

    /// <summary>Follows the page's back button, which replaces any earlier one, and attaches it to the current activity.</summary>
    public void Connect(IBackButton scoped) => link.Connect(scoped);

    /// <summary>Stops following the back button when its page is disposed. A back button that is not the connected one is ignored.</summary>
    public void Disconnect(IBackButton scoped) => link.Disconnect(scoped);

    /// <summary>
    /// Removes the handlers when the current activity is destroyed: its pages are not disposed with it.
    /// An earlier activity is ignored. The dispatcher removes the callback itself.
    /// </summary>
    public void Detach(Android.App.Activity activity)
    {
        if (link.Detach(activity))
        {
            callback = null;
        }
    }

    private void Update() => MainThread.BeginInvokeOnMainThread(() =>
    {
        if (callback is not null)
        {
            callback.Enabled = link.Enabled;
        }
    });

    private sealed class Callback(AndroidBackButton owner, OnBackPressedDispatcher dispatcher) : OnBackPressedCallback(false)
    {
        // The handler can be removed between the gesture starting and back being pressed; Android then handles it.
        public override void HandleOnBackPressed()
        {
            if (owner.link.TryHandle())
            {
                return;
            }

            Enabled = false;
            dispatcher.OnBackPressed();
        }
    }
}
