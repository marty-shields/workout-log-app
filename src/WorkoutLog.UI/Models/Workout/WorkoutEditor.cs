using WorkoutLog.Core.Domain.Exercises;
using WorkoutLog.Core.Domain.Workouts;

namespace WorkoutLog.UI.Models.Workout;

/// <summary>
/// The exercises of a new or saved workout while it is edited: the exercise picker, adding and removing exercises, changes
/// (D73), and the check and save. One instance per workout page.
/// </summary>
/// <param name="catalog">Exercise list.</param>
/// <param name="setChecker">Reads each exercise as a record to save.</param>
/// <param name="workoutChecker">Checks the workout on Save and words a failed save or delete.</param>
public sealed class WorkoutEditor(IExerciseCatalog catalog, ISetChecker setChecker, IWorkoutChecker workoutChecker)
{
    private List<ExerciseState> exercises = [];

    /// <summary>Exercises in screen order, newest first (D42).</summary>
    public IReadOnlyList<ExerciseState> Exercises => exercises;

    /// <summary>The exercise picker is open.</summary>
    public bool PickerOpen { get; private set; }

    /// <summary>The picker's search text.</summary>
    public string Query { get; private set; } = "";

    /// <summary>The picker's results for <see cref="Query"/>.</summary>
    public IReadOnlyList<PickerItem> PickerItems { get; private set; } = [];

    /// <summary>The latest request to focus an exercise.</summary>
    public ExerciseFocus? Focus { get; private set; }

    /// <summary>The latest request to move focus after a failed Save or Delete.</summary>
    public ErrorFocus? ErrorFocus { get; private set; }

    /// <summary>Message on the workout itself.</summary>
    public string? Error { get; private set; }

    /// <summary>A save is running or has succeeded.</summary>
    public bool Saving { get; private set; }

    /// <summary>
    /// A value was typed, a set added or removed, or an exercise added, picked again or removed since <see cref="Start"/>, even if
    /// later undone (D73).
    /// </summary>
    public bool Changed { get; private set; }

    /// <summary>Edits a copy of these exercises, given oldest first, with nothing else open, shown or changed.</summary>
    public void Start(IEnumerable<ExerciseState> loaded)
    {
        exercises = [.. loaded.Reverse()];
        PickerOpen = false;
        Query = "";
        PickerItems = [];
        Focus = null;
        ErrorFocus = null;
        Error = null;
        Saving = false;
        Changed = false;
    }

    /// <summary>Opens the picker on the whole list.</summary>
    public void OpenPicker()
    {
        Search("");
        PickerOpen = true;
    }

    /// <summary>Sets the search text and its results.</summary>
    public void Search(string text)
    {
        Query = text;
        PickerItems = catalog.Search(text).Select(ExerciseFields.PickerItem).ToList();
    }

    /// <summary>Closes the picker.</summary>
    public void ClosePicker() => PickerOpen = false;

    /// <summary>
    /// Moves the exercise already in the workout to the top and focuses it, adding a set to a strength one; otherwise adds it at
    /// the top. Either is a change. Clears the workout's error and closes the picker.
    /// </summary>
    public void Pick(PickerItem item)
    {
        Focus = Show(item.Id);
        Error = null;
        PickerOpen = false;
    }

    /// <summary>Removes the exercise.</summary>
    public void Remove(ExerciseState exercise)
    {
        exercises.Remove(exercise);
        Changed = true;
    }

    /// <summary>Records a value, set or entry change.</summary>
    public void MarkChanged() => Changed = true;

    /// <summary>
    /// Checks the workout (FR-09) and asks to focus the first error. A valid workout goes to <paramref name="save"/> as records,
    /// oldest first; returns true when it was saved. If not, shows SaveFailed and asks to focus Save (D40). Ignored while a save
    /// runs and after one succeeds (D41).
    /// </summary>
    /// <param name="save">Saves the records; true when saved.</param>
    public async Task<bool> SaveAsync(Func<IReadOnlyList<WorkoutExercise>, Task<bool>> save)
    {
        if (Saving)
        {
            return false;
        }

        var check = workoutChecker.Check(exercises);
        Error = check.Error;
        if (!check.IsValid)
        {
            ErrorFocus = new ErrorFocus();
            return false;
        }

        Saving = true;
        Saving = await save(exercises.Select(setChecker.Read).Reverse().ToList());
        if (!Saving)
        {
            Error = workoutChecker.SaveFailed;
            ErrorFocus = new ErrorFocus(ErrorFocusTarget.Save);
        }

        return Saving;
    }

    /// <summary>Shows DeleteFailed and asks to focus Delete (D46).</summary>
    public void ShowDeleteFailed()
    {
        Error = workoutChecker.DeleteFailed;
        ErrorFocus = new ErrorFocus(ErrorFocusTarget.Delete);
    }

    private ExerciseFocus Show(int exerciseId)
    {
        if (exercises.Find(e => e.ExerciseId == exerciseId) is { } existing)
        {
            exercises.Remove(existing);
            exercises.Insert(0, existing);
            Changed = true;
            return new ExerciseFocus(existing, addSet: existing.Kind == ExerciseKind.Strength);
        }

        var added = ExerciseFields.New(catalog.All.Single(e => e.Id == exerciseId));
        exercises.Insert(0, added);
        Changed = true;
        return new ExerciseFocus(added);
    }
}
