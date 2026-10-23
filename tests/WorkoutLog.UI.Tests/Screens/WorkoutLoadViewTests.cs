using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using Microsoft.JSInterop;
using WorkoutLog.UI.Logging;
using WorkoutLog.UI.Screens;

namespace WorkoutLog.UI.Tests.Screens;

public class WorkoutLoadViewTests : ComponentTestBase
{
    [Test]
    public void Loading_ShowsTheSpinnerUnderWorkout()
    {
        var cut = Context.Render<WorkoutLoadView>();

        Assert.Multiple(() =>
        {
            Assert.That(cut.Find("[data-testid=screen-title]").TextContent, Is.EqualTo("Workout"));
            Assert.That(cut.FindAll("[data-testid=workout-loading]"), Has.Count.EqualTo(1));
            Assert.That(cut.FindAll("[data-testid=workout-load-error]"), Is.Empty);
            Assert.That(cut.FindAll("[data-testid=workout-not-found]"), Is.Empty);
            Assert.That(cut.FindAll("[data-testid=delete-workout]"), Is.Empty);
            Assert.That(cut.FindAll("[data-testid=bottom-bar]"), Is.Empty);
        });
    }

    [Test]
    public void Failed_ShowsTheErrorAndTryAgain()
    {
        var cut = Context.Render<WorkoutLoadView>(p => p.Add(x => x.Failed, true));

        Assert.Multiple(() =>
        {
            Assert.That(cut.Find("[data-testid=workout-load-error]").TextContent, Is.EqualTo("Couldn't load the workout."));
            Assert.That(cut.Find("[data-testid=workout-load-error]").GetAttribute("role"), Is.EqualTo("alert"));
            Assert.That(cut.Find("[data-testid=workout-retry]").TextContent.Trim(), Is.EqualTo("Try again"));
            Assert.That(cut.FindAll("[data-testid=workout-loading]"), Is.Empty);
        });
    }

    [Test]
    public void NotFound_ShowsTheMessageWithoutTryAgain()
    {
        var cut = Context.Render<WorkoutLoadView>(p => p.Add(x => x.NotFound, true).Add(x => x.Failed, true));

        Assert.Multiple(() =>
        {
            Assert.That(cut.Find("[data-testid=workout-not-found]").TextContent.Trim(), Is.EqualTo("This workout no longer exists."));
            Assert.That(cut.FindAll("[data-testid=workout-retry]"), Is.Empty);
            Assert.That(cut.FindAll("[data-testid=workout-loading]"), Is.Empty);
        });
    }

    [Test]
    public void TryAgain_RaisesOnRetry()
    {
        var retries = 0;
        var cut = Context.Render<WorkoutLoadView>(p => p.Add(x => x.Failed, true).Add(x => x.OnRetry, () => retries++));

        cut.Find("[data-testid=workout-retry]").Click();

        Assert.That(retries, Is.EqualTo(1));
    }

    [Test]
    public void Failed_FocusesTryAgain()
    {
        var cut = Context.Render<WorkoutLoadView>(p => p.Add(x => x.Failed, true));

        cut.WaitForAssertion(() => Focuses().Single().Arguments[0]
            .ShouldBeElementReferenceTo(cut.Find("[data-testid=workout-retry]")));
    }

    [Test]
    public void Failed_RenderedAgain_DoesNotFocusAgain()
    {
        var cut = Context.Render<WorkoutLoadView>(p => p.Add(x => x.Failed, true));
        cut.WaitForAssertion(() => Assert.That(Focuses(), Has.Count.EqualTo(1)));

        cut.Render(p => p.Add(x => x.Failed, true));

        Assert.That(Focuses(), Has.Count.EqualTo(1));
    }

    [Test]
    public void FailedAfterLoading_FocusesTryAgainAgain()
    {
        var cut = Context.Render<WorkoutLoadView>(p => p.Add(x => x.Failed, true));
        cut.WaitForAssertion(() => Assert.That(Focuses(), Has.Count.EqualTo(1)));

        cut.Render(p => p.Add(x => x.Failed, false));
        cut.Render(p => p.Add(x => x.Failed, true));

        cut.WaitForAssertion(() => Assert.That(Focuses(), Has.Count.EqualTo(2)));
    }

    [Test]
    public void FocusFails_LogsAWarningAndKeepsTheError()
    {
        var logger = new FakeLogger<WorkoutLoadView>();
        Context.Services.AddSingleton<ILogger<WorkoutLoadView>>(logger);
        Context.JSInterop.SetupVoid("Blazor._internal.domWrapper.focus", _ => true).SetException(new JSException("Focus failed"));

        var cut = Context.Render<WorkoutLoadView>(p => p.Add(x => x.Failed, true));

        cut.WaitForAssertion(() => Assert.That(LoggedEvents.Of(logger),
            Is.EqualTo(new[] { (UiEvent.WorkoutRetryFocusFailed, LogLevel.Warning, "Focusing Try again failed") })));
        Assert.Multiple(() =>
        {
            Assert.That(logger.LatestRecord.Exception, Is.TypeOf<JSException>());
            Assert.That(cut.FindAll("[data-testid=workout-retry]"), Has.Count.EqualTo(1));
        });
    }

    [Test]
    public void Back_RaisesOnBack()
    {
        var backs = 0;
        var cut = Context.Render<WorkoutLoadView>(p => p.Add(x => x.OnBack, () => backs++));

        cut.Find("[data-testid=back]").Click();

        Assert.That(backs, Is.EqualTo(1));
    }

    [Test]
    public async Task AndroidBack_RaisesOnBack()
    {
        var backs = 0;
        var cut = Context.Render<WorkoutLoadView>(p => p.Add(x => x.NotFound, true).Add(x => x.OnBack, () => backs++));

        await cut.InvokeAsync(BackButton.HandleAsync);

        Assert.That(backs, Is.EqualTo(1));
    }

    private IReadOnlyList<JSRuntimeInvocation> Focuses() =>
        Context.JSInterop.Invocations["Blazor._internal.domWrapper.focus"];
}
