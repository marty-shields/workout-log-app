using Microsoft.Extensions.Logging;
using WorkoutLog.UI.Logging;

namespace WorkoutLog.UI.Lifecycle;

/// <summary>Keeps the handlers that run when the activity starts.</summary>
public sealed partial class AppLifecycle(ILogger<AppLifecycle> logger) : IAppStart, IAppLifecycle
{
    private readonly List<Registration> handlers = [];
    private readonly Lock gate = new();
    private object? current;

    /// <inheritdoc />
    public IDisposable Register(Func<Task> handler)
    {
        var registration = new Registration(this, handler);
        lock (gate)
        {
            handlers.Add(registration);
        }

        return registration;
    }

    /// <inheritdoc />
    /// <remarks>Runs the handlers one after another. A handler that throws is logged and the others still run.</remarks>
    public async Task NotifyStartedAsync()
    {
        Registration[] registered;
        lock (gate)
        {
            registered = [.. handlers];
        }

        foreach (var registration in registered)
        {
            try
            {
                await registration.Handler();
            }
            catch (Exception exception)
            {
                Log.HandlerFailed(logger, exception);
            }
        }
    }

    /// <inheritdoc />
    public void Attach(object activity)
    {
        lock (gate)
        {
            current = activity;
            handlers.Clear();
        }
    }

    /// <inheritdoc />
    public void Detach(object activity)
    {
        lock (gate)
        {
            if (ReferenceEquals(current, activity))
            {
                current = null;
                handlers.Clear();
            }
        }
    }

    private void Remove(Registration registration)
    {
        lock (gate)
        {
            handlers.Remove(registration);
        }
    }

    private sealed class Registration(AppLifecycle owner, Func<Task> handler) : IDisposable
    {
        public Func<Task> Handler { get; } = handler;

        public void Dispose() => owner.Remove(this);
    }

    private static partial class Log
    {
        [LoggerMessage((int)UiEvent.AppStartHandlerFailed, LogLevel.Error, "App start handler failed")]
        public static partial void HandlerFailed(ILogger logger, Exception exception);
    }
}
