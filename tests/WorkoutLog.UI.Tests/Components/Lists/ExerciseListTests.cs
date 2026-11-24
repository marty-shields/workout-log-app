using WorkoutLog.UI.Components.Lists;
using WorkoutLog.UI.Models.Exercises;
using WorkoutLog.UI.Models.Workout;
using WorkoutLog.UI.Stories.Samples;

namespace WorkoutLog.UI.Tests.Components.Lists;

public class ExerciseListTests : ComponentTestBase
{
    [Test]
    public void Rows_ShowTheNameLastSessionAndKindInTheOrderGiven()
    {
        var cut = Render(SampleExercises.All);

        Assert.Multiple(() =>
        {
            Assert.That(cut.FindAll("[data-testid^=list-row-]").Select(r => r.GetAttribute("data-testid")),
                Is.EqualTo(SampleExercises.All.Select(e => $"list-row-{e.Id}")));
            Assert.That(cut.Find("[data-testid=list-row-311] .text").TextContent, Is.EqualTo("Outdoor Run"));
            Assert.That(cut.Find("[data-testid=list-row-311] .detail").TextContent, Is.EqualTo("Last Sun 11 Oct · 5.2km"));
            Assert.That(cut.FindAll("[data-testid^=list-row-] .aside").Select(a => a.TextContent), Is.EqualTo(SampleExercises.All.Select(e => e.Kind.ToString())));
        });
    }

    [Test]
    public void NotLogged_HasNoDetail()
    {
        var cut = Render([new ExerciseListItem(1, "Ab Wheel Rollout", ExerciseKind.Strength, null)]);

        Assert.That(cut.FindAll("[data-testid=list-row-1] .detail"), Is.Empty);
    }

    [Test]
    public void WithoutOnPick_RowsAreNotTappable() =>
        Assert.That(Render(SampleExercises.All).FindAll("button"), Is.Empty);

    [Test]
    public void TappingARow_RaisesOnPickWithIt()
    {
        ExerciseListItem? picked = null;
        var cut = Context.Render<ExerciseList>(p => p
            .Add(x => x.Items, ExerciseListGroups.Flat(SampleExercises.All))
            .Add(x => x.OnPick, (ExerciseListItem item) => picked = item)
            .Add(x => x.TestId, "list"));

        cut.Find("[data-testid=list-row-258]").Click();

        Assert.That(picked, Is.EqualTo(SampleExercises.All.Single(e => e.Id == 258)));
    }

    [Test]
    public void NoItems_ShowsNoMatch() =>
        Assert.That(Render([]).Find("[data-testid=list-none]").TextContent, Is.EqualTo("No exercises match"));

    [Test]
    public void Grouped_ShowsARecentHeaderThenAnOtherHeaderEachWithItsRowsAndCount()
    {
        ExerciseListItem[] recent = [SampleExercises.All[0], SampleExercises.All[1]];
        ExerciseListItem[] other = [new(1, "Ab Wheel Rollout", ExerciseKind.Strength, null), new(2, "Bike", ExerciseKind.Cardio, null), new(3, "Crunch", ExerciseKind.Strength, null)];

        var cut = RenderGroups(new ExerciseListGroups(recent, other, true));

        Assert.Multiple(() =>
        {
            Assert.That(cut.Find("[data-testid=list-group-recent-title]").TextContent, Is.EqualTo("Recent Exercises"));
            Assert.That(cut.Find("[data-testid=list-group-recent-total]").TextContent, Is.EqualTo("2"));
            Assert.That(cut.Find("[data-testid=list-group-other-title]").TextContent, Is.EqualTo("Other Exercises"));
            Assert.That(cut.Find("[data-testid=list-group-other-total]").TextContent, Is.EqualTo("3"));
            Assert.That(cut.FindAll("[data-testid^=list-row-]").Select(r => r.GetAttribute("data-testid")),
                Is.EqualTo(recent.Concat(other).Select(e => $"list-row-{e.Id}")));
            Assert.That(cut.FindAll("[data-testid=list-group-recent] ~ ul [data-testid^=list-row-]"), Has.Count.EqualTo(2));
        });
    }

    [Test]
    public void Grouped_AGroupWithNoRowsIsLeftOutWithItsHeader()
    {
        var cut = RenderGroups(new ExerciseListGroups([], [new(2, "Bike", ExerciseKind.Cardio, null)], true));

        Assert.Multiple(() =>
        {
            Assert.That(cut.FindAll("[data-testid=list-group-recent]"), Is.Empty);
            Assert.That(cut.Find("[data-testid=list-group-other-title]").TextContent, Is.EqualTo("Other Exercises"));
        });
    }

    [Test]
    public void Grouped_BothGroupsEmpty_ShowsNoMatchAndNoHeaders()
    {
        var cut = RenderGroups(new ExerciseListGroups([], [], true));

        Assert.Multiple(() =>
        {
            Assert.That(cut.Find("[data-testid=list-none]").TextContent, Is.EqualTo("No exercises match"));
            Assert.That(cut.FindAll("[data-testid^=list-group-]"), Is.Empty);
        });
    }

    [Test]
    public void Flat_HasNoHeaders() =>
        Assert.That(Render(SampleExercises.All).FindAll("[data-testid^=list-group-]"), Is.Empty);

    [Test]
    public void GroupHeaders_AreHeadings() =>
        Assert.That(RenderGroups(new ExerciseListGroups([SampleExercises.All[0]], [new(2, "Bike", ExerciseKind.Cardio, null)], true))
            .FindAll("h1, h2, h3, h4, h5, h6").Select(h => h.QuerySelector("[data-testid$=-title]")?.TextContent),
            Is.EqualTo(new[] { "Recent Exercises", "Other Exercises" }));

    private IRenderedComponent<ExerciseList> RenderGroups(ExerciseListGroups groups) =>
        Context.Render<ExerciseList>(p => p.Add(x => x.Items, groups).Add(x => x.TestId, "list"));

    private IRenderedComponent<ExerciseList> Render(IReadOnlyList<ExerciseListItem> items) =>
        Context.Render<ExerciseList>(p => p.Add(x => x.Items, ExerciseListGroups.Flat(items)).Add(x => x.TestId, "list"));
}
