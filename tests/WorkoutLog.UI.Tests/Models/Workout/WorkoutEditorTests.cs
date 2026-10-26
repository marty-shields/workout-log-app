using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Time.Testing;
using WorkoutLog.Core.Domain.Exercises;
using WorkoutLog.Core.Domain.Workouts;
using WorkoutLog.UI.Models.Workout;
using WorkoutLog.UI.Stories.Samples;

namespace WorkoutLog.UI.Tests.Models.Workout;

public class WorkoutEditorTests
{
    private static readonly IExerciseCatalog Catalog = new ExerciseCatalog();

    private const int Squat = 6;
    private const int Treadmill = 300;

    private WorkoutChecker workoutChecker = null!;
    private WorkoutEditor editor = null!;

    [SetUp]
    public void CreateEditor()
    {
        var workouts = new SampleWorkoutService(new FakeTimeProvider(), Catalog);
        var setChecker = new SetChecker(workouts, Catalog, NullLogger<SetChecker>.Instance);
        workoutChecker = new WorkoutChecker(setChecker, workouts, NullLogger<WorkoutChecker>.Instance);
        editor = new WorkoutEditor(Catalog, setChecker, workoutChecker);
    }

    [Test]
    public void Constructor_IsEmptyClosedUnchangedAndNotSaving()
    {
        Assert.Multiple(() =>
        {
            Assert.That(editor.Exercises, Is.Empty);
            Assert.That(editor.PickerOpen, Is.False);
            Assert.That(editor.Changed, Is.False);
            Assert.That(editor.Saving, Is.False);
        });
    }

    [Test]
    public void OpenPicker_ListsTheWholeCatalogWithNoSearch()
    {
        editor.Search("squat");

        editor.OpenPicker();

        Assert.Multiple(() =>
        {
            Assert.That(editor.PickerOpen, Is.True);
            Assert.That(editor.Query, Is.Empty);
            Assert.That(editor.PickerItems, Has.Count.EqualTo(Catalog.All.Count));
        });
    }

    [Test]
    public void Search_SetsTheQueryAndTheCatalogsResults()
    {
        editor.Search("squat");

        Assert.Multiple(() =>
        {
            Assert.That(editor.Query, Is.EqualTo("squat"));
            Assert.That(editor.PickerItems, Is.EqualTo(Catalog.Search("squat").Select(ExerciseFields.PickerItem)));
        });
    }

    [Test]
    public void ClosePicker_ClosesIt()
    {
        editor.OpenPicker();

        editor.ClosePicker();

        Assert.That(editor.PickerOpen, Is.False);
    }

    [Test]
    public async Task Pick_New_AddsItAtTheTopFocusesItClosesThePickerAndClearsTheError()
    {
        await editor.SaveAsync(_ => Task.FromResult(true));
        Assume.That(editor.Error, Is.Not.Null);
        editor.Pick(Item(Treadmill));
        editor.OpenPicker();

        editor.Pick(Item(Squat));

        var squat = editor.Exercises[0];
        Assert.Multiple(() =>
        {
            Assert.That(editor.Exercises.Select(e => e.ExerciseId), Is.EqualTo(new[] { Squat, Treadmill }));
            Assert.That(squat.Entries, Has.Count.EqualTo(1));
            Assert.That(editor.Focus!.Exercise, Is.SameAs(squat));
            Assert.That(editor.Focus.AddSet, Is.False);
            Assert.That(editor.PickerOpen, Is.False);
            Assert.That(editor.Error, Is.Null);
            Assert.That(editor.Changed, Is.True);
        });
    }

    [Test]
    public void Pick_StrengthAgain_MovesItToTheTopFocusesItAndAsksForASet()
    {
        var squat = SampleData.Exercise(Catalog, Squat, ["8", "60"]);
        editor.Start([squat, SampleData.Exercise(Catalog, Treadmill, ["0:20:00"])]);

        editor.Pick(Item(Squat));

        Assert.Multiple(() =>
        {
            Assert.That(editor.Exercises.Select(e => e.ExerciseId), Is.EqualTo(new[] { Squat, Treadmill }));
            Assert.That(editor.Exercises[0], Is.SameAs(squat));
            Assert.That(editor.Focus!.Exercise, Is.SameAs(squat));
            Assert.That(editor.Focus.AddSet, Is.True);
            Assert.That(editor.Changed, Is.True);
        });
    }

    [Test]
    public void Pick_CardioAgain_MovesItToTheTopAndFocusesItWithoutAdding()
    {
        var treadmill = SampleData.Exercise(Catalog, Treadmill, ["0:20:00"]);
        editor.Start([treadmill, SampleData.Exercise(Catalog, Squat, ["8", "60"])]);

        editor.Pick(Item(Treadmill));

        Assert.Multiple(() =>
        {
            Assert.That(editor.Exercises.Select(e => e.ExerciseId), Is.EqualTo(new[] { Treadmill, Squat }));
            Assert.That(treadmill.Entries, Has.Count.EqualTo(1));
            Assert.That(editor.Focus!.Exercise, Is.SameAs(treadmill));
            Assert.That(editor.Focus.AddSet, Is.False);
            Assert.That(editor.Changed, Is.True);
        });
    }

    [Test]
    public void Pick_TheTopExerciseAgain_KeepsTheOrderAndIsAChange()
    {
        editor.Start([SampleData.Exercise(Catalog, Squat, ["8", "60"]), SampleData.Exercise(Catalog, Treadmill, ["0:20:00"])]);

        editor.Pick(Item(Treadmill));

        Assert.Multiple(() =>
        {
            Assert.That(editor.Exercises.Select(e => e.ExerciseId), Is.EqualTo(new[] { Treadmill, Squat }));
            Assert.That(editor.Changed, Is.True);
        });
    }

    [Test]
    public void Remove_RemovesOnlyThatExerciseAndIsAChange()
    {
        editor.Start([SampleData.Exercise(Catalog, Squat, ["8", "60"]), SampleData.Exercise(Catalog, Treadmill, ["0:20:00"])]);

        editor.Remove(editor.Exercises[0]);

        Assert.Multiple(() =>
        {
            Assert.That(editor.Exercises.Select(e => e.ExerciseId), Is.EqualTo(new[] { Squat }));
            Assert.That(editor.Changed, Is.True);
        });
    }

    [Test]
    public void MarkChanged_IsAChange()
    {
        editor.MarkChanged();

        Assert.That(editor.Changed, Is.True);
    }

    [Test]
    public async Task Start_SetsTheExercisesAndResetsEverythingElse()
    {
        var squat = SampleData.Exercise(Catalog, Squat, ["8", "60"]);
        editor.Start([squat]);
        await editor.SaveAsync(_ => Task.FromResult(true));
        editor.Pick(Item(Treadmill));
        editor.ShowDeleteFailed();
        editor.OpenPicker();
        editor.Search("run");

        editor.Start([squat]);

        Assert.Multiple(() =>
        {
            Assert.That(editor.Exercises, Is.EqualTo(new[] { squat }));
            Assert.That(editor.PickerOpen, Is.False);
            Assert.That(editor.Query, Is.Empty);
            Assert.That(editor.PickerItems, Is.Empty);
            Assert.That(editor.Focus, Is.Null);
            Assert.That(editor.ErrorFocus, Is.Null);
            Assert.That(editor.Error, Is.Null);
            Assert.That(editor.Changed, Is.False);
            Assert.That(editor.Saving, Is.False);
        });
    }

    [Test]
    public void Start_ShowsTheGivenExercisesNewestFirst()
    {
        editor.Start([SampleData.Exercise(Catalog, Squat, ["8", "60"]), SampleData.Exercise(Catalog, Treadmill, ["0:20:00"])]);

        Assert.That(editor.Exercises.Select(e => e.ExerciseId), Is.EqualTo(new[] { Treadmill, Squat }));
    }

    [Test]
    public void Start_TakesACopySoLaterEditsLeaveTheGivenAndThePreviousListsAlone()
    {
        var given = new List<ExerciseState> { SampleData.Exercise(Catalog, Squat, ["8", "60"]) };
        editor.Start(given);
        editor.Pick(Item(Treadmill));
        var previous = editor.Exercises;

        editor.Start([]);
        editor.Pick(Item(Treadmill));

        Assert.Multiple(() =>
        {
            Assert.That(given, Has.Count.EqualTo(1));
            Assert.That(previous, Has.Count.EqualTo(2));
        });
    }

    [Test]
    public async Task SaveAsync_Invalid_ShowsTheErrorFocusesTheFirstErrorAndDoesNotSave()
    {
        var saves = 0;

        var saved = await editor.SaveAsync(_ => Task.FromResult(++saves > 0));

        Assert.Multiple(() =>
        {
            Assert.That(saved, Is.False);
            Assert.That(saves, Is.Zero);
            Assert.That(editor.Error, Is.EqualTo("Add at least one exercise"));
            Assert.That(editor.ErrorFocus!.Target, Is.EqualTo(ErrorFocusTarget.FirstError));
            Assert.That(editor.Saving, Is.False);
        });
    }

    [Test]
    public async Task SaveAsync_Valid_SavesTheReadRecordsAndKeepsSaving()
    {
        editor.Start([SampleData.Exercise(Catalog, Squat, ["8", "60"]), SampleData.Exercise(Catalog, Treadmill, ["0:20:00"])]);
        IReadOnlyList<WorkoutExercise>? records = null;

        var saved = await editor.SaveAsync(r =>
        {
            records = r;
            return Task.FromResult(true);
        });

        Assert.Multiple(() =>
        {
            Assert.That(saved, Is.True);
            Assert.That(records!.Select(r => r.ExerciseId), Is.EqualTo(new[] { Squat, Treadmill }));
            Assert.That(records![0], Is.InstanceOf<StrengthExercise>());
            Assert.That(records[1], Is.InstanceOf<CardioExercise>());
            Assert.That(editor.Error, Is.Null);
            Assert.That(editor.Saving, Is.True);
        });
    }

    [Test]
    public async Task SaveAsync_AfterAPickAgain_SavesTheRecordsOldestFirst()
    {
        editor.Start([SampleData.Exercise(Catalog, Squat, ["8", "60"]), SampleData.Exercise(Catalog, Treadmill, ["0:20:00"])]);
        editor.Pick(Item(Squat));
        IReadOnlyList<WorkoutExercise>? records = null;

        await editor.SaveAsync(r =>
        {
            records = r;
            return Task.FromResult(true);
        });

        Assert.That(records!.Select(r => r.ExerciseId), Is.EqualTo(new[] { Treadmill, Squat }));
    }

    [Test]
    public async Task SaveAsync_WhileSavingOrAfterASave_IsIgnored()
    {
        editor.Start([SampleData.Exercise(Catalog, Squat, ["8", "60"])]);
        var pending = new TaskCompletionSource<bool>();
        var saves = 0;
        Task<bool> Save(IReadOnlyList<WorkoutExercise> _)
        {
            saves++;
            return pending.Task;
        }

        var first = editor.SaveAsync(Save);
        var whileSaving = await editor.SaveAsync(Save);
        pending.SetResult(true);
        await first;
        var afterSave = await editor.SaveAsync(Save);

        Assert.Multiple(() =>
        {
            Assert.That(whileSaving, Is.False);
            Assert.That(afterSave, Is.False);
            Assert.That(saves, Is.EqualTo(1));
        });
    }

    [Test]
    public async Task SaveAsync_Fails_ShowsSaveFailedFocusesSaveAndCanSaveAgain()
    {
        editor.Start([SampleData.Exercise(Catalog, Squat, ["8", "60"])]);

        var failed = await editor.SaveAsync(_ => Task.FromResult(false));

        Assert.Multiple(() =>
        {
            Assert.That(failed, Is.False);
            Assert.That(editor.Error, Is.EqualTo(workoutChecker.SaveFailed));
            Assert.That(editor.ErrorFocus!.Target, Is.EqualTo(ErrorFocusTarget.Save));
            Assert.That(editor.Saving, Is.False);
        });

        var retried = await editor.SaveAsync(_ => Task.FromResult(true));

        Assert.Multiple(() =>
        {
            Assert.That(retried, Is.True);
            Assert.That(editor.Error, Is.Null);
        });
    }

    [Test]
    public void ShowDeleteFailed_ShowsDeleteFailedAndFocusesDelete()
    {
        editor.ShowDeleteFailed();

        Assert.Multiple(() =>
        {
            Assert.That(editor.Error, Is.EqualTo(workoutChecker.DeleteFailed));
            Assert.That(editor.ErrorFocus!.Target, Is.EqualTo(ErrorFocusTarget.Delete));
        });
    }

    private static PickerItem Item(int exerciseId) => ExerciseFields.PickerItem(Catalog.All.Single(e => e.Id == exerciseId));
}
