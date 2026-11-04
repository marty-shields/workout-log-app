using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Time.Testing;
using WorkoutLog.Core.Domain.Exercises;
using WorkoutLog.Core.Domain.Exercises.Bests;
using WorkoutLog.Core.Domain.Workouts;
using WorkoutLog.Core.Services;
using WorkoutLog.UI.Haptics;
using WorkoutLog.UI.Models.Exercises;
using WorkoutLog.UI.Models.Workout;
using WorkoutLog.UI.Stories.Samples;

namespace WorkoutLog.UI.Tests.Models.Workout;

public class WorkoutEditorTests
{
    private static readonly IExerciseCatalog Catalog = new ExerciseCatalog();

    private const int Squat = 6;
    private const int Treadmill = 300;
    private const int Curl = 132;

    /// <summary>Wed 28 Oct 2026 12:00 UTC.</summary>
    private static readonly DateTimeOffset Now = new(2026, 10, 28, 12, 0, 0, TimeSpan.Zero);

    private WorkoutChecker workoutChecker = null!;
    private FakeLoggedExercisesService loggedExercises = null!;
    private FakeHaptics haptics = null!;
    private WorkoutEditor editor = null!;

    [SetUp]
    public void CreateEditor()
    {
        var workouts = new SampleWorkoutService(new FakeTimeProvider(), Catalog);
        var setChecker = new SetChecker(workouts, Catalog, NullLogger<SetChecker>.Instance);
        workoutChecker = new WorkoutChecker(setChecker, workouts, NullLogger<WorkoutChecker>.Instance);
        loggedExercises = new FakeLoggedExercisesService();
        haptics = new FakeHaptics();
        editor = new WorkoutEditor(Catalog, setChecker, workoutChecker, loggedExercises, new FakeTimeProvider(Now), haptics);
    }

    [Test]
    public void Constructor_IsEmptyClosedUnchangedAndNotSaving()
    {
        Assert.Multiple(() =>
        {
            Assert.That(editor.Exercises, Is.Empty);
            Assert.That(editor.ListOpen, Is.False);
            Assert.That(editor.Changed, Is.False);
            Assert.That(editor.SaveState, Is.EqualTo(SavePhase.Idle));
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
        await editor.SaveAsync((_, _) => Task.FromResult(true));
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
            Assert.That(editor.Focus.Target, Is.EqualTo(ExerciseFocusTarget.OpenSet));
            Assert.That(editor.ListOpen, Is.False);
            Assert.That(editor.Error, Is.Null);
            Assert.That(editor.Changed, Is.True);
        });
    }

    [Test]
    public void Pick_StrengthAgain_MovesItToTheTopFocusesItAndAsksForASet()
    {
        var squat = SampleData.Exercise(Catalog, Squat, ["60", "8"]);
        editor.Start([squat, SampleData.Exercise(Catalog, Treadmill, ["0:20:00", "3"])], null);

        editor.Pick(Item(Squat));

        Assert.Multiple(() =>
        {
            Assert.That(editor.Exercises.Select(e => e.ExerciseId), Is.EqualTo(new[] { Squat, Treadmill }));
            Assert.That(editor.Exercises[0], Is.SameAs(squat));
            Assert.That(editor.Focus!.Exercise, Is.SameAs(squat));
            Assert.That(editor.Focus.Target, Is.EqualTo(ExerciseFocusTarget.AddedSet));
            Assert.That(editor.Changed, Is.True);
        });
    }

    [Test]
    public void Pick_StrengthAgainWithTenSets_MovesItToTheTopAndFocusesItWithoutAskingForASet()
    {
        var squat = SampleData.FullSets(Catalog, Squat);
        editor.Start([squat, SampleData.Exercise(Catalog, Treadmill, ["0:20:00", "3"])], null);

        editor.Pick(Item(Squat));

        Assert.Multiple(() =>
        {
            Assert.That(editor.Exercises[0], Is.SameAs(squat));
            Assert.That(editor.Focus!.Exercise, Is.SameAs(squat));
            Assert.That(editor.Focus.Target, Is.EqualTo(ExerciseFocusTarget.OpenSet));
            Assert.That(squat.Entries, Has.Count.EqualTo(10));
        });
    }

    [Test]
    public void Pick_NewWithTenExercises_AddsNothingAndClosesTheList()
    {
        editor.Start(SampleData.FullWorkout(Catalog), null);
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
        editor.Start(SampleData.FullWorkout(Catalog), null);

        editor.Pick(Item(Squat));

        Assert.That(editor.Exercises[0].ExerciseId, Is.EqualTo(Squat));
    }

    [Test]
    public void Pick_CardioAgain_MovesItToTheTopAndFocusesItWithoutAdding()
    {
        var treadmill = SampleData.Exercise(Catalog, Treadmill, ["0:20:00", "3"]);
        editor.Start([treadmill, SampleData.Exercise(Catalog, Squat, ["60", "8"])], null);

        editor.Pick(Item(Treadmill));

        Assert.Multiple(() =>
        {
            Assert.That(editor.Exercises.Select(e => e.ExerciseId), Is.EqualTo(new[] { Treadmill, Squat }));
            Assert.That(treadmill.Entries, Has.Count.EqualTo(1));
            Assert.That(editor.Focus!.Exercise, Is.SameAs(treadmill));
            Assert.That(editor.Focus.Target, Is.EqualTo(ExerciseFocusTarget.OpenSet));
            Assert.That(editor.Changed, Is.True);
        });
    }

    [Test]
    public void Pick_TheTopExerciseAgain_KeepsTheOrderAndIsAChange()
    {
        editor.Start([SampleData.Exercise(Catalog, Squat, ["60", "8"]), SampleData.Exercise(Catalog, Treadmill, ["0:20:00", "3"])], null);

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
        editor.Start([SampleData.Exercise(Catalog, Squat, ["60", "8"]), SampleData.Exercise(Catalog, Treadmill, ["0:20:00", "3"])], null);

        editor.Remove(editor.Exercises[0]);

        Assert.Multiple(() =>
        {
            Assert.That(editor.Exercises.Select(e => e.ExerciseId), Is.EqualTo(new[] { Squat }));
            Assert.That(editor.Changed, Is.True);
        });
    }

    [Test]
    public void Pick_ADoneExerciseAgain_OpensItAndMovesItToTheTop()
    {
        var squat = SampleData.Exercise(Catalog, Squat, ["8", "60"]);
        editor.Start([squat, SampleData.Exercise(Catalog, Treadmill, ["0:20:00", "3"])], null);
        squat.IsDone = true;

        editor.Pick(Item(Squat));

        Assert.Multiple(() =>
        {
            Assert.That(editor.Exercises[0], Is.SameAs(squat));
            Assert.That(squat.IsDone, Is.False);
            Assert.That(squat.Entries, Has.Count.EqualTo(1));
            Assert.That(editor.Focus!.Target, Is.EqualTo(ExerciseFocusTarget.AddedSet));
        });
    }

    [Test]
    public void Remove_OffersUndoNamedForTheExercise()
    {
        editor.Start([SampleData.Exercise(Catalog, Squat, ["8", "60"])], null);
        var squat = editor.Exercises[0];

        editor.Remove(squat);

        Assert.That(editor.PendingUndo, Is.EqualTo(new RemovedExercise(squat, 0)));
        Assert.That(editor.PendingUndo!.Message, Is.EqualTo("Barbell Back Squat deleted"));
    }

    [Test]
    public void Remove_AnExerciseNotInTheWorkout_ChangesNothing()
    {
        var gone = SampleData.Exercise(Catalog, Squat, ["8", "60"]);
        editor.Start([SampleData.Exercise(Catalog, Treadmill, ["0:20:00", "3"])], null);

        editor.Remove(gone);

        Assert.Multiple(() =>
        {
            Assert.That(editor.Exercises, Has.Count.EqualTo(1));
            Assert.That(editor.PendingUndo, Is.Null);
            Assert.That(editor.Changed, Is.False);
        });
    }

    [Test]
    public void Undo_PutsTheExerciseBackInItsPlaceDoneAsBefore()
    {
        var squat = SampleData.Exercise(Catalog, Squat, ["8", "60"]);
        var treadmill = SampleData.Exercise(Catalog, Treadmill, ["0:20:00", "3"]);
        var curl = SampleData.Exercise(Catalog, Curl, ["10", "12"]);
        editor.Start([squat, treadmill, curl], null);
        treadmill.IsDone = true;
        editor.Remove(treadmill);

        editor.Undo();

        Assert.Multiple(() =>
        {
            Assert.That(editor.Exercises, Is.EqualTo(new[] { curl, treadmill, squat }));
            Assert.That(treadmill.IsDone, Is.True);
            Assert.That((editor.Focus!.Exercise, editor.Focus.Target, editor.Focus.Set), Is.EqualTo((treadmill, ExerciseFocusTarget.Restored, (EntryState?)null)));
            Assert.That(editor.PendingUndo, Is.Null);
            Assert.That(editor.Changed, Is.True);
        });
    }

    [Test]
    public void RemoveSet_TakesTheSetOutAtOnceAndOffersUndoNamedForIt()
    {
        var squat = SampleData.Exercise(Catalog, Squat, ["8", "60"]);
        var second = new EntryState();
        squat.Entries.Add(second);
        editor.Start([squat], null);

        editor.RemoveSet(new SetRef(squat, second));

        Assert.Multiple(() =>
        {
            Assert.That(squat.Entries, Has.Count.EqualTo(1));
            Assert.That(editor.PendingUndo, Is.TypeOf<RemovedSet>().With.Property(nameof(RemovedSet.Entry)).SameAs(second));
            Assert.That(editor.PendingUndo!.Message, Is.EqualTo("Barbell Back Squat set 2 deleted"));
            Assert.That(editor.Changed, Is.True);
        });
    }

    [Test]
    public void RemoveSet_TheOnlySet_LeavesTheExerciseWithNoSets()
    {
        var squat = SampleData.Exercise(Catalog, Squat, ["8", "60"]);
        editor.Start([squat], null);

        editor.RemoveSet(new SetRef(squat, squat.Entries[0]));

        Assert.Multiple(() =>
        {
            Assert.That(squat.Entries, Is.Empty);
            Assert.That(editor.Exercises, Has.Count.EqualTo(1));
        });
    }

    [Test]
    public void RemoveSet_ASetAlreadyRemoved_ChangesNothing()
    {
        var squat = SampleData.Exercise(Catalog, Squat, ["8", "60"]);
        var gone = new EntryState();
        editor.Start([squat], null);

        editor.RemoveSet(new SetRef(squat, gone));

        Assert.Multiple(() =>
        {
            Assert.That(squat.Entries, Has.Count.EqualTo(1));
            Assert.That(editor.PendingUndo, Is.Null);
            Assert.That(editor.Changed, Is.False);
        });
    }

    [Test]
    public void Undo_PutsTheSetBackInItsPlaceWithItsValues()
    {
        var squat = SampleData.Exercise(Catalog, Squat, ["8", "60"]);
        var first = squat.Entries[0];
        var third = new EntryState();
        squat.Entries.Add(third);
        squat.Entries.Insert(0, new EntryState());
        editor.Start([squat], null);
        editor.RemoveSet(new SetRef(squat, first));

        editor.Undo();

        Assert.Multiple(() =>
        {
            Assert.That(squat.Entries[1], Is.SameAs(first));
            Assert.That((editor.Focus!.Exercise, editor.Focus.Target, editor.Focus.Set), Is.EqualTo((squat, ExerciseFocusTarget.Restored, first)));
            Assert.That(squat.Entries, Has.Count.EqualTo(3));
            Assert.That(first.Values.Values, Is.EqualTo(new[] { "8", "60" }));
            Assert.That(editor.PendingUndo, Is.Null);
        });
    }

    [Test]
    public void Undo_FewerSetsLeft_PutsTheSetAtTheEnd()
    {
        var squat = SampleData.Exercise(Catalog, Squat, ["8", "60"]);
        squat.Entries.Add(new EntryState());
        var last = squat.Entries[1];
        editor.Start([squat], null);
        editor.RemoveSet(new SetRef(squat, last));
        squat.Entries.RemoveAt(0);

        editor.Undo();

        Assert.That(squat.Entries, Is.EqualTo(new[] { last }));
    }

    [Test]
    public void Undo_ASetAboveRemovedMeanwhile_PutsTheSetBackUnderItsNeighbour()
    {
        var squat = SampleData.Exercise(Catalog, Squat, ["8", "60"]);
        squat.Entries.AddRange([new EntryState(), new EntryState(), new EntryState()]);
        var (a, b, c, d) = (squat.Entries[0], squat.Entries[1], squat.Entries[2], squat.Entries[3]);
        editor.Start([squat], null);
        editor.RemoveSet(new SetRef(squat, c));
        squat.Entries.Remove(a);
        editor.MarkChanged();

        editor.Undo();

        Assert.That(squat.Entries, Is.EqualTo(new[] { b, c, d }));
    }

    [Test]
    public void Undo_ASetAddedMeanwhile_PutsTheSetBackInItsPlace()
    {
        var squat = SampleData.Exercise(Catalog, Squat, ["8", "60"]);
        squat.Entries.Add(new EntryState());
        var (a, b) = (squat.Entries[0], squat.Entries[1]);
        editor.Start([squat], null);
        editor.RemoveSet(new SetRef(squat, a));
        var added = new EntryState();
        squat.Entries.Add(added);
        editor.MarkChanged();

        editor.Undo();

        Assert.That(squat.Entries, Is.EqualTo(new[] { a, b, added }));
    }

    [Test]
    public void RemoveSet_AfterAnotherDeletion_KeepsTheEarlierOneDeletedAndOffersOnlyTheLatest()
    {
        var squat = SampleData.Exercise(Catalog, Squat, ["8", "60"]);
        var treadmill = SampleData.Exercise(Catalog, Treadmill, ["0:20:00", "3"]);
        squat.Entries.Add(new EntryState());
        editor.Start([squat, treadmill], null);
        editor.Remove(treadmill);
        var second = squat.Entries[1];

        editor.RemoveSet(new SetRef(squat, second));
        editor.Undo();

        Assert.Multiple(() =>
        {
            Assert.That(editor.Exercises, Is.EqualTo(new[] { squat }));
            Assert.That(squat.Entries, Has.Count.EqualTo(2));
        });
    }

    [Test]
    public void Undo_NothingPending_ChangesNothing()
    {
        editor.Start([SampleData.Exercise(Catalog, Squat, ["8", "60"])], null);

        editor.Undo();

        Assert.Multiple(() =>
        {
            Assert.That(editor.Exercises, Has.Count.EqualTo(1));
            Assert.That(editor.Changed, Is.False);
        });
    }

    [Test]
    public void DismissUndo_KeepsTheDeletionAndEndsTheUndo()
    {
        editor.Start([SampleData.Exercise(Catalog, Squat, ["8", "60"])], null);
        editor.Remove(editor.Exercises[0]);

        editor.DismissUndo(editor.PendingUndo!);
        editor.Undo();

        Assert.Multiple(() =>
        {
            Assert.That(editor.PendingUndo, Is.Null);
            Assert.That(editor.Exercises, Is.Empty);
        });
    }

    [Test]
    public void DismissUndo_ALaterDeletionReplacedIt_KeepsTheLaterUndo()
    {
        editor.Start([SampleData.Exercise(Catalog, Squat, ["8", "60"]), SampleData.Exercise(Catalog, Treadmill, ["0:20:00", "3"])], null);
        editor.Remove(editor.Exercises[0]);
        var earlier = editor.PendingUndo!;
        editor.Remove(editor.Exercises[0]);
        var later = editor.PendingUndo;

        editor.DismissUndo(earlier);

        Assert.That(editor.PendingUndo, Is.SameAs(later));
    }

    [Test]
    public async Task OpenListAsyncThenCloseList_KeepsTheUndo()
    {
        editor.Start([SampleData.Exercise(Catalog, Squat, ["8", "60"])], null);
        editor.Remove(editor.Exercises[0]);
        var undo = editor.PendingUndo;

        await editor.OpenListAsync();
        editor.CloseList();
        editor.Undo();

        Assert.Multiple(() =>
        {
            Assert.That(undo, Is.Not.Null);
            Assert.That(editor.Exercises, Has.Count.EqualTo(1));
        });
    }

    [Test]
    public void MarkChanged_ASetAddedFillsTheExerciseWhoseSetUndoWouldPutBack_EndsTheUndo()
    {
        var squat = SampleData.FullSets(Catalog, Squat);
        editor.Start([squat], null);
        editor.RemoveSet(new SetRef(squat, squat.Entries[0]));
        squat.Entries.Add(new EntryState());

        editor.MarkChanged();
        editor.Undo();

        Assert.Multiple(() =>
        {
            Assert.That(editor.PendingUndo, Is.Null);
            Assert.That(squat.Entries, Has.Count.EqualTo(WorkoutLimits.MaxSets));
        });
    }

    [Test]
    public void MarkChanged_ASetAddedToAnotherExercise_KeepsTheUndo()
    {
        var squat = SampleData.FullSets(Catalog, Squat);
        var curl = SampleData.Exercise(Catalog, Curl, ["10", "12"]);
        editor.Start([squat, curl], null);
        editor.RemoveSet(new SetRef(squat, squat.Entries[0]));
        curl.Entries.Add(new EntryState());

        editor.MarkChanged();

        Assert.That(editor.PendingUndo, Is.Not.Null);
    }

    [Test]
    public void Undo_TheExerciseFilledUpWithoutAChangeReported_LeavesItAtMaxSets()
    {
        var squat = SampleData.FullSets(Catalog, Squat);
        editor.Start([squat], null);
        editor.RemoveSet(new SetRef(squat, squat.Entries[0]));
        squat.Entries.Add(new EntryState());

        editor.Undo();

        Assert.Multiple(() =>
        {
            Assert.That(editor.PendingUndo, Is.Null);
            Assert.That(squat.Entries, Has.Count.EqualTo(WorkoutLimits.MaxSets));
        });
    }

    [Test]
    public void Pick_ANewExerciseFillsTheWorkoutWhoseExerciseUndoWouldPutBack_EndsTheUndo()
    {
        editor.Start(Catalog.All.Where(e => e.Id != Curl).Take(WorkoutLimits.MaxExercises).Select(ExerciseFields.New), null);
        editor.Remove(editor.Exercises[0]);

        editor.Pick(Item(Curl));
        editor.Undo();

        Assert.Multiple(() =>
        {
            Assert.That(editor.PendingUndo, Is.Null);
            Assert.That(editor.Exercises, Has.Count.EqualTo(WorkoutLimits.MaxExercises));
            Assert.That(editor.Exercises[0].ExerciseId, Is.EqualTo(Curl));
        });
    }

    [Test]
    public void Pick_TheDeletedExerciseAgain_EndsTheUndo()
    {
        var squat = SampleData.Exercise(Catalog, Squat, ["8", "60"]);
        var treadmill = SampleData.Exercise(Catalog, Treadmill, ["0:20:00", "3"]);
        editor.Start([squat, treadmill], null);
        editor.Remove(squat);

        editor.Pick(Item(Squat));
        editor.Undo();

        Assert.Multiple(() =>
        {
            Assert.That(editor.PendingUndo, Is.Null);
            Assert.That(editor.Exercises.Select(e => e.ExerciseId), Is.EqualTo(new[] { Squat, Treadmill }));
        });
    }

    [Test]
    public async Task SaveAsync_Saved_EndsTheUndo()
    {
        editor.Start([SampleData.Exercise(Catalog, Squat, ["8", "60"]), SampleData.Exercise(Catalog, Treadmill, ["0:20:00", "3"])], null);
        editor.Remove(editor.Exercises[0]);

        await editor.SaveAsync((_, _) => Task.FromResult(true));

        Assert.That(editor.PendingUndo, Is.Null);
    }

    [Test]
    public async Task SaveAsync_Fails_KeepsTheUndo()
    {
        var squat = SampleData.Exercise(Catalog, Squat, ["8", "60"]);
        var treadmill = SampleData.Exercise(Catalog, Treadmill, ["0:20:00", "3"]);
        editor.Start([squat, treadmill], null);
        editor.Remove(treadmill);

        await editor.SaveAsync((_, _) => Task.FromResult(false));
        editor.Undo();

        Assert.That(editor.Exercises, Is.EqualTo(new[] { treadmill, squat }));
    }

    [Test]
    public async Task Undo_WhileSaving_IsIgnored()
    {
        var squat = SampleData.Exercise(Catalog, Squat, ["8", "60"]);
        var treadmill = SampleData.Exercise(Catalog, Treadmill, ["0:20:00", "3"]);
        editor.Start([squat, treadmill], null);
        editor.Remove(treadmill);
        var saving = new TaskCompletionSource<bool>();
        var save = editor.SaveAsync((_, _) => saving.Task);

        editor.Undo();
        saving.SetResult(false);
        await save;

        Assert.Multiple(() =>
        {
            Assert.That(editor.Exercises, Is.EqualTo(new[] { squat }));
            Assert.That(editor.PendingUndo, Is.Not.Null);
        });
    }

    [Test]
    public async Task SaveAsync_Invalid_KeepsTheUndo()
    {
        editor.Start([SampleData.Exercise(Catalog, Squat, ["8", "60"])], null);
        editor.Remove(editor.Exercises[0]);

        await editor.SaveAsync((_, _) => Task.FromResult(true));

        Assert.That(editor.PendingUndo, Is.Not.Null);
    }

    [Test]
    public void Start_EndsTheUndo()
    {
        editor.Start([SampleData.Exercise(Catalog, Squat, ["8", "60"])], null);
        editor.Remove(editor.Exercises[0]);

        editor.Start([], null);

        Assert.That(editor.PendingUndo, Is.Null);
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
        var squat = SampleData.Exercise(Catalog, Squat, ["60", "8"]);
        editor.Start([squat], null);
        await editor.SaveAsync((_, _) => Task.FromResult(true));
        editor.Pick(Item(Treadmill));
        editor.ShowDeleteFailed();
        await editor.OpenListAsync();
        editor.Search("run");

        editor.Start([squat], null);

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
            Assert.That(editor.SaveState, Is.EqualTo(SavePhase.Idle));
        });
    }

    [Test]
    public void Start_ShowsTheGivenExercisesNewestFirst()
    {
        editor.Start([SampleData.Exercise(Catalog, Squat, ["60", "8"]), SampleData.Exercise(Catalog, Treadmill, ["0:20:00", "3"])], null);

        Assert.That(editor.Exercises.Select(e => e.ExerciseId), Is.EqualTo(new[] { Treadmill, Squat }));
    }

    [Test]
    public void Start_TakesACopySoLaterEditsLeaveTheGivenAndThePreviousListsAlone()
    {
        var given = new List<ExerciseState> { SampleData.Exercise(Catalog, Squat, ["60", "8"]) };
        editor.Start(given, null);
        editor.Pick(Item(Treadmill));
        var previous = editor.Exercises;

        editor.Start([], null);
        editor.Pick(Item(Treadmill));

        Assert.Multiple(() =>
        {
            Assert.That(given, Has.Count.EqualTo(1));
            Assert.That(previous, Has.Count.EqualTo(2));
        });
    }

    [Test]
    public void Start_TakesTheNoteAndClearsTheNoteErrorAndChanged()
    {
        editor.Start([], "Slept badly");
        editor.SetNote(SampleData.NoteTooLong);
        Assume.That(editor.Changed, Is.True);

        editor.Start([], "Felt strong");

        Assert.Multiple(() =>
        {
            Assert.That(editor.Note, Is.EqualTo("Felt strong"));
            Assert.That(editor.NoteError, Is.Null);
            Assert.That(editor.Changed, Is.False);
        });
    }

    [Test]
    public void SetNote_SetsTheNoteAndIsAChange()
    {
        editor.Start([], null);

        editor.SetNote("Felt strong");

        Assert.Multiple(() =>
        {
            Assert.That(editor.Note, Is.EqualTo("Felt strong"));
            Assert.That(editor.Changed, Is.True);
        });
    }

    [Test]
    public async Task SetNote_ClearsTheNoteErrorOfAFailedSave()
    {
        editor.Start([SampleData.Exercise(Catalog, Squat, ["60", "8"])], SampleData.NoteTooLong);
        await editor.SaveAsync((_, _) => Task.FromResult(true));
        Assume.That(editor.NoteError, Is.Not.Null);

        editor.SetNote("Short");

        Assert.That(editor.NoteError, Is.Null);
    }

    [Test]
    public async Task SaveAsync_PassesTheWorkoutNoteAndTheExerciseNotes()
    {
        var squat = SampleData.Exercise(Catalog, Squat, ["60", "8"]);
        squat.Note = "Belt on";
        var treadmill = SampleData.Exercise(Catalog, Treadmill, ["0:20:00", "3"]);
        treadmill.Note = "Windy";
        editor.Start([squat, treadmill], "Felt strong");
        string? note = null;
        IReadOnlyList<WorkoutExercise>? records = null;

        await editor.SaveAsync((n, r) =>
        {
            note = n;
            records = r;
            return Task.FromResult(true);
        });

        Assert.Multiple(() =>
        {
            Assert.That(note, Is.EqualTo("Felt strong"));
            Assert.That(records!.Select(r => r.Note), Is.EqualTo(new[] { "Belt on", "Windy" }));
        });
    }

    [Test]
    public async Task SaveAsync_NoteTooLong_ShowsTheNoteErrorFocusesItAndDoesNotSave()
    {
        editor.Start([SampleData.Exercise(Catalog, Squat, ["60", "8"])], SampleData.NoteTooLong);
        var saves = 0;

        var saved = await editor.SaveAsync((_, _) => Task.FromResult(++saves > 0));

        Assert.Multiple(() =>
        {
            Assert.That(saved, Is.False);
            Assert.That(saves, Is.Zero);
            Assert.That(editor.NoteError, Is.EqualTo(WorkoutChecker.NoteTooLong));
            Assert.That(editor.Error, Is.Null);
            Assert.That(editor.ErrorFocus!.Target, Is.EqualTo(ErrorFocusTarget.FirstError));
        });
    }

    [Test]
    public async Task SaveAsync_ExerciseNoteTooLong_ReopensTheDoneExerciseAndDoesNotSave()
    {
        var squat = SampleData.Exercise(Catalog, Squat, ["60", "8"]);
        squat.Note = SampleData.NoteTooLong;
        squat.IsDone = true;
        editor.Start([squat], null);

        var saved = await editor.SaveAsync((_, _) => Task.FromResult(true));

        Assert.Multiple(() =>
        {
            Assert.That(saved, Is.False);
            Assert.That(squat.NoteError, Is.EqualTo(WorkoutChecker.NoteTooLong));
            Assert.That(squat.IsDone, Is.False);
        });
    }

    [Test]
    public async Task SaveAsync_Fails_KeepsTheNotesAndSavesThemOnRetry()
    {
        var squat = SampleData.Exercise(Catalog, Squat, ["60", "8"]);
        squat.Note = "Belt on";
        editor.Start([squat], "Felt strong");
        await editor.SaveAsync((_, _) => Task.FromResult(false));
        string? note = null;
        IReadOnlyList<WorkoutExercise>? records = null;

        await editor.SaveAsync((n, r) =>
        {
            note = n;
            records = r;
            return Task.FromResult(true);
        });

        Assert.Multiple(() =>
        {
            Assert.That(editor.Note, Is.EqualTo("Felt strong"));
            Assert.That(note, Is.EqualTo("Felt strong"));
            Assert.That(records!.Single().Note, Is.EqualTo("Belt on"));
        });
    }

    [Test]
    public async Task SaveAsync_Invalid_ShowsTheErrorFocusesTheFirstErrorAndDoesNotSave()
    {
        var saves = 0;

        var saved = await editor.SaveAsync((_, _) => Task.FromResult(++saves > 0));

        Assert.Multiple(() =>
        {
            Assert.That(saved, Is.False);
            Assert.That(saves, Is.Zero);
            Assert.That(editor.Error, Is.EqualTo("Add at least one exercise"));
            Assert.That(editor.ErrorFocus!.Target, Is.EqualTo(ErrorFocusTarget.FirstError));
            Assert.That(editor.SaveState, Is.EqualTo(SavePhase.Idle));
        });
    }

    [Test]
    public async Task SaveAsync_Saved_PlaysWorkoutSavedOnceAndNothingWhenCalledAgain()
    {
        editor.Start([SampleData.Exercise(Catalog, Squat, ["60", "8"])], null);

        await editor.SaveAsync((_, _) => Task.FromResult(true));
        await editor.SaveAsync((_, _) => Task.FromResult(true));

        Assert.That(haptics.Played, Is.EqualTo(new[] { HapticCue.WorkoutSaved }));
    }

    [Test]
    public async Task SaveAsync_Invalid_PlaysRejected()
    {
        await editor.SaveAsync((_, _) => Task.FromResult(true));

        Assert.That(haptics.Played, Is.EqualTo(new[] { HapticCue.Rejected }));
    }

    [Test]
    public async Task SaveAsync_Fails_PlaysNothing()
    {
        editor.Start([SampleData.Exercise(Catalog, Squat, ["60", "8"])], null);

        await editor.SaveAsync((_, _) => Task.FromResult(false));

        Assert.That(haptics.Played, Is.Empty);
    }

    [Test]
    public async Task SaveAsync_Invalid_ReopensTheDoneExercisesWithErrorsOnly()
    {
        var bad = SampleData.Exercise(Catalog, Squat, ["60", "0"]);
        var good = SampleData.Exercise(Catalog, Treadmill, ["0:20:00", "3"]);
        editor.Start([bad, good], null);
        bad.IsDone = true;
        good.IsDone = true;

        await editor.SaveAsync((_, _) => Task.FromResult(true));

        Assert.Multiple(() =>
        {
            Assert.That(bad.IsDone, Is.False);
            Assert.That(good.IsDone, Is.True);
        });
    }

    [Test]
    public async Task SaveAsync_Valid_SavesTheReadRecordsAndKeepsSaving()
    {
        editor.Start([SampleData.Exercise(Catalog, Squat, ["60", "8"]), SampleData.Exercise(Catalog, Treadmill, ["0:20:00", "3"])], null);
        IReadOnlyList<WorkoutExercise>? records = null;

        var saved = await editor.SaveAsync((_, r) =>
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
            Assert.That(editor.SaveState, Is.Not.EqualTo(SavePhase.Idle));
        });
    }

    [Test]
    public async Task SaveAsync_AfterAPickAgain_SavesTheRecordsOldestFirst()
    {
        editor.Start([SampleData.Exercise(Catalog, Squat, ["60", "8"]), SampleData.Exercise(Catalog, Treadmill, ["0:20:00", "3"])], null);
        editor.Pick(Item(Squat));
        IReadOnlyList<WorkoutExercise>? records = null;

        await editor.SaveAsync((_, r) =>
        {
            records = r;
            return Task.FromResult(true);
        });

        Assert.That(records!.Select(r => r.ExerciseId), Is.EqualTo(new[] { Treadmill, Squat }));
    }

    [Test]
    public async Task SaveAsync_WhileSavingOrAfterASave_IsIgnored()
    {
        editor.Start([SampleData.Exercise(Catalog, Squat, ["60", "8"])], null);
        var pending = new TaskCompletionSource<bool>();
        var saves = 0;
        Task<bool> Save(string? _, IReadOnlyList<WorkoutExercise> __)
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
        editor.Start([SampleData.Exercise(Catalog, Squat, ["60", "8"])], null);

        var failed = await editor.SaveAsync((_, _) => Task.FromResult(false));

        Assert.Multiple(() =>
        {
            Assert.That(failed, Is.False);
            Assert.That(editor.Error, Is.EqualTo(workoutChecker.SaveFailed));
            Assert.That(editor.ErrorFocus!.Target, Is.EqualTo(ErrorFocusTarget.Save));
            Assert.That(editor.SaveState, Is.EqualTo(SavePhase.Idle));
        });

        var retried = await editor.SaveAsync((_, _) => Task.FromResult(true));

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
