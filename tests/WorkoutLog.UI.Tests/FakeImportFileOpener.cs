using WorkoutLog.Core.Services;
using WorkoutLog.UI.Import;

namespace WorkoutLog.UI.Tests;

/// <summary>Counts each open; each waits until the test completes or fails it.</summary>
internal sealed class FakeImportFileOpener : IImportFileOpener
{
    private TaskCompletionSource<ImportReview?> pending = new();

    public int Calls { get; private set; }

    public Task<ImportReview?> OpenAsync()
    {
        Calls++;
        pending = new TaskCompletionSource<ImportReview?>();
        return pending.Task;
    }

    /// <summary>Completes the latest open with <paramref name="review"/>; null as a cancelled picker.</summary>
    public void Complete(ImportReview? review) => pending.SetResult(review);

    /// <summary>Fails the latest open.</summary>
    public void Fail(Exception exception) => pending.SetException(exception);
}
