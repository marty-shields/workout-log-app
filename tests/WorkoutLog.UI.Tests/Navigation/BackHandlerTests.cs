using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using WorkoutLog.UI.Components.Layout;
using WorkoutLog.UI.Navigation;

namespace WorkoutLog.UI.Tests.Navigation;

public class BackHandlerTests : ComponentTestBase
{
    [Test]
    public void NoOnBack_DoesNotRegister()
    {
        Context.Render<BackHandler>();

        Assert.That(BackButton.HasHandler, Is.False);
    }

    [Test]
    public async Task OnBackSetLater_Registers()
    {
        var back = 0;
        var cut = Context.Render<BackHandler>();

        cut.Render(p => p.Add(x => x.OnBack, () => back++));
        await BackButton.HandleAsync();

        Assert.That(back, Is.EqualTo(1));
    }

    [Test]
    public void OnBackCleared_Unregisters()
    {
        var cut = Context.Render<BackHandler>(p => p.Add(x => x.OnBack, () => { }));

        cut.Render(p => p.Add(x => x.OnBack, default(EventCallback)));

        Assert.That(BackButton.HasHandler, Is.False);
    }

    [Test]
    public void Removed_Unregisters()
    {
        RenderFragment handler = b =>
        {
            b.OpenComponent<BackHandler>(0);
            b.AddComponentParameter(1, nameof(BackHandler.OnBack), EventCallback.Factory.Create(this, () => { }));
            b.CloseComponent();
        };
        var cut = Context.Render<AppScreen>(p => p
            .Add(x => x.Title, "Workout Log")
            .Add(x => x.ChildContent, handler));

        cut.Render(p => p.Add(x => x.ChildContent, (RenderFragment?)null));

        Assert.That(BackButton.HasHandler, Is.False);
    }

    [Test]
    public async Task OnBackThrows_ShowsTheErrorScreenAsTheBackArrowDoes()
    {
        var logger = new FakeLogger<AppErrorBoundary>();
        Context.Services.AddSingleton<ILogger<AppErrorBoundary>>(logger);
        RenderFragment screen = b =>
        {
            b.OpenComponent<AppScreen>(0);
            b.AddComponentParameter(1, nameof(AppScreen.Title), "History");
            b.AddComponentParameter(2, nameof(AppScreen.OnBack), EventCallback.Factory.Create(this, () => throw new InvalidOperationException()));
            b.CloseComponent();
        };
        var cut = Context.Render<AppErrorBoundary>(p => p
            .Add(x => x.OnRestart, () => { })
            .AddChildContent(screen));

        await BackButton.HandleAsync();

        Assert.Multiple(() =>
        {
            Assert.That(cut.FindAll("[data-testid=error-message]"), Has.Count.EqualTo(1));
            Assert.That(LoggedEvents.Of(logger), Is.EqualTo(new[] { (2101, LogLevel.Error, "Unhandled error in a screen") }));
        });
    }
}
