using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using WorkoutLog.UI.Logging;
using WorkoutLog.UI.Navigation;

namespace WorkoutLog.UI.Tests.Navigation;

public class BackButtonTests
{
    private FakeLogger<BackButton> logger = null!;
    private BackButton backButton = null!;

    [SetUp]
    public void Create()
    {
        logger = new FakeLogger<BackButton>();
        backButton = new BackButton(logger);
    }

    [Test]
    public async Task NoHandler_HasNoHandlerAndLogsNotHandled()
    {
        await backButton.HandleAsync();

        Assert.Multiple(() =>
        {
            Assert.That(backButton.HasHandler, Is.False);
            Assert.That(LoggedEvents.Of(logger), Is.EqualTo(new[] { (UiEvent.BackNotHandled, LogLevel.Debug, "Back pressed: no handler") }));
        });
    }

    [Test]
    public async Task Register_RunsTheHandlerAndLogsHandled()
    {
        var calls = 0;
        backButton.Register(() => { calls++; return Task.CompletedTask; });

        await backButton.HandleAsync();

        Assert.Multiple(() =>
        {
            Assert.That(backButton.HasHandler, Is.True);
            Assert.That(calls, Is.EqualTo(1));
            Assert.That(LoggedEvents.Of(logger), Is.EqualTo(new[] { (UiEvent.BackHandled, LogLevel.Debug, "Back pressed: handled by the page") }));
        });
    }

    [Test]
    public async Task TwoHandlers_RunsOnlyTheNewest()
    {
        var ran = new List<string>();
        backButton.Register(() => { ran.Add("page"); return Task.CompletedTask; });
        backButton.Register(() => { ran.Add("picker"); return Task.CompletedTask; });

        await backButton.HandleAsync();

        Assert.That(ran, Is.EqualTo(new[] { "picker" }));
    }

    [Test]
    public async Task DisposeNewest_RunsTheOneBefore()
    {
        var ran = new List<string>();
        backButton.Register(() => { ran.Add("page"); return Task.CompletedTask; });
        var picker = backButton.Register(() => { ran.Add("picker"); return Task.CompletedTask; });

        picker.Dispose();
        await backButton.HandleAsync();

        Assert.That(ran, Is.EqualTo(new[] { "page" }));
    }

    [Test]
    public void DisposeLast_HasNoHandler()
    {
        var registration = backButton.Register(() => Task.CompletedTask);

        registration.Dispose();

        Assert.That(backButton.HasHandler, Is.False);
    }

    [Test]
    public void Changed_RaisedOnRegisterAndOnFirstDisposeOnly()
    {
        var changes = 0;
        backButton.Changed += () => changes++;

        var registration = backButton.Register(() => Task.CompletedTask);
        registration.Dispose();
        registration.Dispose();

        Assert.That(changes, Is.EqualTo(2));
    }

    [Test]
    public async Task Attach_RemovesTheEarlierActivitysHandlersAndRaisesChangedOnce()
    {
        var calls = 0;
        var changes = 0;
        backButton.Attach(new object());
        backButton.Register(() => { calls++; return Task.CompletedTask; });
        var picker = backButton.Register(() => { calls++; return Task.CompletedTask; });
        backButton.Changed += () => changes++;

        backButton.Attach(new object());
        picker.Dispose();
        await backButton.HandleAsync();

        Assert.Multiple(() =>
        {
            Assert.That(backButton.HasHandler, Is.False);
            Assert.That(calls, Is.Zero);
            Assert.That(changes, Is.EqualTo(1));
        });
    }

    [Test]
    public void Attach_WithNoHandlers_DoesNotRaiseChanged()
    {
        var changes = 0;
        backButton.Changed += () => changes++;

        backButton.Attach(new object());

        Assert.That(changes, Is.Zero);
    }

    [Test]
    public async Task Detach_CurrentActivity_RemovesEveryHandlerAndRaisesChangedOnce()
    {
        var calls = 0;
        var changes = 0;
        var activity = new object();
        backButton.Attach(activity);
        backButton.Register(() => { calls++; return Task.CompletedTask; });
        backButton.Register(() => { calls++; return Task.CompletedTask; });
        backButton.Changed += () => changes++;

        backButton.Detach(activity);
        await backButton.HandleAsync();

        Assert.Multiple(() =>
        {
            Assert.That(backButton.HasHandler, Is.False);
            Assert.That(calls, Is.Zero);
            Assert.That(changes, Is.EqualTo(1));
        });
    }

    [Test]
    public async Task Detach_EarlierActivity_KeepsTheCurrentActivitysHandlers()
    {
        var calls = 0;
        var changes = 0;
        var earlier = new object();
        backButton.Attach(earlier);
        backButton.Attach(new object());
        backButton.Register(() => { calls++; return Task.CompletedTask; });
        backButton.Changed += () => changes++;

        backButton.Detach(earlier);
        await backButton.HandleAsync();

        Assert.Multiple(() =>
        {
            Assert.That(backButton.HasHandler, Is.True);
            Assert.That(calls, Is.EqualTo(1));
            Assert.That(changes, Is.Zero);
        });
    }

    [Test]
    public void Detach_WithNoHandlers_DoesNotRaiseChanged()
    {
        var changes = 0;
        var activity = new object();
        backButton.Attach(activity);
        backButton.Changed += () => changes++;

        backButton.Detach(activity);

        Assert.That(changes, Is.Zero);
    }

    [Test]
    public async Task HandlerThrows_LogsTheException()
    {
        backButton.Register(() => throw new InvalidOperationException());

        await backButton.HandleAsync();

        Assert.Multiple(() =>
        {
            Assert.That(LoggedEvents.Of(logger), Is.EqualTo(new[]
            {
                (UiEvent.BackHandled, LogLevel.Debug, "Back pressed: handled by the page"),
                (UiEvent.BackHandlerFailed, LogLevel.Error, "Back handler failed"),
            }));
            Assert.That(logger.LatestRecord.Exception, Is.TypeOf<InvalidOperationException>());
        });
    }
}
