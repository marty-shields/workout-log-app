using Shields.UI.Lifecycle;

namespace WorkoutLog.App.Lifecycle;

/// <summary>
/// Passes the activity's create, start and destroy to the kit's <see cref="IAppLifecycle"/>, which runs the pages' app start handlers.
/// The lifecycle is scoped to the BlazorWebView, so <c>ActivityBridge</c> hands it over with <see cref="Connect"/>
/// once the page exists, which is after the activity is created and started.
/// </summary>
internal sealed class AndroidAppLifecycle
{
    private object? current;
    private IAppLifecycle? lifecycle;

    /// <summary>Makes a created activity current, which replaces any earlier one.</summary>
    public void Attach(object activity)
    {
        current = activity;
        lifecycle?.Attach(activity);
    }

    /// <summary>Follows the page's lifecycle, which replaces any earlier one, and attaches it to the current activity.</summary>
    public void Connect(IAppLifecycle scoped)
    {
        lifecycle = scoped;
        if (current is not null)
        {
            scoped.Attach(current);
        }
    }

    /// <summary>Stops following the lifecycle when its page is disposed. A lifecycle that is not the connected one is ignored.</summary>
    public void Disconnect(IAppLifecycle scoped)
    {
        if (ReferenceEquals(scoped, lifecycle))
        {
            lifecycle = null;
        }
    }

    /// <summary>Runs the app start handlers, if the page is connected.</summary>
    public void NotifyStarted() => _ = lifecycle?.NotifyStartedAsync();

    /// <summary>Removes the handlers when the current activity is destroyed. An earlier activity is ignored.</summary>
    public void Detach(object activity)
    {
        if (ReferenceEquals(activity, current))
        {
            current = null;
            lifecycle?.Detach(activity);
        }
    }
}
