using Microsoft.Extensions.Logging;

namespace WorkoutLog.UI.Navigation;

/// <summary>Keeps the registered back handlers, newest last.</summary>
public sealed partial class BackButton(ILogger<BackButton> logger) : IBackButton
{
    private readonly List<Registration> handlers = [];
    private readonly Lock gate = new();

    /// <inheritdoc />
    public bool HasHandler
    {
        get
        {
            lock (gate)
            {
                return handlers.Count > 0;
            }
        }
    }

    /// <inheritdoc />
    public event Action? Changed;

    /// <inheritdoc />
    public IDisposable Register(Func<Task> handler)
    {
        var registration = new Registration(this, handler);
        lock (gate)
        {
            handlers.Add(registration);
        }

        Changed?.Invoke();
        return registration;
    }

    /// <inheritdoc />
    public async Task HandleAsync()
    {
        Func<Task>? handler;
        lock (gate)
        {
            handler = handlers.Count > 0 ? handlers[^1].Handler : null;
        }

        if (handler is null)
        {
            Log.NotHandled(logger);
            return;
        }

        Log.Handled(logger);
        try
        {
            await handler();
        }
        catch (Exception exception)
        {
            Log.Failed(logger, exception);
        }
    }

    /// <inheritdoc />
    public void Clear()
    {
        bool cleared;
        lock (gate)
        {
            cleared = handlers.Count > 0;
            handlers.Clear();
        }

        if (cleared)
        {
            Changed?.Invoke();
        }
    }

    private void Remove(Registration registration)
    {
        bool removed;
        lock (gate)
        {
            removed = handlers.Remove(registration);
        }

        if (removed)
        {
            Changed?.Invoke();
        }
    }

    private sealed class Registration(BackButton owner, Func<Task> handler) : IDisposable
    {
        public Func<Task> Handler { get; } = handler;

        public void Dispose() => owner.Remove(this);
    }

    private static partial class Log
    {
        [LoggerMessage(2201, LogLevel.Debug, "Back pressed: handled by the page")]
        public static partial void Handled(ILogger logger);

        [LoggerMessage(2202, LogLevel.Debug, "Back pressed: no handler")]
        public static partial void NotHandled(ILogger logger);

        [LoggerMessage(2203, LogLevel.Error, "Back handler failed")]
        public static partial void Failed(ILogger logger, Exception exception);
    }
}
