using Shields.UI.Navigation;

namespace WorkoutLog.App.Navigation;

/// <summary>
/// Keeps which activity and which kit <see cref="IBackButton"/> are current for <c>AndroidBackButton</c>, with no Android types.
/// <c>update</c> is called whenever <see cref="Enabled"/> may have changed.
/// </summary>
internal sealed class BackButtonLink(Action update)
{
    private object? current;
    private IBackButton? backButton;

    /// <summary>True while the connected back button has a handler: only then is the Android callback on.</summary>
    public bool Enabled => backButton?.HasHandler ?? false;

    /// <summary>Makes a created activity current, which replaces any earlier one.</summary>
    public void Attach(object activity)
    {
        current = activity;
        backButton?.Attach(activity);
        update();
    }

    /// <summary>Follows the page's back button, which replaces any earlier one, and attaches it to the current activity.</summary>
    public void Connect(IBackButton scoped)
    {
        if (backButton is not null)
        {
            backButton.Changed -= update;
        }

        backButton = scoped;
        scoped.Changed += update;
        if (current is not null)
        {
            scoped.Attach(current);
        }

        update();
    }

    /// <summary>Stops following the back button when its page is disposed. A back button that is not the connected one is ignored.</summary>
    public void Disconnect(IBackButton scoped)
    {
        if (!ReferenceEquals(scoped, backButton))
        {
            return;
        }

        scoped.Changed -= update;
        backButton = null;
        update();
    }

    /// <summary>Runs the connected back button's newest handler. Returns false, doing nothing, when there is none.</summary>
    public bool TryHandle()
    {
        if (backButton is not { HasHandler: true } connected)
        {
            return false;
        }

        _ = connected.HandleAsync();
        return true;
    }

    /// <summary>Forgets the current activity and detaches the back button from it. An earlier activity is ignored. Returns whether it was the current one.</summary>
    public bool Detach(object activity)
    {
        if (!ReferenceEquals(activity, current))
        {
            return false;
        }

        current = null;
        backButton?.Detach(activity);
        return true;
    }
}
