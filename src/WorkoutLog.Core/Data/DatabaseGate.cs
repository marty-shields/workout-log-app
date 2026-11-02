namespace WorkoutLog.Core.Data;

/// <summary>Runs database work one call at a time.</summary>
internal sealed class DatabaseGate
{
    private readonly SemaphoreSlim semaphore = new(1, 1);

    /// <summary>Waits for earlier work to finish, then runs <paramref name="work"/>.</summary>
    public async Task<T> RunAsync<T>(Func<Task<T>> work)
    {
        await semaphore.WaitAsync();
        try
        {
            return await work();
        }
        finally
        {
            semaphore.Release();
        }
    }

    /// <summary>Waits for earlier work to finish, then runs <paramref name="work"/>.</summary>
    public Task RunAsync(Func<Task> work) =>
        RunAsync(async () =>
        {
            await work();
            return true;
        });
}
