using Moq;
using WorkoutLog.Core.Data;

namespace WorkoutLog.Core.Tests.Data;

public class GatedUnitOfWorkMockTests
{
    [Test]
    public async Task RunAsync_PassesTheWriteToTheInner()
    {
        Action<IWorkoutWriter> write = _ => { };
        var inner = new Mock<IUnitOfWork>(MockBehavior.Strict);
        inner.Setup(u => u.RunAsync(write)).Returns(Task.CompletedTask);

        await new GatedUnitOfWork(inner.Object, new DatabaseGate()).RunAsync(write);

        inner.Verify(u => u.RunAsync(write), Times.Once);
    }

    [Test]
    public async Task RunAsync_WhileTheGateIsHeld_DoesNotReachTheInner()
    {
        var inner = new Mock<IUnitOfWork>();
        var gate = new DatabaseGate();
        var release = new TaskCompletionSource();
        var holder = gate.RunAsync(() => release.Task);

        var pending = new GatedUnitOfWork(inner.Object, gate).RunAsync(_ => { });

        Assert.That(pending.IsCompleted, Is.False);
        Assert.That(inner.Invocations, Is.Empty);
        release.SetResult();
        await Task.WhenAll(holder, pending);
        Assert.That(inner.Invocations, Has.Count.EqualTo(1));
    }
}
