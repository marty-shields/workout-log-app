namespace WorkoutLog.UI.Navigation;

/// <summary>Passes the Android back button to the page that registered for it.</summary>
public interface IBackButton
{
    /// <summary>True when a handler is registered, so back stays in the app.</summary>
    bool HasHandler { get; }

    /// <summary>Raised when a handler is registered or removed.</summary>
    event Action? Changed;

    /// <summary>Registers a handler for back. The newest handler runs; disposing the result removes it.</summary>
    IDisposable Register(Func<Task> handler);

    /// <summary>Runs the newest handler. Does nothing when none is registered.</summary>
    Task HandleAsync();

    /// <summary>Removes every handler. Called when the activity, and the pages in it, are destroyed.</summary>
    void Clear();
}
