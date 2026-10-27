using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using Microsoft.JSInterop;
using WorkoutLog.UI.Components.Feedback;
using WorkoutLog.UI.Logging;

namespace WorkoutLog.UI.Tests.Components.Feedback;

public class LoadFailedTests : ComponentTestBase
{
    [Test]
    public void Render_ShowsTheMessageAsAnAlertAndTryAgain()
    {
        var cut = Render();

        var error = cut.Find("[data-testid=history-error]");
        Assert.Multiple(() =>
        {
            Assert.That(error.TextContent, Is.EqualTo("Couldn't load your history"));
            Assert.That(error.GetAttribute("role"), Is.EqualTo("alert"));
            Assert.That(cut.Find("[data-testid=history-retry]").TextContent.Trim(), Is.EqualTo("Try again"));
        });
    }

    [Test]
    public void TryAgain_RaisesOnRetry()
    {
        var retries = 0;
        var cut = Render(() => retries++);

        cut.Find("[data-testid=history-retry]").Click();

        Assert.That(retries, Is.EqualTo(1));
    }

    [Test]
    public void FirstRender_FocusesTryAgain()
    {
        var cut = Render();

        cut.WaitForAssertion(() => Focuses().Single().Arguments[0]
            .ShouldBeElementReferenceTo(cut.Find("[data-testid=history-retry]")));
    }

    [Test]
    public void RenderedAgain_DoesNotFocusAgain()
    {
        var cut = Render();
        cut.WaitForAssertion(() => Assert.That(Focuses(), Has.Count.EqualTo(1)));

        cut.Render(p => p.Add(x => x.Message, "Couldn't load your history"));

        Assert.That(Focuses(), Has.Count.EqualTo(1));
    }

    [Test]
    public void FocusFails_LogsAWarningAndKeepsTheError()
    {
        var logger = new FakeLogger<LoadFailed>();
        Context.Services.AddSingleton<ILogger<LoadFailed>>(logger);
        Context.JSInterop.SetupVoid("Blazor._internal.domWrapper.focus", _ => true).SetException(new JSException("Focus failed"));

        var cut = Render();

        cut.WaitForAssertion(() => Assert.That(LoggedEvents.Of(logger), Is.EqualTo(new[] { (UiEvent.LoadFailedFocusFailed, LogLevel.Warning, "Focusing Try again failed") })));
        Assert.Multiple(() =>
        {
            Assert.That(logger.LatestRecord.Exception, Is.TypeOf<JSException>());
            Assert.That(cut.FindAll("[data-testid=history-retry]"), Has.Count.EqualTo(1));
        });
    }

    private IRenderedComponent<LoadFailed> Render(Action? onRetry = null) =>
        Context.Render<LoadFailed>(p => p
            .Add(x => x.Message, "Couldn't load your history")
            .Add(x => x.TestId, "history")
            .Add(x => x.OnRetry, onRetry ?? (() => { })));

    private IReadOnlyList<JSRuntimeInvocation> Focuses() =>
        Context.JSInterop.Invocations["Blazor._internal.domWrapper.focus"];
}
