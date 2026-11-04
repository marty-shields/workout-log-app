using WorkoutLog.Core.Domain.Exercises;
using WorkoutLog.UI.Components.Workout;
using WorkoutLog.UI.Models.Fields;
using WorkoutLog.UI.Models.Workout;

namespace WorkoutLog.UI.Tests.Components.Workout;

public class ExerciseCardSetKindsTests : ComponentTestBase
{
    private static readonly Dictionary<string, FieldSpec[]> Specs = new()
    {
        ["strength"] =
        [
            new("reps", Metric.Reps, "Reps", FieldKind.Whole, Required: true),
            new("weight", Metric.WeightKg, "Weight", FieldKind.Decimal, Unit: "kg"),
        ],
        ["hold"] =
        [
            new("duration", Metric.Duration, "Duration", FieldKind.Duration, Required: true),
            new("weight", Metric.WeightKg, "Weight", FieldKind.Decimal, Unit: "kg", HideZero: true),
        ],
        ["assisted"] =
        [
            new("reps", Metric.Reps, "Reps", FieldKind.Whole, Required: true),
            new("weight", Metric.WeightKg, "Assist", FieldKind.Decimal, Unit: "kg", BlankFill: "0"),
        ],
        ["carry"] =
        [
            new("distance", Metric.Distance, "Distance", FieldKind.Decimal, Required: true, Unit: "m"),
            new("weight", Metric.WeightKg, "Weight", FieldKind.Decimal, Unit: "kg"),
            new("duration", Metric.Duration, "Duration", FieldKind.Duration),
        ],
    };

    private static readonly string[] Kinds = [.. Specs.Keys];

    private static ExerciseState Build(string kind, int sets)
    {
        var fields = Specs[kind];
        var exercise = new ExerciseState(1, kind, ExerciseKind.Strength, fields);
        for (var i = 0; i < sets; i++)
        {
            var entry = new EntryState();
            entry.Values[fields[0].Key] = "5";
            exercise.Entries.Add(entry);
        }

        return exercise;
    }

    private IRenderedComponent<ExerciseCard> Render(ExerciseState exercise, Func<ExerciseState, EntryState, bool>? validate = null, Action<EntryState>? ask = null) =>
        Context.Render<ExerciseCard>(p =>
        {
            p.Add(x => x.Exercise, exercise).Add(x => x.TestId, "exercise-1");
            if (validate is not null)
            {
                p.Add(x => x.ValidateSet, validate);
            }

            if (ask is not null)
            {
                p.Add(x => x.OnRemoveClosedSet, ask);
            }
        });

    private static List<string?> OpenSets(IRenderedComponent<ExerciseCard> cut) =>
        cut.FindAll(".set-row, .panel").Select(e => e.GetAttribute("data-testid")).ToList();

    private static bool Invalid(ExerciseState exercise, EntryState entry)
    {
        entry.Errors[exercise.Fields[0].Key] = "bad";
        return false;
    }

    [TestCaseSource(nameof(Kinds))]
    public void OnlyTheLastSetIsOpenAtFirst(string kind)
    {
        var cut = Render(Build(kind, 3));

        Assert.That(OpenSets(cut), Is.EqualTo(new[] { "exercise-1-set-3" }));
    }

    [TestCaseSource(nameof(Kinds))]
    public void Pencil_OpensThatSetAndClosesTheOther(string kind)
    {
        var cut = Render(Build(kind, 3), (_, _) => true);

        cut.Find("[data-testid=exercise-1-set-1-edit]").Click();

        Assert.That(OpenSets(cut), Is.EqualTo(new[] { "exercise-1-set-1" }));
    }

    [TestCaseSource(nameof(Kinds))]
    public void Pencil_BlockedWhileTheOpenSetIsInvalid(string kind)
    {
        var cut = Render(Build(kind, 3), Invalid);

        cut.Find("[data-testid=exercise-1-set-1-edit]").Click();

        Assert.That(OpenSets(cut), Is.EqualTo(new[] { "exercise-1-set-3" }));
    }

    [TestCaseSource(nameof(Kinds))]
    public void AddSet_ClosesTheValidSetAndOpensTheNewOne(string kind)
    {
        var cut = Render(Build(kind, 2), (_, _) => true);

        cut.Find("[data-testid=exercise-1-add-set]").Click();

        Assert.That(OpenSets(cut), Is.EqualTo(new[] { "exercise-1-set-3" }));
    }

    [TestCaseSource(nameof(Kinds))]
    public void AddSet_BlockedWhileTheOpenSetIsInvalid(string kind)
    {
        var exercise = Build(kind, 2);
        var cut = Render(exercise, Invalid);

        cut.Find("[data-testid=exercise-1-add-set]").Click();

        Assert.Multiple(() =>
        {
            Assert.That(exercise.Entries, Has.Count.EqualTo(2));
            Assert.That(OpenSets(cut), Is.EqualTo(new[] { "exercise-1-set-2" }));
        });
    }

    [TestCaseSource(nameof(Kinds))]
    public void SetGivenWithErrors_OpensAndIsNeverShownClosed(string kind)
    {
        var exercise = Build(kind, 3);
        exercise.Entries[0].Errors[exercise.Fields[0].Key] = "bad";

        var cut = Render(exercise, (_, _) => true);

        Assert.Multiple(() =>
        {
            Assert.That(OpenSets(cut), Is.EqualTo(new[] { "exercise-1-set-1" }));
            Assert.That(cut.FindAll(".summary .error"), Is.Empty);
        });
    }

    [TestCaseSource(nameof(Kinds))]
    public void DoneThenReopen_LeavesEverySetCollapsed(string kind)
    {
        var exercise = Build(kind, 3);
        var cut = Render(exercise);
        cut.Find("[data-testid=exercise-1-set-1-edit]").Click();

        cut.Find("[data-testid=exercise-1-done]").Click();
        cut.Find("[data-testid=exercise-1-edit]").Click();

        Assert.Multiple(() =>
        {
            Assert.That(exercise.IsDone, Is.False);
            Assert.That(OpenSets(cut), Is.Empty);
            Assert.That(cut.FindAll(".summary"), Has.Count.EqualTo(3));
        });
    }

    [TestCaseSource(nameof(Kinds))]
    public void Reopen_WithoutErrors_KeepsEverySetCollapsedAfterAnotherRender(string kind)
    {
        var exercise = Build(kind, 2);
        exercise.IsDone = true;
        var cut = Render(exercise);

        cut.Find("[data-testid=exercise-1-edit]").Click();
        cut.Render(p => p.Add(x => x.Exercise, exercise).Add(x => x.TestId, "exercise-1"));

        Assert.That(OpenSets(cut), Is.Empty);
    }

    [TestCaseSource(nameof(Kinds))]
    public void Done_BlockedWhenTheExerciseFailsTheCheck_StaysOpen(string kind)
    {
        var exercise = Build(kind, 1);
        var cut = Context.Render<ExerciseCard>(p => p
            .Add(x => x.Exercise, exercise)
            .Add(x => x.ValidateDone, e => { e.Error = "No"; return false; })
            .Add(x => x.TestId, "exercise-1"));

        cut.Find("[data-testid=exercise-1-done]").Click();

        Assert.Multiple(() =>
        {
            Assert.That(exercise.IsDone, Is.False);
            Assert.That(OpenSets(cut), Is.EqualTo(new[] { "exercise-1-set-1" }));
        });
    }

    [TestCaseSource(nameof(Kinds))]
    public void Done_BlockedByANoteError_DoesNotCollapse(string kind)
    {
        var exercise = Build(kind, 1);
        var cut = Context.Render<ExerciseCard>(p => p
            .Add(x => x.Exercise, exercise)
            .Add(x => x.ValidateDone, e => { e.NoteError = "Too long"; return false; })
            .Add(x => x.TestId, "exercise-1"));

        cut.Find("[data-testid=exercise-1-done]").Click();

        Assert.That(exercise.IsDone, Is.False);
    }

    [TestCaseSource(nameof(Kinds))]
    public void Done_WhenValid_Collapses(string kind)
    {
        var exercise = Build(kind, 1);
        var cut = Context.Render<ExerciseCard>(p => p
            .Add(x => x.Exercise, exercise)
            .Add(x => x.ValidateDone, _ => true)
            .Add(x => x.TestId, "exercise-1"));

        cut.Find("[data-testid=exercise-1-done]").Click();

        Assert.That(exercise.IsDone, Is.True);
    }

    [TestCaseSource(nameof(Kinds))]
    public void RemovingTheOpenSet_LeavesNoneOpen(string kind)
    {
        var exercise = Build(kind, 3);
        var cut = Render(exercise);

        cut.Find("[data-testid=exercise-1-set-3-remove]").Click();

        Assert.Multiple(() =>
        {
            Assert.That(exercise.Entries, Has.Count.EqualTo(2));
            Assert.That(OpenSets(cut), Is.Empty);
        });
    }

    [TestCaseSource(nameof(Kinds))]
    public void TrashOnAClosedSet_AsksAndKeepsTheOpenSetOpen(string kind)
    {
        var exercise = Build(kind, 3);
        EntryState? asked = null;
        var cut = Render(exercise, ask: e => asked = e);

        cut.Find("[data-testid=exercise-1-set-1-remove]").Click();

        Assert.Multiple(() =>
        {
            Assert.That(asked, Is.SameAs(exercise.Entries[0]));
            Assert.That(exercise.Entries, Has.Count.EqualTo(3));
            Assert.That(OpenSets(cut), Is.EqualTo(new[] { "exercise-1-set-3" }));
        });
    }

    [TestCaseSource(nameof(Kinds))]
    public void ClosedRow_ShowsTheNumberAndValuesOnly(string kind)
    {
        var cut = Render(Build(kind, 2));

        var summary = cut.Find("[data-testid=exercise-1-set-1]");

        Assert.Multiple(() =>
        {
            Assert.That(summary.TextContent, Does.Not.Contain("Last time"));
            Assert.That(summary.QuerySelectorAll("input"), Is.Empty);
        });
    }
}
