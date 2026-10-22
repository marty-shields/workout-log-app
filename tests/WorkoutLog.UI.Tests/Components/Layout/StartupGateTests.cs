using WorkoutLog.UI.Components.Layout;

namespace WorkoutLog.UI.Tests.Components.Layout;

public class StartupGateTests : ComponentTestBase
{
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
