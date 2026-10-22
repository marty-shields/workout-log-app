using Microsoft.AspNetCore.Components;
using WorkoutLog.UI.Models.Fields;
using WorkoutLog.UI.Models.Workout;
using WorkoutLog.UI.Screens;

namespace WorkoutLog.UI.Tests.Screens;

public class WorkoutViewTests : ComponentTestBase
{
    private static readonly DateTime Started = new(2026, 10, 20, 18, 5, 0);

    private static ExerciseState Curl()
    {
        var exercise = new ExerciseState("Dumbbell Curl", ExerciseKind.Strength,
            [new FieldSpec("reps", "Reps", FieldKind.Whole, Required: true)]);
        exercise.Entries.Add(new EntryState());
        return exercise;
    }

    private static ExerciseState Carry()
    {
        var exercise = new ExerciseState("Farmer's Walk", ExerciseKind.Strength,
        [
            new FieldSpec("distance", "Distance", FieldKind.Decimal, Required: true, Unit: "m"),
            new FieldSpec("weight", "Weight", FieldKind.Decimal, Unit: "kg"),
            new FieldSpec("duration", "Duration", FieldKind.Duration),
        ]);
        var entry = new EntryState();
        entry.Values["distance"] = "40";
        exercise.Entries.Add(entry);
        return exercise;
    }

    [TestCase(WorkoutMode.New, true)]
    [TestCase(WorkoutMode.Edit, false)]
    public void CarrySets_OpenLastSetOnlyForNewWorkout(WorkoutMode mode, bool open)
    {
        var cut = Context.Render<WorkoutView>(p => p
            .Add(x => x.Mode, mode)
            .Add(x => x.StartedAt, Started)
            .Add(x => x.Exercises, [Carry()]));

        Assert.That(cut.FindAll(".panel").Count, Is.EqualTo(open ? 1 : 0));
    }

    [Test]
    public void ValidateSet_IsPassedToExerciseCards()
    {
        var curl = Curl();
        var cut = Context.Render<WorkoutView>(p => p
            .Add(x => x.StartedAt, Started)
            .Add(x => x.Exercises, [curl])
            .Add(x => x.ValidateSet, (_, _) => false));

        cut.Find("[data-testid=exercise-1-add-set]").Click();

        Assert.That(curl.Entries, Has.Count.EqualTo(1));
    }

    [Test]
    public void New_ShowsDateAndElapsedTime()
    {
        var cut = Context.Render<WorkoutView>(p => p
            .Add(x => x.Mode, WorkoutMode.New)
            .Add(x => x.StartedAt, Started)
            .Add(x => x.Time, new TimeSpan(0, 12, 34))
            .Add(x => x.Exercises, []));

        Assert.Multiple(() =>
        {
            Assert.That(cut.Find("[data-testid=screen-title]").TextContent, Is.EqualTo("Tue 20 Oct 2026"));
            Assert.That(cut.Find("[data-testid=screen-subtitle]").TextContent, Is.EqualTo("Elapsed 0:12:34"));
            Assert.That(cut.FindAll("[data-testid=delete-workout]"), Is.Empty);
        });
    }

    [Test]
    public void Edit_ShowsTotalTimeAndDelete()
    {
        var deleted = false;
        var cut = Context.Render<WorkoutView>(p => p
            .Add(x => x.Mode, WorkoutMode.Edit)
            .Add(x => x.StartedAt, Started)
            .Add(x => x.Time, new TimeSpan(1, 2, 3))
            .Add(x => x.Exercises, [Curl()])
            .Add(x => x.OnDelete, () => deleted = true));

        cut.Find("[data-testid=delete-workout]").Click();

        Assert.Multiple(() =>
        {
            Assert.That(cut.Find("[data-testid=screen-subtitle]").TextContent, Is.EqualTo("Total 1:02:03"));
            Assert.That(deleted, Is.True);
        });
    }

    [Test]
    public void ElapsedOverADay_ShowsTotalHours()
    {
        var cut = Context.Render<WorkoutView>(p => p
            .Add(x => x.StartedAt, Started)
            .Add(x => x.Time, new TimeSpan(1, 2, 3, 4))
            .Add(x => x.Exercises, []));

        Assert.That(cut.Find("[data-testid=screen-subtitle]").TextContent, Is.EqualTo("Elapsed 26:03:04"));
    }

    [Test]
    public void NoExercises_ShowsEmptyState()
    {
        var cut = Context.Render<WorkoutView>(p => p
            .Add(x => x.StartedAt, Started)
            .Add(x => x.Exercises, []));

        Assert.That(cut.Find("[data-testid=no-exercises]").TextContent, Does.Contain("No exercises yet"));
    }

    [Test]
    public void Exercises_RenderInOrder()
    {
        var cut = Context.Render<WorkoutView>(p => p
            .Add(x => x.StartedAt, Started)
            .Add(x => x.Exercises, [Curl(), new ExerciseState("Plank", ExerciseKind.Strength, [])]));

        Assert.That(cut.FindAll(".name").Select(n => n.TextContent), Is.EqualTo(new[] { "Dumbbell Curl", "Plank" }));
    }

    [Test]
    public void Error_IsShown()
    {
        var cut = Context.Render<WorkoutView>(p => p
            .Add(x => x.StartedAt, Started)
            .Add(x => x.Exercises, [])
            .Add(x => x.Error, "Add at least one exercise"));

        Assert.That(cut.Find("[data-testid=workout-error]").TextContent, Is.EqualTo("Add at least one exercise"));
    }

    [Test]
    public void BottomBar_RaisesAddExerciseAndSave()
    {
        var added = false;
        var saved = false;
        var cut = Context.Render<WorkoutView>(p => p
            .Add(x => x.StartedAt, Started)
            .Add(x => x.Exercises, [])
            .Add(x => x.OnAddExercise, () => added = true)
            .Add(x => x.OnSave, () => saved = true));

        cut.Find("[data-testid=bottom-bar] [data-testid=add-exercise]").Click();
        cut.Find("[data-testid=bottom-bar] [data-testid=save-workout]").Click();

        Assert.Multiple(() =>
        {
            Assert.That(added, Is.True);
            Assert.That(saved, Is.True);
        });
    }

    [Test]
    public void RemoveExercise_RaisesWithThatExercise()
    {
        var curl = Curl();
        ExerciseState? removed = null;
        var cut = Context.Render<WorkoutView>(p => p
            .Add(x => x.StartedAt, Started)
            .Add(x => x.Exercises, [curl])
            .Add(x => x.OnRemoveExercise, (ExerciseState e) => removed = e));

        cut.Find("[data-testid=exercise-1-remove]").Click();

        Assert.That(removed, Is.SameAs(curl));
    }

    [Test]
    public void Overlay_IsRendered()
    {
        var cut = Context.Render<WorkoutView>(p => p
            .Add(x => x.StartedAt, Started)
            .Add(x => x.Exercises, [])
            .Add(x => x.Overlay, (RenderFragment)(b => b.AddMarkupContent(0, "<div data-testid=\"overlay\"></div>"))));

        Assert.That(cut.Find("[data-testid=overlay]"), Is.Not.Null);
    }
}
