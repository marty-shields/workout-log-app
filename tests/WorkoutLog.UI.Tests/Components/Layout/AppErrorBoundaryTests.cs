using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using WorkoutLog.UI.Components.Layout;
using WorkoutLog.UI.Stories.Shared;

namespace WorkoutLog.UI.Tests.Components.Layout;

public class AppErrorBoundaryTests : ComponentTestBase
{
    private FakeLogger<AppErrorBoundary> logger = null!;
    private bool broken;

    [SetUp]
    public void AddLogger()
    {
        broken = false;
        logger = new FakeLogger<AppErrorBoundary>();
        Context.Services.AddSingleton<ILogger<AppErrorBoundary>>(logger);
    }

    [Test]
    public void NoError_ShowsContentAndLogsNothing()
    {
        var cut = Render(() => { });

        Assert.Multiple(() =>
        {
            Assert.That(cut.FindAll("[data-testid=content]"), Has.Count.EqualTo(1));
            Assert.That(cut.FindAll("[data-testid=error-message]"), Is.Empty);
            Assert.That(LoggedEvents.Of(logger), Is.Empty);
        });
    }

    [Test]
    public void ContentThrows_ShowsTheErrorScreenAndLogsTheException()
    {
        broken = true;

        var cut = Render(() => { });

        Assert.Multiple(() =>
        {
            Assert.That(cut.FindAll("[data-testid=error-message]"), Has.Count.EqualTo(1));
            Assert.That(cut.FindAll("[data-testid=content]"), Is.Empty);
            Assert.That(LoggedEvents.Of(logger), Is.EqualTo(new[] { (2101, LogLevel.Error, "Unhandled error in a screen") }));
            Assert.That(logger.LatestRecord.Exception, Is.TypeOf<InvalidOperationException>());
        });
    }

    [Test]
    public void Restart_RaisesOnRestartThenShowsTheContentAgain()
    {
        broken = true;
        var restarted = 0;
        var cut = Render(() =>
        {
            restarted++;
            broken = false;
        });

        cut.Find("[data-testid=error-restart]").Click();

        Assert.Multiple(() =>
        {
            Assert.That(restarted, Is.EqualTo(1));
            Assert.That(cut.FindAll("[data-testid=content]"), Has.Count.EqualTo(1));
            Assert.That(cut.FindAll("[data-testid=error-message]"), Is.Empty);
        });
    }

    private IRenderedComponent<AppErrorBoundary> Render(Action onRestart) =>
        Context.Render<AppErrorBoundary>(p => p
            .Add(x => x.OnRestart, onRestart)
            .AddChildContent(Content));

    private void Content(RenderTreeBuilder builder)
    {
        if (broken)
        {
            builder.OpenComponent<ThrowOnRender>(0);
            builder.CloseComponent();
        }
        else
        {
            builder.AddMarkupContent(1, "<p data-testid=\"content\">App</p>");
        }
    }
}
