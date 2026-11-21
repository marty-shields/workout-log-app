using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Shields.UI.Components.Layout;

namespace WorkoutLog.UI.Tests.Lifecycle;

/// <summary>
/// The App's <c>Routes.razor</c> can't be rendered from here, so this renders the kit's <c>StartupGate</c> with the same Title and FailedMessage it passes
/// (<c>ActivityWiringTests</c> pins that Routes uses a <c>StartupGate</c> with an <c>Initialize</c>).
/// </summary>
public class StartupFailureTests : ComponentTestBase
{
    private const string AppTitle = "Workout Log";
    private const string AppFailedMessage = "Couldn't open the workout database";

    [SetUp]
    public void AddLogger() => Context.Services.AddSingleton<ILogger<StartupGate>>(NullLogger<StartupGate>.Instance);

    [Test]
    public void InitializeThrows_ShowsTheTitledFailedStateWithTryAgain()
    {
        var cut = Render(() => throw new InvalidOperationException("database"));

        Assert.Multiple(() =>
        {
            Assert.That(cut.Find("[data-testid=startup-top-bar] .title").TextContent, Is.EqualTo(AppTitle));
            Assert.That(cut.Find("[data-testid=startup-failed]").TextContent, Does.Contain(AppFailedMessage));
            Assert.That(cut.Find("[data-testid=startup-retry]").TextContent, Is.EqualTo("Try again"));
            Assert.That(cut.FindAll("[data-testid=content]"), Is.Empty);
        });
    }

    [Test]
    public void TryAgain_RunsInitializeAgainAndShowsTheContentOnceItWorks()
    {
        var attempts = 0;
        var cut = Render(() => ++attempts == 1 ? throw new InvalidOperationException("database") : Task.CompletedTask);

        cut.Find("[data-testid=startup-retry]").Click();

        cut.WaitForAssertion(() =>
        {
            Assert.That(attempts, Is.EqualTo(2));
            Assert.That(cut.FindAll("[data-testid=content]"), Has.Count.EqualTo(1));
            Assert.That(cut.FindAll("[data-testid=startup-failed]"), Is.Empty);
        });
    }

    private IRenderedComponent<StartupGate> Render(Func<Task> initialize) =>
        Context.Render<StartupGate>(p => p
            .Add(x => x.Initialize, initialize)
            .Add(x => x.Title, AppTitle)
            .Add(x => x.FailedMessage, AppFailedMessage)
            .AddChildContent("<p data-testid=\"content\">ready</p>"));
}
