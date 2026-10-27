using System.Globalization;
using Microsoft.AspNetCore.Components;
using WorkoutLog.Core.Domain.Exercises;
using WorkoutLog.UI.Components.Actions;
using WorkoutLog.UI.Components.Layout;
using WorkoutLog.UI.Components.Workout;
using WorkoutLog.UI.Models.Workout;

namespace WorkoutLog.UI.Tests.Components.Workout;

public class ExercisePickerTests : ComponentTestBase
{
    private static readonly PickerItem[] Items =
    [
        new(132, "Dumbbell Curl", ExerciseKind.Strength),
        new(258, "Plank", ExerciseKind.Strength),
        new(300, "Treadmill", ExerciseKind.Cardio),
    ];

    [Test]
    public void NoQuery_GroupsStrengthThenCardio()
    {
        var cut = Context.Render<ExercisePicker>(p => p.Add(x => x.Items, Items));

        Assert.Multiple(() =>
        {
            Assert.That(cut.FindAll(".group").Select(g => g.TextContent), Is.EqualTo(new[] { "Strength", "Cardio" }));
            Assert.That(cut.FindAll(".list")[0].TextContent, Does.Contain("Dumbbell Curl").And.Contain("Plank"));
            Assert.That(cut.FindAll(".list")[1].TextContent, Does.Contain("Treadmill"));
        });
    }

    [Test]
    public void Query_ListsItemsInGivenOrderWithoutGroups()
    {
        var cut = Context.Render<ExercisePicker>(p => p
            .Add(x => x.Query, "t")
            .Add(x => x.Items, [Items[2], Items[0]]));

        Assert.Multiple(() =>
        {
            Assert.That(cut.FindAll(".group"), Is.Empty);
            Assert.That(cut.FindAll(".item").Select(i => i.GetAttribute("data-testid")),
                Is.EqualTo(new[] { "picker-item-300", "picker-item-132" }));
        });
    }

    [TestCase("t", "Cardio")]
    [TestCase(null, null)]
    public void Query_ShowsTypeOnEachItem(string? query, string? expected)
    {
        var cut = Context.Render<ExercisePicker>(p => p
            .Add(x => x.Query, query)
            .Add(x => x.Items, [Items[2]]));

        Assert.That(cut.FindAll("[data-testid=picker-item-300] .kind").SingleOrDefault()?.TextContent, Is.EqualTo(expected));
    }

    [Test]
    public void Open_FocusesSearch()
    {
        var cut = Context.Render<ExercisePicker>(p => p.Add(x => x.Items, Items));

        Context.JSInterop.VerifyFocusAsyncInvoke().Arguments[0].ShouldBeElementReferenceTo(cut.Find("[data-testid=picker-search]"));
    }

    [Test]
    public void NoItems_ShowsNoMatch()
    {
        var cut = Context.Render<ExercisePicker>(p => p
            .Add(x => x.Query, "xyz")
            .Add(x => x.Items, []));

        Assert.That(cut.Find("[data-testid=picker-none]").TextContent, Is.EqualTo("No exercises match"));
    }

    [Test]
    public void TypingInSearch_RaisesQueryChanged()
    {
        string? query = null;
        var cut = Context.Render<ExercisePicker>(p => p
            .Add(x => x.Items, Items)
            .Add(x => x.QueryChanged, (string q) => query = q));

        cut.Find("[data-testid=picker-search]").Input("curl");

        Assert.That(query, Is.EqualTo("curl"));
    }

    [Test]
    public void Search_HasTheMaxLength() =>
        Assert.That(
            Context.Render<ExercisePicker>(p => p.Add(x => x.Items, Items)).Find("[data-testid=picker-search]").GetAttribute("maxlength"),
            Is.EqualTo(ExerciseCatalog.MaxQueryLength.ToString(CultureInfo.InvariantCulture)));

    [TestCase(ExerciseCatalog.MaxQueryLength - 1, ExerciseCatalog.MaxQueryLength - 1)]
    [TestCase(ExerciseCatalog.MaxQueryLength, ExerciseCatalog.MaxQueryLength)]
    [TestCase(ExerciseCatalog.MaxQueryLength + 1, ExerciseCatalog.MaxQueryLength)]
    [TestCase(100_000, ExerciseCatalog.MaxQueryLength)]
    public void TypingInSearch_CutsTextToTheMaxLength(int typed, int raised)
    {
        string? query = null;
        var cut = Context.Render<ExercisePicker>(p => p
            .Add(x => x.Items, Items)
            .Add(x => x.QueryChanged, (string q) => query = q));

        cut.Find("[data-testid=picker-search]").Input(new string('a', typed));

        Assert.That(query, Is.EqualTo(new string('a', raised)));
    }

    [Test]
    public void TappingItem_RaisesOnPick()
    {
        PickerItem? picked = null;
        var cut = Context.Render<ExercisePicker>(p => p
            .Add(x => x.Items, Items)
            .Add(x => x.OnPick, (PickerItem item) => picked = item));

        cut.Find("[data-testid=picker-item-258]").Click();

        Assert.That(picked, Is.EqualTo(Items[1]));
    }

    [Test]
    public void Back_RaisesOnClose()
    {
        var closed = false;
        var cut = Context.Render<ExercisePicker>(p => p
            .Add(x => x.Items, Items)
            .Add(x => x.OnClose, () => closed = true));

        cut.Find("[data-testid=picker-back]").Click();

        Assert.That(closed, Is.True);
    }

    [Test]
    public void Back_IsTheBackIcon()
    {
        var cut = Context.Render<ExercisePicker>(p => p.Add(x => x.Items, Items));

        var back = cut.FindComponent<IconButton>().Instance;

        Assert.That((back.TestId, back.Icon, back.Muted, back.Size), Is.EqualTo(("picker-back", IconName.Back, false, IconSize.Default)));
    }

    [Test]
    public async Task AndroidBack_ClosesThePickerThenTheScreenBelowTakesBack()
    {
        var screenBack = 0;
        var closed = 0;
        RenderFragment picker = b =>
        {
            b.OpenComponent<ExercisePicker>(0);
            b.AddComponentParameter(1, nameof(ExercisePicker.Items), Items);
            b.AddComponentParameter(2, nameof(ExercisePicker.OnClose), EventCallback.Factory.Create(this, () => closed++));
            b.CloseComponent();
        };
        var cut = Context.Render<AppScreen>(p => p
            .Add(x => x.Title, "New Workout")
            .Add(x => x.OnBack, () => screenBack++)
            .Add(x => x.ChildContent, picker));

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
        Context.Render<ExercisePicker>(p => p.Add(x => x.Items, Items));

        Assert.That(BackButton.HasHandler, Is.False);
    }
}
