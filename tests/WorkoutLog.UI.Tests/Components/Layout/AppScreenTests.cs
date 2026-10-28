using Microsoft.AspNetCore.Components;
using WorkoutLog.UI.Components.Actions;
using WorkoutLog.UI.Components.Layout;

namespace WorkoutLog.UI.Tests.Components.Layout;

public class AppScreenTests : ComponentTestBase
{
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
}
