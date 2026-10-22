using WorkoutLog.Core.Data;

namespace WorkoutLog.Core.Tests.Data;

public class DatabaseGateTests
{
    private readonly DatabaseGate gate = new();

    [Test]
    public async Task RunAsync_WhileOtherWorkRuns_WaitsForIt()
    {
        var release = new TaskCompletionSource();
        var first = gate.RunAsync(() => release.Task);
        var secondStarted = false;

        var second = gate.RunAsync(() =>
        {
            secondStarted = true;
            return Task.CompletedTask;
        });

        Assert.That(secondStarted, Is.False);
        release.SetResult();
        await Task.WhenAll(first, second);
        Assert.That(secondStarted, Is.True);
    }

    [Test]
    public async Task RunAsync_ReturnsTheResult()
    {
        Assert.That(await gate.RunAsync(() => Task.FromResult(42)), Is.EqualTo(42));
    }

    [Test]
    public async Task RunAsync_WorkThrows_RethrowsAndLetsTheNextWorkRun()
    {
        Assert.That(() => gate.RunAsync(() => Task.FromException(new InvalidOperationException())), Throws.InvalidOperationException);

        Assert.That(await gate.RunAsync(() => Task.FromResult(1)), Is.EqualTo(1));
    }
}
