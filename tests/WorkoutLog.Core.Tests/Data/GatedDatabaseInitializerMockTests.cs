using Moq;
using WorkoutLog.Core.Data;

namespace WorkoutLog.Core.Tests.Data;

public class GatedDatabaseInitializerMockTests
{
    [Test]
    public async Task InitializeAsync_CallsTheInner()
    {
        var inner = new Mock<IDatabaseInitializer>(MockBehavior.Strict);
        inner.Setup(i => i.InitializeAsync()).Returns(Task.CompletedTask);

        await new GatedDatabaseInitializer(inner.Object, new DatabaseGate()).InitializeAsync();

        inner.Verify(i => i.InitializeAsync(), Times.Once);
    }

    [Test]
    public async Task InitializeAsync_WhileTheGateIsHeld_DoesNotReachTheInner()
    {
        var inner = new Mock<IDatabaseInitializer>();
        var gate = new DatabaseGate();
        var release = new TaskCompletionSource();
        var holder = gate.RunAsync(() => release.Task);

        var pending = new GatedDatabaseInitializer(inner.Object, gate).InitializeAsync();

        Assert.That(pending.IsCompleted, Is.False);
        inner.Verify(i => i.InitializeAsync(), Times.Never);
        release.SetResult();
        await Task.WhenAll(holder, pending);
        inner.Verify(i => i.InitializeAsync(), Times.Once);
    }
}
