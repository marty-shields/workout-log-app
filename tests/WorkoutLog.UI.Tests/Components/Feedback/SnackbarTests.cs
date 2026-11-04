using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using Microsoft.Extensions.Time.Testing;
using WorkoutLog.UI.Components.Feedback;
using WorkoutLog.UI.Components.Layout;
using WorkoutLog.UI.Logging;

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
    public async Task OnTimeoutThrows_ReachesTheErrorBoundary()
    {
        var logger = new FakeLogger<AppErrorBoundary>();
        Context.Services.AddSingleton<ILogger<AppErrorBoundary>>(logger);
        RenderFragment snackbar = b =>
        {
            b.OpenComponent<Snackbar>(0);
            b.AddComponentParameter(1, nameof(Snackbar.Message), "Set 2 deleted");
            b.AddComponentParameter(2, nameof(Snackbar.OnTimeout), EventCallback.Factory.Create(this, () => throw new InvalidOperationException()));
            b.CloseComponent();
        };
        var cut = Context.Render<AppErrorBoundary>(p => p
            .Add(x => x.OnRestart, () => { })
            .AddChildContent(snackbar));

        clock.Advance(Snackbar.Duration);
        await WaitForRendererAsync();

        Assert.Multiple(() =>
        {
            Assert.That(cut.FindAll("[data-testid=error-message]"), Has.Count.EqualTo(1));
            Assert.That(LoggedEvents.Of(logger), Is.EqualTo(new[] { (UiEvent.ScreenError, LogLevel.Error, "Unhandled error in a screen") }));
        });
    }

    [Test]
    public void TakeFocus_FocusesUndoWhenShown()
    {
        var cut = Context.Render<Snackbar>(p => p
            .Add(x => x.Message, "Set 2 deleted")
            .Add(x => x.TakeFocus, true));

        Context.JSInterop.VerifyFocusAsyncInvoke().Arguments[0].ShouldBeElementReferenceTo(cut.Find("[data-testid=snackbar-undo]"));
    }

    [Test]
    public void NoTakeFocus_LeavesFocusAlone()
    {
        Context.Render<Snackbar>(p => p.Add(x => x.Message, "Set 2 deleted"));

        Context.JSInterop.VerifyNotInvoke("Blazor._internal.domWrapper.focus");
    }

    [Test]
    public async Task UndoFocused_NeverTimesOut()
    {
        var timedOut = 0;
        var cut = Context.Render<Snackbar>(p => p
            .Add(x => x.Message, "Set 2 deleted")
            .Add(x => x.OnTimeout, () => timedOut++));

        await cut.Find("[data-testid=snackbar-undo]").FocusAsync(new FocusEventArgs());
        clock.Advance(Snackbar.Duration * 3);
        await WaitForRendererAsync();

        Assert.That(timedOut, Is.Zero);
    }

    [Test]
    public async Task UndoLosesFocus_TimesOutAFullDurationLater()
    {
        var timedOut = 0;
        var cut = Context.Render<Snackbar>(p => p
            .Add(x => x.Message, "Set 2 deleted")
            .Add(x => x.OnTimeout, () => timedOut++));
        clock.Advance(Snackbar.Duration / 2);
        await cut.Find("[data-testid=snackbar-undo]").FocusAsync(new FocusEventArgs());
        clock.Advance(Snackbar.Duration);

        await cut.Find("[data-testid=snackbar-undo]").BlurAsync(new FocusEventArgs());
        clock.Advance(Snackbar.Duration - TimeSpan.FromMilliseconds(1));
        await WaitForRendererAsync();
        var early = timedOut;
        clock.Advance(TimeSpan.FromMilliseconds(1));
        await WaitForRendererAsync();

        Assert.Multiple(() =>
        {
            Assert.That(early, Is.Zero);
            Assert.That(timedOut, Is.EqualTo(1));
        });
    }

    [Test]
    public async Task FocusedAndLeftAfterTimingOut_DoesNotTimeOutAgain()
    {
        var timedOut = 0;
        var cut = Context.Render<Snackbar>(p => p
            .Add(x => x.Message, "Set 2 deleted")
            .Add(x => x.OnTimeout, () => timedOut++));
        clock.Advance(Snackbar.Duration);
        await WaitForRendererAsync();

        await cut.Find("[data-testid=snackbar-undo]").FocusAsync(new FocusEventArgs());
        await cut.Find("[data-testid=snackbar-undo]").BlurAsync(new FocusEventArgs());
        clock.Advance(Snackbar.Duration);
        await WaitForRendererAsync();

        Assert.That(timedOut, Is.EqualTo(1));
    }

    [Test]
    public void Duration_IsWithinMaterialsFourToTenSeconds()
    {
        Assert.That(Snackbar.Duration, Is.InRange(TimeSpan.FromSeconds(4), TimeSpan.FromSeconds(10)));
    }
}
