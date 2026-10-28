using WorkoutLog.Core.Domain.Exercises;
using WorkoutLog.UI.Models.Exercises;
using WorkoutLog.UI.Screens;
using WorkoutLog.UI.Stories.Samples;

namespace WorkoutLog.UI.Tests.Screens;

public class ExercisesViewTests : ComponentTestBase
{
    [Test]
    public void List_ShowsSearchChipsAndARowPerExerciseWithItsLastSession()
    {
        var cut = Context.Render<ExercisesView>(p => p.Add(x => x.Items, SampleExercises.All.Take(2).ToList()).Add(x => x.OnOpen, (ExerciseListItem _) => { }));

        Assert.Multiple(() =>
        {
            Assert.That(cut.Find("[data-testid=screen-title]").TextContent, Is.EqualTo("Exercises"));
            Assert.That(cut.Find("[data-testid=exercises-search]").GetAttribute("placeholder"), Is.EqualTo("Search exercises"));
            Assert.That(cut.FindAll("[data-testid=exercises-filter] .text").Select(t => t.TextContent), Is.EqualTo(new[] { "All", "Strength", "Cardio" }));
            Assert.That(cut.FindAll("[data-testid^=exercises-row-] .text").Select(t => t.TextContent), Is.EqualTo(new[] { "Barbell Back Squat", "Machine Assisted Pull-up" }));
            Assert.That(cut.FindAll("[data-testid^=exercises-row-] .detail").Select(d => d.TextContent),
                Is.EqualTo(new[] { "Last Tue 20 Oct · 70kg × 3", "Last Tue 20 Oct · 0kg × 5" }));
        });
    }

    [TestCase(ExerciseFilter.All, "exercises-filter-all")]
    [TestCase(ExerciseFilter.Strength, "exercises-filter-strength")]
    [TestCase(ExerciseFilter.Cardio, "exercises-filter-cardio")]
    public void Filter_ChecksItsChip(ExerciseFilter filter, string testId)
    {
        var cut = Context.Render<ExercisesView>(p => p.Add(x => x.Items, SampleExercises.All).Add(x => x.Filter, filter));

        Assert.That(cut.FindAll("[data-testid=exercises-filter] input[checked]").Select(i => i.GetAttribute("data-testid")), Is.EqualTo(new[] { testId }));
    }

    [Test]
    public void TappingAChip_RaisesFilterChanged()
    {
        var changed = new List<ExerciseFilter>();
        var cut = Context.Render<ExercisesView>(p => p.Add(x => x.Items, SampleExercises.All).Add(x => x.FilterChanged, (ExerciseFilter f) => changed.Add(f)));

        cut.Find("[data-testid=exercises-filter-cardio]").Change(true);

        Assert.That(changed, Is.EqualTo(new[] { ExerciseFilter.Cardio }));
    }

    [Test]
    public void Typing_RaisesQueryChangedCutToTheLimit()
    {
        var queries = new List<string>();
        var cut = Context.Render<ExercisesView>(p => p.Add(x => x.Items, SampleExercises.All).Add(x => x.QueryChanged, (string q) => queries.Add(q)));

        cut.Find("[data-testid=exercises-search]").Input("squat");
        cut.Find("[data-testid=exercises-search]").Input(new string('a', ExerciseCatalog.MaxQueryLength + 1));

        Assert.That(queries.Select(q => q.Length), Is.EqualTo(new[] { 5, ExerciseCatalog.MaxQueryLength }));
    }

    [Test]
    public void Query_FillsTheSearchBox()
    {
        var cut = Context.Render<ExercisesView>(p => p.Add(x => x.Items, []).Add(x => x.Query, "deadlift"));

        Assert.That(cut.Find("[data-testid=exercises-search]").GetAttribute("value"), Is.EqualTo("deadlift"));
    }

    [Test]
    public void TappingALiftRow_RaisesOnOpenWithIt()
    {
        ExerciseListItem? opened = null;
        var cut = Context.Render<ExercisesView>(p => p.Add(x => x.Items, SampleExercises.All).Add(x => x.OnOpen, (ExerciseListItem item) => opened = item));

        cut.Find("[data-testid=exercises-row-6]").Click();

        Assert.That(opened, Is.EqualTo(SampleExercises.All.Single(e => e.Id == 6)));
    }

    [Test]
    public void WithOnOpen_EveryRowIsTappable()
    {
        var cut = Context.Render<ExercisesView>(p => p.Add(x => x.Items, SampleExercises.All).Add(x => x.OnOpen, (ExerciseListItem _) => { }));

        Assert.That(cut.FindAll("button[data-testid^=exercises-row-]").Select(r => r.GetAttribute("data-testid")),
            Is.EqualTo(SampleExercises.All.Select(e => $"exercises-row-{e.Id}")));
    }

    [Test]
    public void NoOnOpen_RowsAreNotTappableAndShowTheirKind()
    {
        var cut = Context.Render<ExercisesView>(p => p.Add(x => x.Items, SampleExercises.All));

        Assert.Multiple(() =>
        {
            Assert.That(cut.FindAll("button[data-testid^=exercises-row-]"), Is.Empty);
            Assert.That(cut.FindAll("[data-testid^=exercises-row-] .aside").Select(a => a.TextContent), Is.EqualTo(SampleExercises.All.Select(e => e.Kind.ToString())));
        });
    }

    [Test]
    public void NoItems_ShowsNoExercisesMatchUnderTheSearchAndChips()
    {
        var cut = Context.Render<ExercisesView>(p => p.Add(x => x.Items, []).Add(x => x.Query, "deadlift"));

        Assert.Multiple(() =>
        {
            Assert.That(cut.Find("[data-testid=exercises-none]").TextContent, Is.EqualTo("No exercises match"));
            Assert.That(cut.FindAll("[data-testid=exercises-search]"), Has.Count.EqualTo(1));
            Assert.That(cut.FindAll("[data-testid=exercises-filter]"), Has.Count.EqualTo(1));
        });
    }

    [Test]
    public void NothingLogged_ShowsTheMessageWithNoSearchOrChips()
    {
        var cut = Context.Render<ExercisesView>(p => p.Add(x => x.Items, []).Add(x => x.NothingLogged, true));

        Assert.Multiple(() =>
        {
            Assert.That(TestText.Normalise(cut.Find("[data-testid=exercises-empty]").TextContent), Is.EqualTo("No exercises logged yet"));
            Assert.That(cut.FindAll("[data-testid=exercises-search]"), Is.Empty);
            Assert.That(cut.FindAll("[data-testid=exercises-filter]"), Is.Empty);
            Assert.That(cut.FindAll("[data-testid=exercises-none]"), Is.Empty);
        });
    }

    [Test]
    public void Loading_ShowsTheSpinnerOnly()
    {
        var cut = Context.Render<ExercisesView>(p => p.Add(x => x.Items, SampleExercises.All).Add(x => x.Loading, true).Add(x => x.OnBack, () => { }));

        Assert.Multiple(() =>
        {
            Assert.That(cut.Find("[data-testid=exercises-loading]").TextContent, Does.Contain("Loading…"));
            Assert.That(cut.FindAll("[data-testid=exercises-search]"), Is.Empty);
            Assert.That(cut.FindAll("[data-testid^=exercises-row-]"), Is.Empty);
            Assert.That(cut.FindAll("[data-testid=back]"), Has.Count.EqualTo(1));
        });
    }

    [Test]
    public void Failed_ShowsTheErrorAndTryAgainInsteadOfTheContent()
    {
        var cut = Context.Render<ExercisesView>(p => p.Add(x => x.Items, SampleExercises.All).Add(x => x.Failed, true));

        Assert.Multiple(() =>
        {
            Assert.That(cut.Find("[data-testid=exercises-error]").TextContent, Is.EqualTo("Couldn't load your exercises"));
            Assert.That(cut.Find("[data-testid=exercises-retry]").TextContent.Trim(), Is.EqualTo("Try again"));
            Assert.That(cut.FindAll("[data-testid=exercises-search]"), Is.Empty);
            Assert.That(cut.FindAll("[data-testid^=exercises-row-]"), Is.Empty);
        });
    }

    [Test]
    public void TryAgain_RaisesOnRetry()
    {
        var retries = 0;
        var cut = Context.Render<ExercisesView>(p => p.Add(x => x.Items, []).Add(x => x.Failed, true).Add(x => x.OnRetry, () => retries++));

        cut.Find("[data-testid=exercises-retry]").Click();

        Assert.That(retries, Is.EqualTo(1));
    }

    [Test]
    public void Failed_FocusesTryAgain()
    {
        var cut = Context.Render<ExercisesView>(p => p.Add(x => x.Items, []).Add(x => x.Failed, true));

        cut.WaitForAssertion(() => Context.JSInterop.Invocations["Blazor._internal.domWrapper.focus"].Single().Arguments[0]
            .ShouldBeElementReferenceTo(cut.Find("[data-testid=exercises-retry]")));
    }

    [Test]
    public void Back_RaisesOnBack()
    {
        var back = false;
        var cut = Context.Render<ExercisesView>(p => p.Add(x => x.Items, SampleExercises.All).Add(x => x.OnBack, () => back = true));

        cut.Find("[data-testid=back]").Click();

        Assert.That(back, Is.True);
    }
}
