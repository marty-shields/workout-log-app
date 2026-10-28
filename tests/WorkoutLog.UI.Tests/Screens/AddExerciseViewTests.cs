using Microsoft.AspNetCore.Components;
using WorkoutLog.UI.Components.Actions;
using WorkoutLog.UI.Components.Layout;
using WorkoutLog.UI.Models.Exercises;
using WorkoutLog.UI.Screens;
using WorkoutLog.UI.Stories.Samples;

namespace WorkoutLog.UI.Tests.Screens;

public class AddExerciseViewTests : ComponentTestBase
{
    [Test]
    public void List_ShowsTheTitleSearchChipsAndTappableRowsWithTheirKind()
    {
        var cut = Context.Render<AddExerciseView>(p => p.Add(x => x.Items, SampleExercises.All).Add(x => x.OnPick, (ExerciseListItem _) => { }));

        Assert.Multiple(() =>
        {
            Assert.That(cut.Find("[data-testid=exercises-overlay]").GetAttribute("role"), Is.EqualTo("dialog"));
            Assert.That(cut.Find(".title").TextContent, Is.EqualTo("Exercises"));
            Assert.That(cut.FindAll("[data-testid=exercises-filter] .text").Select(t => t.TextContent), Is.EqualTo(new[] { "All", "Strength", "Cardio" }));
            Assert.That(cut.FindAll("button[data-testid^=exercises-row-]"), Has.Count.EqualTo(SampleExercises.All.Count));
            Assert.That(cut.Find("[data-testid=exercises-row-311] .aside").TextContent, Is.EqualTo("Cardio"));
        });
    }

    [Test]
    public void Open_FocusesSearch()
    {
        var cut = Context.Render<AddExerciseView>(p => p.Add(x => x.Items, SampleExercises.All));

        Context.JSInterop.VerifyFocusAsyncInvoke().Arguments[0].ShouldBeElementReferenceTo(cut.Find("[data-testid=exercises-search]"));
    }

    [Test]
    public void Loading_ShowsTheSpinnerThenFocusesSearchOnceTheListShows()
    {
        var cut = Context.Render<AddExerciseView>(p => p.Add(x => x.Items, []).Add(x => x.Loading, true));

        Assert.Multiple(() =>
        {
            Assert.That(cut.Find("[data-testid=exercises-loading]").TextContent, Does.Contain("Loading…"));
            Assert.That(cut.FindAll("[data-testid=exercises-search]"), Is.Empty);
            Assert.That(cut.FindAll("[data-testid=exercises-back]"), Has.Count.EqualTo(1));
        });

        cut.Render(p => p.Add(x => x.Items, SampleExercises.All).Add(x => x.Loading, false));

        Context.JSInterop.VerifyFocusAsyncInvoke().Arguments[0].ShouldBeElementReferenceTo(cut.Find("[data-testid=exercises-search]"));
    }

    [Test]
    public void TappingARow_RaisesOnPick()
    {
        ExerciseListItem? picked = null;
        var cut = Context.Render<AddExerciseView>(p => p.Add(x => x.Items, SampleExercises.All).Add(x => x.OnPick, (ExerciseListItem item) => picked = item));

        cut.Find("[data-testid=exercises-row-132]").Click();

        Assert.That(picked, Is.EqualTo(SampleExercises.All.Single(e => e.Id == 132)));
    }

    [Test]
    public void BackArrow_RaisesOnClose()
    {
        var closed = false;
        var cut = Context.Render<AddExerciseView>(p => p.Add(x => x.Items, SampleExercises.All).Add(x => x.OnClose, () => closed = true));

        cut.Find("[data-testid=exercises-back]").Click();

        Assert.That(closed, Is.True);
    }

    [Test]
    public void Back_IsTheBackIcon()
    {
        var cut = Context.Render<AddExerciseView>(p => p.Add(x => x.Items, SampleExercises.All));

        var back = cut.FindComponent<IconButton>().Instance;

        Assert.That((back.TestId, back.Icon, back.Muted, back.Size), Is.EqualTo(("exercises-back", IconName.Back, false, IconSize.Default)));
    }

    [Test]
    public async Task AndroidBack_ClosesTheListThenTheScreenBelowTakesBack()
    {
        var screenBack = 0;
        var closed = 0;
        RenderFragment list = b =>
        {
            b.OpenComponent<AddExerciseView>(0);
            b.AddComponentParameter(1, nameof(AddExerciseView.Items), SampleExercises.All);
            b.AddComponentParameter(2, nameof(AddExerciseView.OnClose), EventCallback.Factory.Create(this, () => closed++));
            b.CloseComponent();
        };
        var cut = Context.Render<AppScreen>(p => p
            .Add(x => x.Title, "New Workout")
            .Add(x => x.OnBack, () => screenBack++)
            .Add(x => x.ChildContent, list));

        await BackButton.HandleAsync();
        cut.Render(p => p.Add(x => x.ChildContent, (RenderFragment?)null));
        await BackButton.HandleAsync();

        Assert.Multiple(() =>
        {
            Assert.That(closed, Is.EqualTo(1));
            Assert.That(screenBack, Is.EqualTo(1));
        });
    }

    [Test]
    public void NoOnClose_DoesNotTakeAndroidBack()
    {
        Context.Render<AddExerciseView>(p => p.Add(x => x.Items, SampleExercises.All));

        Assert.That(BackButton.HasHandler, Is.False);
    }
}
