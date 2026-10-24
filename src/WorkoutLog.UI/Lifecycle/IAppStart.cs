namespace WorkoutLog.UI.Lifecycle;

/// <summary>Tells pages when the app comes back on screen.</summary>
public interface IAppStart
{
    /// <summary>Registers a handler that runs each time the activity starts. Disposing the result removes it.</summary>
    IDisposable Register(Func<Task> handler);
}
