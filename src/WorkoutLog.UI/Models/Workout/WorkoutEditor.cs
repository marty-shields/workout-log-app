using WorkoutLog.Core.Domain.Exercises;
using WorkoutLog.Core.Domain.Workouts;
using WorkoutLog.Core.Services;
using WorkoutLog.UI.Haptics;
using WorkoutLog.UI.Models.Exercises;

namespace WorkoutLog.UI.Models.Workout;

/// <summary>
/// The exercises of a new or saved workout while it is edited: the add-exercise list, adding and removing exercises, changes,
/// and the check and save. One instance per workout page.
/// </summary>
/// <param name="catalog">Exercise list.</param>
/// <param name="setChecker">Reads each exercise as a record to save.</param>
/// <param name="workoutChecker">Checks the workout on Save and words a failed save or delete.</param>
/// <param name="loggedExercises">Reads the last sessions for the add-exercise list.</param>
/// <param name="clock">Time zone and current time for the last sessions' dates.</param>
/// <param name="haptics">Plays <see cref="HapticCue.WorkoutSaved"/> or <see cref="HapticCue.Rejected"/> on Save.</param>
public sealed class WorkoutEditor(
    IExerciseCatalog catalog, ISetChecker setChecker, IWorkoutChecker workoutChecker, ILoggedExercisesService loggedExercises, TimeProvider clock,
    IHaptics haptics)
{
    private List<ExerciseState> exercises = [];
    private IReadOnlyList<ExerciseListItem> listItems = [];
    private int listOpens;

    /// <summary>Exercises in screen order, newest first.</summary>
    public IReadOnlyList<ExerciseState> Exercises => exercises;

    /// <summary>The add-exercise list is open.</summary>
    public bool ListOpen { get; private set; }

    /// <summary>The add-exercise list is loading the last sessions.</summary>
    public bool ListLoading { get; private set; }

    /// <summary>The add-exercise list's search text.</summary>
    public string Query { get; private set; } = "";

    /// <summary>The add-exercise list's chip.</summary>
    public ExerciseFilter Filter { get; private set; }

    /// <summary>The add-exercise list's rows for <see cref="Query"/> and <see cref="Filter"/>.</summary>
    public IReadOnlyList<ExerciseListItem> ListItems { get; private set; } = [];

    /// <summary>The latest request to focus an exercise.</summary>
    public ExerciseFocus? Focus { get; private set; }

    /// <summary>The latest request to move focus after a failed Save or Delete.</summary>
    public ErrorFocus? ErrorFocus { get; private set; }

    /// <summary>The latest deletion Undo can put back, shown in the snackbar. Null when there is none.</summary>
    public PendingUndo? PendingUndo { get; private set; }

    /// <summary>Message on the workout itself.</summary>
    public string? Error { get; private set; }

    /// <summary>The workout's note as typed. Null when there is none.</summary>
    public string? Note { get; private set; }

    /// <summary>Validation message for <see cref="Note"/>, set by a failed Save and cleared when the note changes.</summary>
    public string? NoteError { get; private set; }

    /// <summary>Whether a save is running or has succeeded.</summary>
    public SavePhase SaveState { get; private set; }

    /// <summary>The Discard prompt is open.</summary>
    public bool DiscardOpen { get; private set; }

    public bool Discarded { get; private set; }

    /// <summary>
    /// A value or note was typed, a set added or removed, or an exercise added, picked again or removed since <see cref="Start"/>,
    /// even if later undone.
    /// </summary>
    public bool Changed { get; private set; }

    /// <summary>Edits a copy of these exercises, given oldest first, and the workout's note, with nothing else open, shown or changed.</summary>
    public void Start(IEnumerable<ExerciseState> loaded, string? note)
    {
        exercises = [.. loaded.Reverse()];
        Note = note;
        NoteError = null;
        ListOpen = false;
        ListLoading = false;
        Query = "";
        Filter = ExerciseFilter.All;
        listItems = [];
        ListItems = [];
        Focus = null;
        ErrorFocus = null;
        PendingUndo = null;
        Error = null;
        SaveState = SavePhase.Idle;
        DiscardOpen = false;
        Discarded = false;
        Changed = false;
    }

    /// <summary>Opens the Discard prompt. Ignored while a save runs, after one succeeds and after a discard.</summary>
    public void AskDiscard()
    {
        if (SaveState == SavePhase.Idle && !Discarded)
        {
            DiscardOpen = true;
        }
    }

    public void CloseDiscard() => DiscardOpen = false;

    /// <summary>
    /// Confirms Discard, once; returns true the first time. While a save runs or after one succeeds, closes the prompt and
    /// returns false. After a discard, returns false and leaves the prompt open.
    /// </summary>
    public bool ConfirmDiscard()
    {
        if (SaveState != SavePhase.Idle)
        {
            DiscardOpen = false;
            return false;
        }

        if (Discarded)
        {
            return false;
        }

        Discarded = true;
        return true;
    }

    /// <summary>
    /// Opens the add-exercise list with no search text and the All chip, loading while it reads the last sessions through
    /// <see cref="ILoggedExercisesService.GetAsync"/>. Then lists the whole catalog, Strength then Cardio A–Z, each logged exercise
    /// with its last session. A failed read lists the catalog with no last sessions. A read finished after the list closed or
    /// reopened is ignored.
    /// </summary>
    public async Task OpenListAsync()
    {
        var open = ++listOpens;
        ListOpen = true;
        ListLoading = true;
        ListItems = [];
        Query = "";
        Filter = ExerciseFilter.All;
        var result = await loggedExercises.GetAsync();
        if (open != listOpens || !ListOpen)
        {
            return;
        }

        var logged = result.Exercises.ToDictionary(e => e.ExerciseId);
        var now = clock.GetUtcNow();
        listItems = catalog.Search(null)
            .Select(e => ExerciseListItems.Item(e, logged.GetValueOrDefault(e.Id), clock.LocalTimeZone, now))
            .ToList();
        ListLoading = false;
        Find();
    }

    /// <summary>Sets the search text and the rows.</summary>
    public void Search(string text)
    {
        Query = text;
        Find();
    }

    /// <summary>Sets the chip and the rows.</summary>
    public void FilterBy(ExerciseFilter filter)
    {
        Filter = filter;
        Find();
    }

    public void CloseList() => ListOpen = false;

    /// <summary>
    /// Moves the exercise already in the workout to the top, opens it if it is done and focuses it, adding a set to a strength one with fewer than
    /// <see cref="WorkoutLimits.MaxSets"/>; otherwise adds it at the top, unless the workout has <see cref="WorkoutLimits.MaxExercises"/>.
    /// Either is a change. Clears the workout's error, closes the add-exercise list and ends an Undo that no longer fits.
    /// </summary>
    public void Pick(ExerciseListItem item)
    {
        if (exercises.TrueForAll(e => e.ExerciseId != item.Id) && exercises.Count >= WorkoutLimits.MaxExercises)
        {
            ListOpen = false;
            return;
        }

        Focus = Show(item.Id);
        Error = null;
        ListOpen = false;
        EndUndoThatNoLongerFits();
    }

    /// <summary>Takes the exercise out at once, a change, and offers Undo for it in place of any earlier deletion.</summary>
    public void Remove(ExerciseState exercise)
    {
        var index = exercises.IndexOf(exercise);
        if (index < 0)
        {
            return;
        }

        exercises.RemoveAt(index);
        PendingUndo = new RemovedExercise(exercise, index);
        Changed = true;
    }

    /// <summary>Takes the closed set out at once, a change, and offers Undo for it in place of any earlier deletion.</summary>
    public void RemoveSet(SetRef set)
    {
        var index = set.Exercise.Entries.IndexOf(set.Entry);
        if (index < 0)
        {
            return;
        }

        set.Exercise.Entries.RemoveAt(index);
        PendingUndo = new RemovedSet(set.Exercise, set.Entry, index);
        Changed = true;
    }

    /// <summary>
    /// Puts the <see cref="PendingUndo"/> deletion back at its position, or at the end when fewer are left, with its values and
    /// done state, and clears it; ignored when it no longer <see cref="PendingUndo.Fits"/>, and while a save runs, which has read
    /// the workout without it. Not a change undone: <see cref="Changed"/> stays set.
    /// </summary>
    public void Undo()
    {
        if (SaveState != SavePhase.Idle)
        {
            return;
        }

        EndUndoThatNoLongerFits();
        switch (PendingUndo)
        {
            case RemovedExercise removed:
                exercises.Insert(Math.Min(removed.Index, exercises.Count), removed.Exercise);
                break;
            case RemovedSet removed:
                removed.Exercise.Entries.Insert(Math.Min(removed.Index, removed.Exercise.Entries.Count), removed.Entry);
                break;
        }

        PendingUndo = null;
    }

    /// <summary>Keeps the deletion deleted and hides its snackbar, unless a later deletion has replaced it.</summary>
    public void DismissUndo(PendingUndo undo)
    {
        if (ReferenceEquals(PendingUndo, undo))
        {
            PendingUndo = null;
        }
    }

    /// <summary>
    /// Records a value, note, set or entry change, and ends an Undo it leaves no room for, such as a set added to an exercise
    /// whose deleted set Undo would put back past <see cref="WorkoutLimits.MaxSets"/>.
    /// </summary>
    public void MarkChanged()
    {
        Changed = true;
        EndUndoThatNoLongerFits();
    }

    /// <summary>Sets the workout's note, a change, and clears its error.</summary>
    public void SetNote(string? note)
    {
        Note = note;
        NoteError = null;
        Changed = true;
    }

    /// <summary>
    /// Checks the workout and asks to focus the first error. A valid workout goes to <paramref name="save"/> with its note and
    /// the records, oldest first; returns true when it was saved, and the <see cref="PendingUndo"/> ends. If not, shows SaveFailed,
    /// asks to focus Save and keeps the Undo. Ignored while a save runs and after one succeeds. Plays <see cref="HapticCue.Rejected"/> for an invalid
    /// workout and <see cref="HapticCue.WorkoutSaved"/> once saved; a failed save plays nothing, its message says why.
    /// </summary>
    /// <param name="save">Saves the note and the records; true when saved.</param>
    public async Task<bool> SaveAsync(Func<string?, IReadOnlyList<WorkoutExercise>, Task<bool>> save)
    {
        if (SaveState != SavePhase.Idle)
        {
            return false;
        }

        var check = workoutChecker.Check(exercises, Note);
        Error = check.Error;
        NoteError = check.NoteError;
        if (!check.IsValid)
        {
            foreach (var exercise in exercises.Where(e => e.HasError))
            {
                exercise.IsDone = false;
            }

            ErrorFocus = new ErrorFocus();
            haptics.Play(HapticCue.Rejected);
            return false;
        }

        SaveState = SavePhase.Running;
        var saved = await save(Note, exercises.Select(setChecker.Read).Reverse().ToList());
        SaveState = saved ? SavePhase.Succeeded : SavePhase.Idle;
        if (saved)
        {
            PendingUndo = null;
            haptics.Play(HapticCue.WorkoutSaved);
        }
        else
        {
            Error = workoutChecker.SaveFailed;
            ErrorFocus = new ErrorFocus(ErrorFocusTarget.Save);
        }

        return saved;
    }

    /// <summary>Shows DeleteFailed and asks to focus Delete.</summary>
    public void ShowDeleteFailed()
    {
        Error = workoutChecker.DeleteFailed;
        ErrorFocus = new ErrorFocus(ErrorFocusTarget.Delete);
    }

    private void EndUndoThatNoLongerFits()
    {
        if (PendingUndo?.Fits(exercises) == false)
        {
            PendingUndo = null;
        }
    }

    private void Find() => ListItems = ExerciseListItems.Find(listItems, catalog, Query, Filter);

    private ExerciseFocus Show(int exerciseId)
    {
        if (exercises.Find(e => e.ExerciseId == exerciseId) is { } existing)
        {
            exercises.Remove(existing);
            exercises.Insert(0, existing);
            existing.IsDone = false;
            Changed = true;
            return new ExerciseFocus(existing, addSet: existing.Kind == ExerciseKind.Strength && !existing.SetsFull);
        }

        var added = ExerciseFields.New(catalog.All.Single(e => e.Id == exerciseId));
        exercises.Insert(0, added);
        Changed = true;
        return new ExerciseFocus(added);
    }
}
