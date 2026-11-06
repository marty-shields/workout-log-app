using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using Microsoft.JSInterop;
using WorkoutLog.UI.Components.Actions;
using WorkoutLog.UI.Components.Layout;
using WorkoutLog.UI.Logging;
using WorkoutLog.UI.Navigation;

namespace WorkoutLog.UI.Tests.Components.Layout;

public class AppScreenTests : ComponentTestBase
{
    private const string AppScreenModule = "./_content/WorkoutLog.UI/Components/Layout/AppScreen.razor.js";

    [Test]
    public void Render_ShowsTitleAndSubtitle()
    {
        var cut = Context.Render<AppScreen>(p => p
            .Add(x => x.Title, "Workouts")
            .Add(x => x.Subtitle, "Elapsed 0:01:00"));

        Assert.Multiple(() =>
        {
            Assert.That(cut.Find("[data-testid=screen-title]").TextContent, Is.EqualTo("Workouts"));
            Assert.That(cut.Find("[data-testid=screen-subtitle]").TextContent, Is.EqualTo("Elapsed 0:01:00"));
        });
    }

    [Test]
    public void NoOnBack_HidesBackArrow()
    {
        var cut = Context.Render<AppScreen>(p => p.Add(x => x.Title, "Workout Log"));

        Assert.That(cut.FindAll("[data-testid=back]"), Is.Empty);
    }

    [Test]
    public void BackArrow_RaisesOnBack()
    {
        var back = false;
        var cut = Context.Render<AppScreen>(p => p
            .Add(x => x.Title, "Workouts")
            .Add(x => x.OnBack, () => back = true));

        cut.Find("[data-testid=back]").Click();

        Assert.That(back, Is.True);
    }

    [Test]
    public void BackArrow_IsTheBackIcon()
    {
        var cut = Context.Render<AppScreen>(p => p
            .Add(x => x.Title, "Workouts")
            .Add(x => x.OnBack, () => { }));

        var back = cut.FindComponent<IconButton>().Instance;

        Assert.That((back.TestId, back.Icon, back.Muted, back.Size), Is.EqualTo(("back", IconName.Back, false, IconSize.Default)));
    }

    [Test]
    public async Task AndroidBack_RaisesOnBack()
    {
        var back = 0;
        Context.Render<AppScreen>(p => p
            .Add(x => x.Title, "Workouts")
            .Add(x => x.OnBack, () => back++));

        await BackButton.HandleAsync();

        Assert.That(back, Is.EqualTo(1));
    }

    [Test]
    public void NoOnBack_DoesNotTakeAndroidBack()
    {
        Context.Render<AppScreen>(p => p.Add(x => x.Title, "Workout Log"));

        Assert.That(BackButton.HasHandler, Is.False);
    }

    [Test]
    public void BottomBar_RendersOnlyWhenSet()
    {
        var without = Context.Render<AppScreen>(p => p.Add(x => x.Title, "Workouts"));
        var with = Context.Render<AppScreen>(p => p
            .Add(x => x.Title, "Workout Log")
            .Add(x => x.BottomBar, (RenderFragment)(b => b.AddContent(0, "New Workout"))));

        Assert.Multiple(() =>
        {
            Assert.That(without.FindAll("[data-testid=bottom-bar]"), Is.Empty);
            Assert.That(with.Find("[data-testid=bottom-bar]").TextContent, Does.Contain("New Workout"));
        });
    }

    [Test]
    public void NoDestination_HidesTheNavigationBar()
    {
        var cut = Context.Render<AppScreen>(p => p.Add(x => x.Title, "Settings"));

        Assert.That(cut.FindAll("[data-testid=bottom-nav]"), Is.Empty);
    }

    [Test]
    public void Destination_ShowsTheNavigationBarUnderTheBottomBar()
    {
        var cut = Context.Render<AppScreen>(p => p
            .Add(x => x.Title, "Workout Log")
            .Add(x => x.Destination, NavDestination.Dashboard)
            .Add(x => x.BottomBar, (RenderFragment)(b => b.AddContent(0, "New Workout"))));

        Assert.Multiple(() =>
        {
            Assert.That(cut.FindAll("footer > *").Select(e => e.GetAttribute("data-testid")), Is.EqualTo(new[] { "bottom-bar", "bottom-nav" }));
            Assert.That(cut.Find("[aria-current=page]").GetAttribute("data-testid"), Is.EqualTo("dashboard-link"));
        });
    }

    [Test]
    public void NavigationBar_RaisesOnNavigate()
    {
        var navigated = new List<NavDestination>();
        var cut = Context.Render<AppScreen>(p => p
            .Add(x => x.Title, "Workout Log")
            .Add(x => x.Destination, NavDestination.Dashboard)
            .Add(x => x.OnNavigate, navigated.Add));

        cut.Find("[data-testid=workouts-link]").Click();

        Assert.That(navigated, Is.EqualTo(new[] { NavDestination.Workouts }));
    }

    [TestCase(NavDestination.Workouts)]
    [TestCase(NavDestination.Exercises)]
    public async Task AndroidBack_OnATopLevelScreen_NavigatesToTheDashboard(NavDestination destination)
    {
        var navigated = new List<NavDestination>();
        var cut = Context.Render<AppScreen>(p => p
            .Add(x => x.Title, "Workouts")
            .Add(x => x.Destination, destination)
            .Add(x => x.OnNavigate, navigated.Add));

        await cut.InvokeAsync(BackButton.HandleAsync);

        Assert.Multiple(() =>
        {
            Assert.That(navigated, Is.EqualTo(new[] { NavDestination.Dashboard }));
            Assert.That(cut.FindAll("[data-testid=back]"), Is.Empty);
        });
    }

    [Test]
    public void TheDashboard_DoesNotTakeAndroidBack()
    {
        Context.Render<AppScreen>(p => p
            .Add(x => x.Title, "Workout Log")
            .Add(x => x.Destination, NavDestination.Dashboard)
            .Add(x => x.OnNavigate, (NavDestination _) => { }));

        Assert.That(BackButton.HasHandler, Is.False);
    }

    [Test]
    public void Status_SitsInAPoliteLiveRegionAboveTheBottomBar()
    {
        var cut = Context.Render<AppScreen>(p => p
            .Add(x => x.Title, "Workouts")
            .Add(x => x.BottomBar, "<button>Save</button>")
            .Add(x => x.Status, "<p data-testid=\"shown\">Set 2 deleted</p>"));

        var slot = cut.Find("footer > [role=status]");
        Assert.Multiple(() =>
        {
            Assert.That(slot.GetAttribute("aria-live"), Is.EqualTo("polite"));
            Assert.That(slot.QuerySelector("[data-testid=shown]"), Is.Not.Null);
            Assert.That(slot.NextElementSibling!.GetAttribute("data-testid"), Is.EqualTo("bottom-bar"));
        });
    }

    [Test]
    public void NoStatus_HasNoLiveRegion()
    {
        var cut = Context.Render<AppScreen>(p => p
            .Add(x => x.Title, "Workouts")
            .Add(x => x.BottomBar, "<button>Save</button>"));

        Assert.That(cut.FindAll("[role=status]"), Is.Empty);
    }

    [Test]
    public void NoOnRefresh_HasNoPullGesture()
    {
        var cut = Context.Render<AppScreen>(p => p.Add(x => x.Title, "Settings"));

        Assert.Multiple(() =>
        {
            Assert.That(Context.JSInterop.Invocations, Is.Empty);
            Assert.That(cut.FindAll("[data-testid=pull-refresh]"), Is.Empty);
        });
    }

    [Test]
    public void OnRefresh_AttachesThePullGestureToTheContent()
    {
        var module = PullModule();

        var cut = Context.Render<AppScreen>(p => p
            .Add(x => x.Title, "Exercises")
            .Add(x => x.OnRefresh, () => Task.CompletedTask));

        var attach = module.Invocations["attach"].Single();
        Assert.Multiple(() =>
        {
            attach.Arguments[0].ShouldBeElementReferenceTo(cut.Find("main"));
            attach.Arguments[1].ShouldBeElementReferenceTo(cut.Find("[data-testid=pull-refresh]"));
            Assert.That(cut.Find("[data-testid=pull-refresh-status]").TextContent, Is.Empty);
        });
    }

    [Test]
    public async Task Refresh_SpinsAndSaysRefreshingUntilOnRefreshFinishes()
    {
        PullModule();
        var done = new TaskCompletionSource();
        var cut = Context.Render<AppScreen>(p => p
            .Add(x => x.Title, "Exercises")
            .Add(x => x.OnRefresh, () => done.Task));

        var refresh = cut.InvokeAsync(cut.Instance.RefreshAsync);

        Assert.Multiple(() =>
        {
            Assert.That(cut.Find("[data-testid=pull-refresh]").ClassList, Does.Contain("refreshing"));
            Assert.That(cut.Find("[data-testid=pull-refresh-status]").TextContent, Is.EqualTo("Refreshing…"));
        });
        done.SetResult();
        await refresh;
        Assert.Multiple(() =>
        {
            Assert.That(cut.Find("[data-testid=pull-refresh]").ClassList, Does.Not.Contain("refreshing"));
            Assert.That(cut.Find("[data-testid=pull-refresh-status]").TextContent, Is.Empty);
        });
    }

    [Test]
    public async Task Refresh_DuringARefresh_IsIgnored()
    {
        PullModule();
        var done = new TaskCompletionSource();
        var calls = 0;
        var cut = Context.Render<AppScreen>(p => p
            .Add(x => x.Title, "Exercises")
            .Add(x => x.OnRefresh, () =>
            {
                calls++;
                return done.Task;
            }));

        var first = cut.InvokeAsync(cut.Instance.RefreshAsync);
        await cut.InvokeAsync(cut.Instance.RefreshAsync);
        done.SetResult();
        await first;
        await cut.InvokeAsync(cut.Instance.RefreshAsync);

        Assert.That(calls, Is.EqualTo(2));
    }

    [Test]
    public void PullGestureFailsToAttach_LogsItAndShowsTheScreen()
    {
        var logger = new FakeLogger<AppScreen>();
        Context.Services.AddSingleton<ILogger<AppScreen>>(logger);
        Context.JSInterop.SetupModule(AppScreenModule).SetupVoid("attach", _ => true).SetException(new JSException("Attach failed"));

        var cut = Context.Render<AppScreen>(p => p
            .Add(x => x.Title, "Exercises")
            .Add(x => x.OnRefresh, () => Task.CompletedTask)
            .AddChildContent("<p data-testid=\"shown\">Rows</p>"));

        Assert.Multiple(() =>
        {
            Assert.That(cut.FindAll("[data-testid=shown]"), Has.Count.EqualTo(1));
            Assert.That(LoggedEvents.Of(logger), Is.EqualTo(new[] { (UiEvent.PullRefreshFailed, LogLevel.Warning, "Setting up pull to refresh failed") }));
        });
    }

    [Test]
    public async Task Disposed_DetachesThePullGesture()
    {
        var module = PullModule();
        var cut = Context.Render<AppScreen>(p => p
            .Add(x => x.Title, "Exercises")
            .Add(x => x.OnRefresh, () => Task.CompletedTask));
        var main = cut.Find("main");

        await Context.DisposeComponentsAsync();

        module.Invocations["detach"].Single().Arguments[0].ShouldBeElementReferenceTo(main);
    }

    private BunitJSModuleInterop PullModule()
    {
        var module = Context.JSInterop.SetupModule(AppScreenModule);
        module.Mode = JSRuntimeMode.Loose;
        return module;
    }
}
