using Microsoft.Extensions.Logging.Abstractions;
using Shields.UI.Lifecycle;
using WorkoutLog.App.Lifecycle;

namespace WorkoutLog.UI.Tests.Lifecycle;

/// <summary>The activity-to-page hand-over, with the kit's real <see cref="AppLifecycle"/>: a handler runs on start only if it registered after the lifecycle was attached.</summary>
public class AndroidAppLifecycleTests
{
    private AndroidAppLifecycle android = null!;
    private AppLifecycle scoped = null!;
    private object activity = null!;
    private int started;

    [SetUp]
    public void SetUp()
    {
        android = new AndroidAppLifecycle();
        scoped = new AppLifecycle(NullLogger<AppLifecycle>.Instance);
        activity = new object();
        started = 0;
    }

    private void Register(AppLifecycle lifecycle) => lifecycle.Register(() =>
    {
        started++;
        return Task.CompletedTask;
    });

    [Test]
    public void NotifyStarted_WithNothingConnected_DoesNothing()
    {
        android.Attach(activity);

        Assert.DoesNotThrow(android.NotifyStarted);
    }

    [Test]
    public void NotifyStarted_RunsTheConnectedLifecyclesHandlers()
    {
        android.Connect(scoped);
        Register(scoped);

        android.NotifyStarted();

        Assert.That(started, Is.EqualTo(1));
    }

    [Test]
    public void Connect_AfterAttach_AttachesTheScopedLifecycleToTheActivity()
    {
        android.Attach(activity);
        Register(scoped);

        android.Connect(scoped);
        android.NotifyStarted();
        Register(scoped);
        android.Detach(activity);
        android.NotifyStarted();

        Assert.That(started, Is.Zero, "attaching cleared the earlier handler, and the activity is current so Detach cleared the later one");
    }

    [Test]
    public void Connect_BeforeAnyActivity_LeavesTheHandlersAlone()
    {
        Register(scoped);

        android.Connect(scoped);
        android.NotifyStarted();

        Assert.That(started, Is.EqualTo(1));
    }

    [Test]
    public void Attach_AfterConnect_AttachesTheScopedLifecycle()
    {
        android.Connect(scoped);
        Register(scoped);

        android.Attach(activity);
        android.NotifyStarted();

        Assert.That(started, Is.Zero);
    }

    [Test]
    public void Disconnect_OfTheConnectedLifecycle_StopsNotifying()
    {
        android.Connect(scoped);
        Register(scoped);

        android.Disconnect(scoped);
        android.NotifyStarted();

        Assert.That(started, Is.Zero);
    }

    [Test]
    public void Disconnect_OfAStaleLifecycle_KeepsTheCurrentOne()
    {
        var stale = new AppLifecycle(NullLogger<AppLifecycle>.Instance);
        android.Connect(stale);
        android.Connect(scoped);
        Register(scoped);

        android.Disconnect(stale);
        android.NotifyStarted();

        Assert.That(started, Is.EqualTo(1));
    }

    [Test]
    public void Detach_OfTheCurrentActivity_ClearsTheHandlersAndForgetsTheActivity()
    {
        android.Attach(activity);
        android.Connect(scoped);
        Register(scoped);

        android.Detach(activity);
        android.NotifyStarted();
        var next = new AppLifecycle(NullLogger<AppLifecycle>.Instance);
        Register(next);
        android.Connect(next);
        android.NotifyStarted();

        Assert.That(started, Is.EqualTo(1), "the first handler was cleared; the second stayed, since no activity was left to attach to");
    }

    [Test]
    public void Detach_OfAnEarlierActivity_IsIgnored()
    {
        var earlier = new object();
        android.Attach(earlier);
        android.Attach(activity);
        android.Connect(scoped);
        Register(scoped);

        android.Detach(earlier);
        android.NotifyStarted();
        var next = new AppLifecycle(NullLogger<AppLifecycle>.Instance);
        Register(next);
        android.Connect(next);
        android.NotifyStarted();

        Assert.That(started, Is.EqualTo(1), "the handler stayed after Detach; the current activity was kept, so connecting cleared the next lifecycle's");
    }
}
