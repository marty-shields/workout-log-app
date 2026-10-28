using WorkoutLog.Core.Domain.Exercises;
using WorkoutLog.Core.Domain.Workouts;
using WorkoutLog.Core.Services;
using WorkoutLog.UI.Models.Exercises;

namespace WorkoutLog.UI.Models.Workout;

/// <summary>
/// The exercises of a new or saved workout while it is edited: the add-exercise list, adding and removing exercises, changes
/// (D73), and the check and save. One instance per workout page.
/// </summary>
/// <param name="catalog">Exercise list.</param>
/// <param name="setChecker">Reads each exercise as a record to save.</param>
/// <param name="workoutChecker">Checks the workout on Save and words a failed save or delete.</param>
/// <param name="loggedExercises">Reads the last sessions for the add-exercise list.</param>
/// <param name="clock">Time zone and current time for the last sessions' dates.</param>
public sealed class WorkoutEditor(
    IExerciseCatalog catalog, ISetChecker setChecker, IWorkoutChecker workoutChecker, ILoggedExercisesService loggedExercises, TimeProvider clock)
{
    private List<ExerciseState> exercises = [];
    private IReadOnlyList<ExerciseListItem> listItems = [];
    private int listOpens;

    /// <summary>Exercises in screen order, newest first (D42).</summary>
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
        ListOpen = false;
        ListLoading = false;
        Query = "";
        Filter = ExerciseFilter.All;
        listItems = [];
        ListItems = [];
        Focus = null;
        ErrorFocus = null;
        Error = null;
        Saving = false;
        Changed = false;
    }

    /// <summary>
    /// Opens the add-exercise list with no search text and the All chip (D64), loading while it reads the last sessions through
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

    /// <summary>Closes the add-exercise list.</summary>
    public void CloseList() => ListOpen = false;

    /// <summary>
    /// Moves the exercise already in the workout to the top and focuses it, adding a set to a strength one; otherwise adds it at
    /// the top. Either is a change. Clears the workout's error and closes the add-exercise list.
    /// </summary>
    public void Pick(ExerciseListItem item)
    {
        Focus = Show(item.Id);
        Error = null;
        ListOpen = false;
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

    private void Find() => ListItems = ExerciseListItems.Find(listItems, catalog, Query, Filter);

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
