using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Time.Testing;
using WorkoutLog.UI.Components.Feedback;

namespace WorkoutLog.UI.Tests.Components.Feedback;

public class SnackbarTests : ComponentTestBase
{
    private FakeTimeProvider clock = null!;

    [SetUp]
    public void UseClock()
    {
        clock = new FakeTimeProvider();
        Context.Services.AddSingleton<TimeProvider>(clock);
    }

    [Test]
    public void Render_ShowsTheMessageAndUndo()
    {
        var cut = Context.Render<Snackbar>(p => p.Add(x => x.Message, "Set 2 deleted"));

        Assert.Multiple(() =>
        {
            Assert.That(cut.Find("[data-testid=snackbar-message]").TextContent, Is.EqualTo("Set 2 deleted"));
            Assert.That(cut.Find("[data-testid=snackbar-undo]").TextContent, Is.EqualTo("Undo"));
        });
    }

    [Test]
    public void Undo_RaisesOnUndo()
    {
        var undone = 0;
        var cut = Context.Render<Snackbar>(p => p
            .Add(x => x.Message, "Set 2 deleted")
            .Add(x => x.OnUndo, () => undone++));

        cut.Find("[data-testid=snackbar-undo]").Click();

        Assert.That(undone, Is.EqualTo(1));
    }

    [Test]
    public async Task Duration_RaisesOnTimeoutOnce()
    {
        var timedOut = 0;
        Context.Render<Snackbar>(p => p
            .Add(x => x.Message, "Set 2 deleted")
            .Add(x => x.OnTimeout, () => timedOut++));

        clock.Advance(Snackbar.Duration - TimeSpan.FromMilliseconds(1));
        await WaitForRendererAsync();
        var early = timedOut;
        clock.Advance(TimeSpan.FromMilliseconds(1));
        await WaitForRendererAsync();
        clock.Advance(Snackbar.Duration);
        await WaitForRendererAsync();

        Assert.Multiple(() =>
        {
            Assert.That(early, Is.Zero);
            Assert.That(timedOut, Is.EqualTo(1));
        });
    }

    [Test]
    public async Task RemovedFirst_NeverTimesOut()
    {
        var timedOut = 0;
        var cut = Context.Render<Snackbar>(p => p
            .Add(x => x.Message, "Set 2 deleted")
            .Add(x => x.OnTimeout, () => timedOut++));

        cut.Instance.Dispose();
        clock.Advance(Snackbar.Duration);
        await WaitForRendererAsync();

        Assert.That(timedOut, Is.Zero);
    }

    [Test]
    public void Duration_IsWithinMaterialsFourToTenSeconds()
    {
        Assert.That(Snackbar.Duration, Is.InRange(TimeSpan.FromSeconds(4), TimeSpan.FromSeconds(10)));
    }
}
