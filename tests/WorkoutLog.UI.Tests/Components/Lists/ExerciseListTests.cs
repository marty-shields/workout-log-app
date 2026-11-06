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
            .Add(x => x.Items, SampleExercises.All)
            .Add(x => x.OnPick, (ExerciseListItem item) => picked = item)
            .Add(x => x.TestId, "list"));

        cut.Find("[data-testid=list-row-258]").Click();

        Assert.That(picked, Is.EqualTo(SampleExercises.All.Single(e => e.Id == 258)));
    }

    [Test]
    public void NoItems_ShowsNoMatch() =>
        Assert.That(Render([]).Find("[data-testid=list-none]").TextContent, Is.EqualTo("No exercises match"));

    private IRenderedComponent<ExerciseList> Render(IReadOnlyList<ExerciseListItem> items) =>
        Context.Render<ExerciseList>(p => p.Add(x => x.Items, items).Add(x => x.TestId, "list"));
}
