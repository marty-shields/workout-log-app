using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Time.Testing;
using WorkoutLog.Core.Domain.Exercises;
using WorkoutLog.Core.Domain.Workouts;
using WorkoutLog.Core.Domain.Exercises.Bests;
using WorkoutLog.Core.Services;
using WorkoutLog.UI.Models.Exercises;
using WorkoutLog.UI.Models.Workout;
using WorkoutLog.UI.Stories.Samples;

namespace WorkoutLog.UI.Tests.Models.Workout;

public class WorkoutEditorTests
{
    private static readonly IExerciseCatalog Catalog = new ExerciseCatalog();

    private const int Squat = 6;
    private const int Treadmill = 300;

    /// <summary>Wed 28 Oct 2026 12:00 UTC.</summary>
    private static readonly DateTimeOffset Now = new(2026, 10, 28, 12, 0, 0, TimeSpan.Zero);

    private WorkoutChecker workoutChecker = null!;
    private FakeLoggedExercisesService loggedExercises = null!;
    private WorkoutEditor editor = null!;

    [SetUp]
    public void CreateEditor()
    {
        var workouts = new SampleWorkoutService(new FakeTimeProvider(), Catalog);
        var setChecker = new SetChecker(workouts, Catalog, NullLogger<SetChecker>.Instance);
        workoutChecker = new WorkoutChecker(setChecker, workouts, NullLogger<WorkoutChecker>.Instance);
        loggedExercises = new FakeLoggedExercisesService();
        editor = new WorkoutEditor(Catalog, setChecker, workoutChecker, loggedExercises, new FakeTimeProvider(Now));
    }

    [Test]
    public void Constructor_IsEmptyClosedUnchangedAndNotSaving()
    {
        Assert.Multiple(() =>
        {
            Assert.That(editor.Exercises, Is.Empty);
            Assert.That(editor.ListOpen, Is.False);
            Assert.That(editor.Changed, Is.False);
            Assert.That(editor.Saving, Is.False);
        });
    }

    [Test]
    public async Task OpenListAsync_ListsTheWholeCatalogStrengthThenCardioWithNoSearchAndAll()
    {
        await editor.OpenListAsync();
        editor.Search("squat");
        editor.FilterBy(ExerciseFilter.Cardio);

        await editor.OpenListAsync();

        Assert.Multiple(() =>
        {
            Assert.That(editor.ListOpen, Is.True);
            Assert.That(editor.ListLoading, Is.False);
            Assert.That(editor.Query, Is.Empty);
            Assert.That(editor.Filter, Is.EqualTo(ExerciseFilter.All));
            Assert.That(editor.ListItems.Select(i => i.Id), Is.EqualTo(Catalog.Search(null).Select(e => e.Id)));
            Assert.That(editor.ListItems.Select(i => i.Last), Is.All.Null);
        });
    }

    [Test]
    public async Task OpenListAsync_LoggedExercisesShowTheirLastSession()
    {
        loggedExercises.Result = new LoggedExercisesResult([new LoggedExercise(Squat, Now.AddDays(-8), new ExerciseBest(70m, 3, null, null))], Failed: false);

        await editor.OpenListAsync();

        Assert.That(editor.ListItems.Single(i => i.Id == Squat).Last, Is.EqualTo("Tue 20 Oct · 70kg × 3"));
    }

    [Test]
    public async Task OpenListAsync_LoadFailed_ListsTheCatalogWithNoLastSessions()
    {
        loggedExercises.Result = new LoggedExercisesResult([], Failed: true);

        await editor.OpenListAsync();

        Assert.Multiple(() =>
        {
            Assert.That(editor.ListLoading, Is.False);
            Assert.That(editor.ListItems, Has.Count.EqualTo(Catalog.All.Count));
            Assert.That(editor.ListItems.Select(i => i.Last), Is.All.Null);
        });
    }

    [Test]
    public async Task OpenListAsync_WhileLoading_IsLoadingWithNoRows()
    {
        loggedExercises.Hold = true;

        var open = editor.OpenListAsync();

        Assert.Multiple(() =>
        {
            Assert.That(editor.ListOpen, Is.True);
            Assert.That(editor.ListLoading, Is.True);
            Assert.That(editor.ListItems, Is.Empty);
        });
        loggedExercises.Complete();
        await open;
        Assert.That(editor.ListLoading, Is.False);
    }

    [Test]
    public async Task OpenListAsync_Reopened_ClearsTheEarlierRowsWhileLoading()
    {
        await editor.OpenListAsync();
        Assume.That(editor.ListItems, Is.Not.Empty);
        editor.CloseList();
        loggedExercises.Hold = true;

        var open = editor.OpenListAsync();

        Assert.Multiple(() =>
        {
            Assert.That(editor.ListLoading, Is.True);
            Assert.That(editor.ListItems, Is.Empty);
        });
        loggedExercises.Complete();
        await open;
    }

    [Test]
    public async Task OpenListAsync_ClosedBeforeTheLoadFinishes_StaysClosed()
    {
        loggedExercises.Hold = true;
        var open = editor.OpenListAsync();
        editor.CloseList();

        loggedExercises.Complete();
        await open;

        Assert.That(editor.ListOpen, Is.False);
    }

    [Test]
    public async Task OpenListAsync_AnEarlierLoadFinishingWhileANewerIsPending_IsIgnored()
    {
        loggedExercises.Hold = true;
        var first = editor.OpenListAsync();
        editor.CloseList();
        var second = editor.OpenListAsync();

        loggedExercises.Complete(0);
        await first;

        Assert.That(editor.ListLoading, Is.True);
        loggedExercises.Complete(1);
        await second;
        Assert.That(editor.ListLoading, Is.False);
    }

    [Test]
    public async Task Search_SetsTheQueryAndTheRankedResults()
    {
        await editor.OpenListAsync();

        editor.Search("squat");

        Assert.Multiple(() =>
        {
            Assert.That(editor.Query, Is.EqualTo("squat"));
            Assert.That(editor.ListItems.Select(i => i.Id), Is.EqualTo(Catalog.Search("squat").Select(e => e.Id)));
        });
    }

    [Test]
    public async Task FilterBy_KeepsOnlyThatKindWithAndWithoutSearch()
    {
        await editor.OpenListAsync();

        editor.FilterBy(ExerciseFilter.Cardio);
        var cardio = editor.ListItems;
        editor.Search("row");

        Assert.Multiple(() =>
        {
            Assert.That(cardio.Select(i => i.Id), Is.EqualTo(Catalog.Search(null).Where(e => e.Type == ExerciseType.Cardio).Select(e => e.Id)));
            Assert.That(editor.ListItems.Select(i => i.Id),
                Is.EqualTo(Catalog.Search("row").Where(e => e.Type == ExerciseType.Cardio).Select(e => e.Id)).And.Not.Empty);
        });
    }

    [Test]
    public async Task CloseList_ClosesIt()
    {
        await editor.OpenListAsync();

        editor.CloseList();

        Assert.That(editor.ListOpen, Is.False);
    }

    [Test]
    public async Task Pick_New_AddsItAtTheTopFocusesItClosesTheListAndClearsTheError()
    {
        await editor.SaveAsync(_ => Task.FromResult(true));
        Assume.That(editor.Error, Is.Not.Null);
        editor.Pick(Item(Treadmill));
        await editor.OpenListAsync();

        editor.Pick(Item(Squat));

        var squat = editor.Exercises[0];
        Assert.Multiple(() =>
        {
            Assert.That(editor.Exercises.Select(e => e.ExerciseId), Is.EqualTo(new[] { Squat, Treadmill }));
            Assert.That(squat.Entries, Has.Count.EqualTo(1));
            Assert.That(editor.Focus!.Exercise, Is.SameAs(squat));
            Assert.That(editor.Focus.AddSet, Is.False);
            Assert.That(editor.ListOpen, Is.False);
            Assert.That(editor.Error, Is.Null);
            Assert.That(editor.Changed, Is.True);
        });
    }

    [Test]
    public void Pick_StrengthAgain_MovesItToTheTopFocusesItAndAsksForASet()
    {
        var squat = SampleData.Exercise(Catalog, Squat, ["8", "60"]);
        editor.Start([squat, SampleData.Exercise(Catalog, Treadmill, ["0:20:00", "3"])]);

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
    public void Pick_StrengthAgainWithTenSets_MovesItToTheTopAndFocusesItWithoutAskingForASet()
    {
        var squat = SampleData.FullSets(Catalog, Squat);
        editor.Start([squat, SampleData.Exercise(Catalog, Treadmill, ["0:20:00", "3"])]);

        editor.Pick(Item(Squat));

        Assert.Multiple(() =>
        {
            Assert.That(editor.Exercises[0], Is.SameAs(squat));
            Assert.That(editor.Focus!.Exercise, Is.SameAs(squat));
            Assert.That(editor.Focus.AddSet, Is.False);
            Assert.That(squat.Entries, Has.Count.EqualTo(10));
        });
    }

    [Test]
    public void Pick_NewWithTenExercises_AddsNothingAndClosesTheList()
    {
        editor.Start(SampleData.FullWorkout(Catalog));
        var before = editor.Exercises.ToList();

        editor.Pick(Item(Treadmill));

        Assert.Multiple(() =>
        {
            Assert.That(editor.Exercises, Is.EqualTo(before));
            Assert.That(editor.Focus, Is.Null);
            Assert.That(editor.ListOpen, Is.False);
            Assert.That(editor.Changed, Is.False);
        });
    }

    [Test]
    public void Pick_AgainWithTenExercises_MovesItToTheTop()
    {
        editor.Start(SampleData.FullWorkout(Catalog));

        editor.Pick(Item(Squat));

        Assert.That(editor.Exercises[0].ExerciseId, Is.EqualTo(Squat));
    }

    [Test]
    public void Pick_CardioAgain_MovesItToTheTopAndFocusesItWithoutAdding()
    {
        var treadmill = SampleData.Exercise(Catalog, Treadmill, ["0:20:00", "3"]);
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
        editor.Start([SampleData.Exercise(Catalog, Squat, ["8", "60"]), SampleData.Exercise(Catalog, Treadmill, ["0:20:00", "3"])]);

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
        editor.Start([SampleData.Exercise(Catalog, Squat, ["8", "60"]), SampleData.Exercise(Catalog, Treadmill, ["0:20:00", "3"])]);

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
        await editor.OpenListAsync();
        editor.Search("run");

        editor.Start([squat]);

        Assert.Multiple(() =>
        {
            Assert.That(editor.Exercises, Is.EqualTo(new[] { squat }));
            Assert.That(editor.ListOpen, Is.False);
            Assert.That(editor.Query, Is.Empty);
            Assert.That(editor.Filter, Is.EqualTo(ExerciseFilter.All));
            Assert.That(editor.ListLoading, Is.False);
            Assert.That(editor.ListItems, Is.Empty);
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
        editor.Start([SampleData.Exercise(Catalog, Squat, ["8", "60"]), SampleData.Exercise(Catalog, Treadmill, ["0:20:00", "3"])]);

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
        editor.Start([SampleData.Exercise(Catalog, Squat, ["8", "60"]), SampleData.Exercise(Catalog, Treadmill, ["0:20:00", "3"])]);
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
        editor.Start([SampleData.Exercise(Catalog, Squat, ["8", "60"]), SampleData.Exercise(Catalog, Treadmill, ["0:20:00", "3"])]);
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

    private static ExerciseListItem Item(int exerciseId) =>
        ExerciseListItems.Item(Catalog.All.Single(e => e.Id == exerciseId), null, TimeZoneInfo.Utc, Now);
}
