using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using WorkoutLog.UI.Components.Layout;
using WorkoutLog.UI.Logging;

namespace WorkoutLog.UI.Tests.Components.Layout;

public class StartupGateTests : ComponentTestBase
{
    private FakeLogger<StartupGate> logger = null!;

    [SetUp]
    public void AddLogger()
    {
        logger = new FakeLogger<StartupGate>();
        Context.Services.AddSingleton<ILogger<StartupGate>>(logger);
    }

    [Test]
    public void Pending_ShowsLoadingAndNotContent()
    {
        var init = new TaskCompletionSource();

        var cut = Render(() => init.Task);

        Assert.Multiple(() =>
        {
            Assert.That(cut.FindAll("[data-testid=startup-loading]"), Has.Count.EqualTo(1));
            Assert.That(cut.FindAll("[data-testid=content]"), Is.Empty);
        });
    }

    [Test]
    public async Task Completed_ShowsContent()
    {
        var init = new TaskCompletionSource();
        var cut = Render(() => init.Task);

        await cut.InvokeAsync(init.SetResult);

        cut.WaitForAssertion(() =>
        {
            Assert.That(cut.FindAll("[data-testid=content]"), Has.Count.EqualTo(1));
            Assert.That(cut.FindAll("[data-testid=startup-loading]"), Is.Empty);
        });
    }

    [Test]
    public async Task FailsLater_ShowsErrorAndNotContent()
    {
        var init = new TaskCompletionSource();
        var cut = Render(() => init.Task);

        await cut.InvokeAsync(() => init.SetException(new InvalidOperationException()));

        cut.WaitForAssertion(() =>
        {
            Assert.That(cut.FindAll("[data-testid=startup-error]"), Has.Count.EqualTo(1));
            Assert.That(cut.FindAll("[data-testid=content]"), Is.Empty);
        });
    }

    [Test]
    public void Throws_ShowsErrorAndNotContent()
    {
        var cut = Render(() => Task.FromException(new InvalidOperationException()));

        Assert.Multiple(() =>
        {
            Assert.That(cut.FindAll("[data-testid=startup-error]"), Has.Count.EqualTo(1));
            Assert.That(cut.FindAll("[data-testid=content]"), Is.Empty);
        });
    }

    [Test]
    public void Completed_LogsTheDuration()
    {
        Render(() => Task.CompletedTask);

        var logged = LoggedEvents.Of(logger).Single();
        Assert.That((logged.Id, logged.Level), Is.EqualTo((UiEvent.StartupFinished, LogLevel.Information)));
        Assert.That(logged.Message, Does.Match(@"^Startup finished in \d+ ms$"));
    }

    [Test]
    public void Throws_LogsTheException()
    {
        var exception = new InvalidOperationException();

        Render(() => Task.FromException(exception));

        Assert.That(LoggedEvents.Of(logger), Is.EqualTo(new[] { (UiEvent.StartupFailed, LogLevel.Error, "Startup failed") }));
        Assert.That(logger.LatestRecord.Exception, Is.SameAs(exception));
    }

    [Test]
    public void Rerender_RunsInitializeOnce()
    {
        var calls = 0;
        var cut = Render(() =>
        {
            calls++;
            return Task.CompletedTask;
        });

        cut.Render();

        Assert.That(calls, Is.EqualTo(1));
    }

    private IRenderedComponent<StartupGate> Render(Func<Task> initialize) =>
        Context.Render<StartupGate>(p => p
            .Add(x => x.Initialize, initialize)
            .AddChildContent("<p data-testid=\"content\">App</p>"));
}
