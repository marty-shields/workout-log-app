using Microsoft.AspNetCore.Components;
using WorkoutLog.Core.Domain.Exercises;
using WorkoutLog.UI.Components.Actions;
using WorkoutLog.UI.Components.Layout;
using WorkoutLog.UI.Models.Exercises;
using WorkoutLog.UI.Screens;
using WorkoutLog.UI.Stories.Samples;

namespace WorkoutLog.UI.Tests.Screens;

public class AddExerciseViewTests : ComponentTestBase
{
    [Test]
    public void List_ShowsSearchChipsAndARowPerExerciseWithItsLastSession()
    {
        var cut = RenderPicker(SampleExercises.All.Take(2).ToList());

        Assert.Multiple(() =>
        {
            Assert.That(cut.Find("[data-testid=screen-title]").TextContent, Is.EqualTo("Exercises"));
            Assert.That(cut.Find(".toolbar [data-testid=add-exercise-search]").GetAttribute("placeholder"), Is.EqualTo("Search exercises"));
            Assert.That(cut.FindAll(".toolbar [data-testid=add-exercise-filter] .text").Select(t => t.TextContent), Is.EqualTo(new[] { "All", "Strength", "Cardio" }));
            Assert.That(cut.FindAll(".content [data-testid^=add-exercise-row-] .text").Select(t => t.TextContent), Is.EqualTo(new[] { "Barbell Back Squat", "Machine Assisted Pull-up" }));
            Assert.That(cut.FindAll("[data-testid^=add-exercise-row-] .detail").Select(d => d.TextContent),
                Is.EqualTo(new[] { "Last Tue 20 Oct · 70kg × 3", "Last Tue 20 Oct · 0kg × 5" }));
        });
    }

    [TestCase(AddExerciseFilter.All, "add-exercise-filter-all")]
    [TestCase(AddExerciseFilter.Strength, "add-exercise-filter-strength")]
    [TestCase(AddExerciseFilter.Cardio, "add-exercise-filter-cardio")]
    public void Filter_ChecksItsChip(AddExerciseFilter filter, string testId)
    {
        var cut = Context.Render<AddExerciseView>(p => p.Add(x => x.Items, SampleExercises.All).Add(x => x.Filter, filter));

        Assert.That(cut.FindAll("[data-testid=add-exercise-filter] input[checked]").Select(i => i.GetAttribute("data-testid")), Is.EqualTo(new[] { testId }));
    }

    [Test]
    public void TappingAChip_RaisesFilterChanged()
    {
        var changed = new List<AddExerciseFilter>();
        var cut = Context.Render<AddExerciseView>(p => p.Add(x => x.Items, SampleExercises.All).Add(x => x.FilterChanged, (AddExerciseFilter f) => changed.Add(f)));

        cut.Find("[data-testid=add-exercise-filter-cardio]").Change(true);

        Assert.That(changed, Is.EqualTo(new[] { AddExerciseFilter.Cardio }));
    }

    [Test]
    public void Typing_RaisesQueryChangedCutToTheLimit()
    {
        var queries = new List<string>();
        var cut = Context.Render<AddExerciseView>(p => p.Add(x => x.Items, SampleExercises.All).Add(x => x.QueryChanged, (string q) => queries.Add(q)));

        cut.Find("[data-testid=add-exercise-search]").Input("squat");
        cut.Find("[data-testid=add-exercise-search]").Input(new string('a', ExerciseCatalog.MaxQueryLength + 1));

        Assert.That(queries.Select(q => q.Length), Is.EqualTo(new[] { 5, ExerciseCatalog.MaxQueryLength }));
    }

    [Test]
    public void Query_FillsTheSearchBox()
    {
        var cut = Context.Render<AddExerciseView>(p => p.Add(x => x.Items, []).Add(x => x.Query, "deadlift"));

        Assert.That(cut.Find("[data-testid=add-exercise-search]").GetAttribute("value"), Is.EqualTo("deadlift"));
    }

    [Test]
    public void TappingALiftRow_RaisesOnPickWithIt()
    {
        ExerciseListItem? opened = null;
        var cut = Context.Render<AddExerciseView>(p => p.Add(x => x.Items, SampleExercises.All).Add(x => x.OnPick, (ExerciseListItem item) => opened = item));

        cut.Find("[data-testid=add-exercise-row-6]").Click();

        Assert.That(opened, Is.EqualTo(SampleExercises.All.Single(e => e.Id == 6)));
    }

    [Test]
    public void WithOnPick_EveryRowIsTappable()
    {
        var cut = Context.Render<AddExerciseView>(p => p.Add(x => x.Items, SampleExercises.All).Add(x => x.OnPick, (ExerciseListItem _) => { }));

        Assert.That(cut.FindAll("button[data-testid^=add-exercise-row-]").Select(r => r.GetAttribute("data-testid")),
            Is.EqualTo(SampleExercises.All.Select(e => $"add-exercise-row-{e.Id}")));
    }

    [Test]
    public void NoOnPick_RowsAreNotTappableAndShowTheirKind()
    {
        var cut = Context.Render<AddExerciseView>(p => p.Add(x => x.Items, SampleExercises.All));

        Assert.Multiple(() =>
        {
            Assert.That(cut.FindAll("button[data-testid^=add-exercise-row-]"), Is.Empty);
            Assert.That(cut.FindAll("[data-testid^=add-exercise-row-] .aside").Select(a => a.TextContent), Is.EqualTo(SampleExercises.All.Select(e => e.Kind.ToString())));
        });
    }

    [Test]
    public void NoItems_ShowsNoExercisesMatchUnderTheSearchAndChips()
    {
        var cut = Context.Render<AddExerciseView>(p => p.Add(x => x.Items, []).Add(x => x.Query, "deadlift"));

        Assert.Multiple(() =>
        {
            Assert.That(cut.Find("[data-testid=add-exercise-none]").TextContent, Is.EqualTo("No exercises match"));
            Assert.That(cut.FindAll("[data-testid=add-exercise-search]"), Has.Count.EqualTo(1));
            Assert.That(cut.FindAll("[data-testid=add-exercise-filter]"), Has.Count.EqualTo(1));
        });
    }

    [Test]
    public void Reloading_KeepsTheSearchAndChipsInTheToolbarWithTheSpinnerInPlaceOfTheRows()
    {
        var cut = Context.Render<AddExerciseView>(p => p
            .Add(x => x.Items, SampleExercises.Strength)
            .Add(x => x.Query, "squat")
            .Add(x => x.Filter, AddExerciseFilter.Strength)
            .Add(x => x.Loading, true));

        Assert.Multiple(() =>
        {
            Assert.That(cut.Find(".toolbar [data-testid=add-exercise-search]").GetAttribute("value"), Is.EqualTo("squat"));
            Assert.That(cut.FindAll(".toolbar [data-testid=add-exercise-filter] input[checked]").Select(i => i.GetAttribute("data-testid")),
                Is.EqualTo(new[] { "add-exercise-filter-strength" }));
            Assert.That(cut.FindAll(".content [data-testid=add-exercise-loading]"), Has.Count.EqualTo(1));
            Assert.That(cut.FindAll("[data-testid^=add-exercise-row-]"), Is.Empty);
        });
    }

    [Test]
    public void IsADialogOverTheWorkoutWithABackArrowAndNoTabBarOrPullToRefresh()
    {
        var cut = RenderPicker();

        Assert.Multiple(() =>
        {
            Assert.That(cut.Find("[data-testid=add-exercise-overlay]").GetAttribute("role"), Is.EqualTo("dialog"));
            Assert.That(cut.Find("[data-testid=add-exercise-overlay] [data-testid=screen-title]").TextContent, Is.EqualTo("Exercises"));
            Assert.That(cut.FindAll("[data-testid=add-exercise-overlay] .toolbar [data-testid=add-exercise-search]"), Has.Count.EqualTo(1));
            Assert.That(cut.FindAll("button[data-testid^=add-exercise-row-]"), Has.Count.EqualTo(SampleExercises.All.Count));
            Assert.That(cut.FindAll("[data-testid=tab-bar]"), Is.Empty);
            Assert.That(cut.FindAll("[data-testid=pull-refresh]"), Is.Empty);
        });
    }

    [Test]
    public void Picking_FocusesSearch()
    {
        var cut = RenderPicker();

        Context.JSInterop.VerifyFocusAsyncInvoke().Arguments[0].ShouldBeElementReferenceTo(cut.Find("[data-testid=add-exercise-search]"));
    }

    [Test]
    public void PickingWhileLoading_ShowsTheSpinnerUnderTheSearchWhichHasFocus()
    {
        var cut = Context.Render<AddExerciseView>(p => p.Add(x => x.Items, []).Add(x => x.Loading, true).Add(x => x.OnClose, () => { }));

        Assert.Multiple(() =>
        {
            Assert.That(cut.Find(".content [data-testid=add-exercise-loading]").TextContent, Does.Contain("Loading…"));
            Assert.That(cut.FindAll(".toolbar [data-testid=add-exercise-search]"), Has.Count.EqualTo(1));
            Assert.That(cut.FindAll("[data-testid=add-exercise-back]"), Has.Count.EqualTo(1));
        });
        Context.JSInterop.VerifyFocusAsyncInvoke().Arguments[0].ShouldBeElementReferenceTo(cut.Find("[data-testid=add-exercise-search]"));
    }

    [Test]
    public void PickingBackArrow_RaisesOnClose()
    {
        var closed = false;
        var cut = Context.Render<AddExerciseView>(p => p.Add(x => x.Items, SampleExercises.All).Add(x => x.OnClose, () => closed = true));

        cut.Find("[data-testid=add-exercise-back]").Click();

        Assert.That(closed, Is.True);
    }

    [Test]
    public void PickingBack_IsTheBackIcon()
    {
        var cut = RenderPicker();

        var back = cut.FindComponent<IconButton>().Instance;

        Assert.That((back.TestId, back.Icon, back.Muted, back.Size), Is.EqualTo(("add-exercise-back", IconName.Back, false, IconSize.Default)));
    }

    [Test]
    public void PickingARow_RaisesOnPickWithIt()
    {
        ExerciseListItem? picked = null;
        var cut = Context.Render<AddExerciseView>(p => p
            .Add(x => x.Items, SampleExercises.All)
            .Add(x => x.OnPick, (ExerciseListItem item) => picked = item)
            .Add(x => x.OnClose, () => { }));

        cut.Find("[data-testid=add-exercise-row-132]").Click();

        Assert.That(picked, Is.EqualTo(SampleExercises.All.Single(e => e.Id == 132)));
    }

    [Test]
    public async Task PickingAndroidBack_ClosesTheListThenTheScreenBelowTakesBack()
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

    private IRenderedComponent<AddExerciseView> RenderPicker(IReadOnlyList<ExerciseListItem>? items = null) =>
        Context.Render<AddExerciseView>(p => p
            .Add(x => x.Items, items ?? SampleExercises.All)
            .Add(x => x.OnPick, (ExerciseListItem _) => { })
            .Add(x => x.OnClose, () => { }));
}
