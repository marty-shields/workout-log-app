using WorkoutLog.Core.Domain.Exercises;
using WorkoutLog.UI.Components.Workout;
using WorkoutLog.UI.Haptics;
using WorkoutLog.UI.Models.Fields;
using WorkoutLog.UI.Models.Workout;

namespace WorkoutLog.UI.Tests.Components.Workout;

public class ExerciseCardHapticsTests : ComponentTestBase
{
    private static ExerciseState Curl(int sets)
    {
        var exercise = new ExerciseState(132, "Dumbbell Curl", ExerciseKind.Strength,
        [
            new FieldSpec("reps", Metric.Reps, "Reps", FieldKind.Whole, Required: true),
            new FieldSpec("weight", Metric.WeightKg, "Weight", FieldKind.Decimal, Unit: "kg"),
        ]);
        for (var i = 0; i < sets; i++)
        {
            exercise.Entries.Add(new EntryState());
        }

        return exercise;
    }

    private static ExerciseState Carry(int sets)
    {
        var exercise = new ExerciseState(187, "Farmer's Walk", ExerciseKind.Strength,
        [
            new FieldSpec("distance", Metric.Distance, "Distance", FieldKind.Decimal, Required: true, Unit: "m"),
            new FieldSpec("weight", Metric.WeightKg, "Weight", FieldKind.Decimal, Unit: "kg"),
        ]);
        for (var i = 0; i < sets; i++)
        {
            exercise.Entries.Add(new EntryState { Values = { ["distance"] = "40" } });
        }

        return exercise;
    }

    private IRenderedComponent<ExerciseCard> Render(ExerciseState exercise, bool setValid = true, bool doneValid = true) =>
        Context.Render<ExerciseCard>(p => p
            .Add(x => x.Exercise, exercise)
            .Add(x => x.ValidateSet, (_, s) =>
            {
                if (!setValid)
                {
                    s.Errors["reps"] = "Enter reps";
                }

                return setValid;
            })
            .Add(x => x.ValidateDone, _ => doneValid)
            .Add(x => x.TestId, "exercise-1"));

    [Test]
    public void AddSet_OpenSetValid_PlaysSetLoggedOnce()
    {
        var cut = Render(Curl(1));

        cut.Find("[data-testid=exercise-1-add-set]").Click();

        Assert.That(Haptics.Played, Is.EqualTo(new[] { HapticCue.SetLogged }));
    }

    [Test]
    public void AddSet_BlockedByTheOpenSet_PlaysRejectedOnce()
    {
        var cut = Render(Curl(1), setValid: false);

        cut.Find("[data-testid=exercise-1-add-set]").Click();

        Assert.That(Haptics.Played, Is.EqualTo(new[] { HapticCue.Rejected }));
    }

    [Test]
    public void AddSet_NoSetOpen_PlaysNothing()
    {
        var cut = Render(Curl(0));

        cut.Find("[data-testid=exercise-1-add-set]").Click();

        Assert.That(Haptics.Played, Is.Empty);
    }

    [Test]
    public async Task AddSetAndFocus_FromPickingTheExerciseAgain_PlaysNothing()
    {
        var cut = Render(Curl(1));

        await cut.InvokeAsync(cut.Instance.AddSetAndFocusAsync);

        Assert.That(Haptics.Played, Is.Empty);
    }

    [Test]
    public void Done_Valid_PlaysExerciseDoneOnce()
    {
        var cut = Render(Curl(1));

        cut.Find("[data-testid=exercise-1-done]").Click();

        Assert.That(Haptics.Played, Is.EqualTo(new[] { HapticCue.ExerciseDone }));
    }

    [Test]
    public void Done_Blocked_PlaysRejectedOnce()
    {
        var cut = Render(Curl(1), doneValid: false);

        cut.Find("[data-testid=exercise-1-done]").Click();

        Assert.That(Haptics.Played, Is.EqualTo(new[] { HapticCue.Rejected }));
    }

    [Test]
    public void OpeningAnotherSet_EvenWhenBlocked_PlaysNothing()
    {
        var cut = Render(Carry(2), setValid: false);

        cut.Find("[data-testid=exercise-1-set-1-edit]").Click();

        Assert.That(Haptics.Played, Is.Empty);
    }

    [Test]
    public void TypingRemovingAndReopening_PlayNothing()
    {
        var exercise = Curl(2);
        var cut = Render(exercise, setValid: false);

        cut.Find("[data-testid=exercise-1-set-2-reps]").Input("8");
        cut.Find("[data-testid=exercise-1-set-2-remove]").Click();
        exercise.IsDone = true;
        cut.Render();
        cut.Find("[data-testid=exercise-1-edit]").Click();

        Assert.That(Haptics.Played, Is.Empty);
    }
}
