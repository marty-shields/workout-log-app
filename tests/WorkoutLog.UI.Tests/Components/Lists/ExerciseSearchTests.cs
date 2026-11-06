using System.Globalization;
using WorkoutLog.Core.Domain.Exercises;
using WorkoutLog.UI.Components.Lists;
using WorkoutLog.UI.Models.Exercises;

namespace WorkoutLog.UI.Tests.Components.Lists;

public class ExerciseSearchTests : ComponentTestBase
{
    [Test]
    public void ShowsTheSearchBoxThenTheChips()
    {
        var cut = Render();

        Assert.Multiple(() =>
        {
            Assert.That(cut.Find("[data-testid=list-search]").GetAttribute("placeholder"), Is.EqualTo("Search exercises"));
            Assert.That(cut.FindAll("[data-testid=list-filter] input").Select(i => i.GetAttribute("data-testid")),
                Is.EqualTo(new[] { "list-filter-all", "list-filter-strength", "list-filter-cardio" }));
        });
    }

    [Test]
    public void Search_HasTheMaxLength() =>
        Assert.That(Render().Find("[data-testid=list-search]").GetAttribute("maxlength"),
            Is.EqualTo(ExerciseCatalog.MaxQueryLength.ToString(CultureInfo.InvariantCulture)));

    [Test]
    public void Typing_RaisesQueryChangedCutToTheLimit()
    {
        var queries = new List<string>();
        var cut = Context.Render<ExerciseSearch>(p => p.Add(x => x.QueryChanged, (string q) => queries.Add(q)).Add(x => x.TestId, "list"));

        cut.Find("[data-testid=list-search]").Input(new string('a', ExerciseCatalog.MaxQueryLength + 1));

        Assert.That(queries.Select(q => q.Length), Is.EqualTo(new[] { ExerciseCatalog.MaxQueryLength }));
    }

    [Test]
    public void TappingAChip_RaisesFilterChanged()
    {
        var changed = new List<ExerciseFilter>();
        var cut = Context.Render<ExerciseSearch>(p => p.Add(x => x.FilterChanged, (ExerciseFilter f) => changed.Add(f)).Add(x => x.TestId, "list"));

        cut.Find("[data-testid=list-filter-strength]").Change(true);

        Assert.That(changed, Is.EqualTo(new[] { ExerciseFilter.Strength }));
    }

    [Test]
    public void FocusSearch_FocusesTheSearchBox()
    {
        var cut = Context.Render<ExerciseSearch>(p => p.Add(x => x.FocusSearch, true).Add(x => x.TestId, "list"));

        Context.JSInterop.VerifyFocusAsyncInvoke().Arguments[0].ShouldBeElementReferenceTo(cut.Find("[data-testid=list-search]"));
    }

    [Test]
    public void NoFocusSearch_LeavesFocusAlone()
    {
        Render();

        Assert.That(Context.JSInterop.Invocations.Where(i => i.Identifier == "Blazor._internal.domWrapper.focus"), Is.Empty);
    }

    private IRenderedComponent<ExerciseSearch> Render() =>
        Context.Render<ExerciseSearch>(p => p.Add(x => x.TestId, "list"));
}
