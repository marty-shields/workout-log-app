using Microsoft.Extensions.Logging.Abstractions;
using Shields.UI.Navigation;
using WorkoutLog.App.Navigation;

namespace WorkoutLog.UI.Tests.Navigation;

/// <summary>The activity-to-page hand-over of the Android back callback, with the kit's real <see cref="BackButton"/>.</summary>
public class BackButtonLinkTests
{
    private BackButton page = null!;
    private object activity = null!;
    private BackButtonLink link = null!;
    private int updates;

    [SetUp]
    public void SetUp()
    {
        page = NewBackButton();
        activity = new object();
        updates = 0;
        link = new BackButtonLink(() => updates++);
    }

    private static BackButton NewBackButton() => new(NullLogger<BackButton>.Instance);

    [Test]
    public void Enabled_IsTrueOnlyWhileAHandlerIsRegistered()
    {
        Assert.That(link.Enabled, Is.False, "nothing connected");
        link.Connect(page);
        Assert.That(link.Enabled, Is.False, "connected, no handler");

        var registration = page.Register(() => Task.CompletedTask);
        Assert.That(link.Enabled, Is.True);

        registration.Dispose();
        Assert.That(link.Enabled, Is.False);
    }

    [Test]
    public void Connect_UpdatesWhenAHandlerIsRegisteredOrRemoved()
    {
        link.Connect(page);
        updates = 0;

        var registration = page.Register(() => Task.CompletedTask);
        registration.Dispose();

        Assert.That(updates, Is.EqualTo(2));
    }

    [Test]
    public void Connect_StopsFollowingTheEarlierBackButton()
    {
        var earlier = NewBackButton();
        link.Connect(earlier);
        link.Connect(page);
        updates = 0;

        earlier.Register(() => Task.CompletedTask);

        Assert.That(updates, Is.Zero);
        Assert.That(link.Enabled, Is.False);
    }

    [Test]
    public void Connect_AfterAttach_AttachesTheBackButtonToTheActivity()
    {
        link.Attach(activity);
        link.Connect(page);
        page.Register(() => Task.CompletedTask);

        link.Detach(activity);

        Assert.That(page.HasHandler, Is.False, "Detach clears the handlers only of the attached activity");
    }

    [Test]
    public void Attach_AfterConnect_AttachesTheBackButtonAndUpdates()
    {
        link.Connect(page);
        page.Register(() => Task.CompletedTask);
        updates = 0;

        link.Attach(activity);

        Assert.That(page.HasHandler, Is.False, "attaching removes the earlier handlers");
        Assert.That(updates, Is.GreaterThan(0));
    }

    [Test]
    public void Disconnect_OfTheConnectedBackButton_StopsFollowingAndUpdates()
    {
        link.Connect(page);
        page.Register(() => Task.CompletedTask);
        updates = 0;

        link.Disconnect(page);

        Assert.That(link.Enabled, Is.False);
        Assert.That(updates, Is.EqualTo(1));
        page.Register(() => Task.CompletedTask);
        Assert.That(updates, Is.EqualTo(1), "no longer subscribed");
    }

    [Test]
    public void Disconnect_OfAStaleBackButton_KeepsTheCurrentOne()
    {
        var stale = NewBackButton();
        link.Connect(stale);
        link.Connect(page);
        page.Register(() => Task.CompletedTask);
        updates = 0;

        link.Disconnect(stale);

        Assert.That(link.Enabled, Is.True);
        Assert.That(updates, Is.Zero);
    }

    [Test]
    public void Detach_OfAnEarlierActivity_IsIgnored()
    {
        var earlier = new object();
        link.Attach(earlier);
        link.Attach(activity);
        link.Connect(page);
        page.Register(() => Task.CompletedTask);

        Assert.That(link.Detach(earlier), Is.False);
        Assert.That(page.HasHandler, Is.True);
        Assert.That(link.Detach(activity), Is.True);
        Assert.That(page.HasHandler, Is.False);
    }

    [Test]
    public async Task TryHandle_RunsTheNewestHandlerOnlyWhenThereIsOne()
    {
        var handled = 0;
        link.Connect(page);
        Assert.That(link.TryHandle(), Is.False);

        page.Register(() =>
        {
            handled++;
            return Task.CompletedTask;
        });

        Assert.That(link.TryHandle(), Is.True);
        await Task.Yield();
        Assert.That(handled, Is.EqualTo(1));
    }
}
