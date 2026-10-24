namespace WorkoutLog.UI.Lifecycle;

/// <summary>Runs the handlers registered through <see cref="IAppStart"/> when the activity starts, and removes them when it's destroyed or replaced. Used by the host.</summary>
public interface IAppLifecycle
{
    /// <summary>Removes any earlier activity's handlers and makes this activity current. Called when an activity is created, before its pages.</summary>
    void Attach(object activity);

    /// <summary>Runs every registered handler. Called when the activity starts.</summary>
    Task NotifyStartedAsync();

    /// <summary>Removes every handler if the activity is current. Called when an activity, and the pages in it, are destroyed.</summary>
    void Detach(object activity);
}
