using static WorkoutLog.UI.Stories.Stories.Prototype.ScenarioStep;

namespace WorkoutLog.UI.Stories.Stories.Prototype;

/// <summary>Typical user tasks, played on the prototype's sample data (docs/06).</summary>
public static class Scenarios
{
    /// <summary>Empty Dashboard and History, then leaving and reopening the app.</summary>
    public static readonly Scenario FirstUse = new(
        "First use",
        "The app with no saved workouts.",
        [
            Show("The Dashboard has no last workout", "last-workout-empty"),
            Show("This week is 0", "count-week"),
            Show("This month is 0", "count-month"),
            Click("Open History", "history-link"),
            Show("History is empty", "history-empty"),
            Click("Back to the Dashboard", "back"),
            Back("Android back on the Dashboard leaves the app"),
            Click("Reopen the app", "reopen"),
        ],
        Empty: true);

    /// <summary>Two strength exercises with sets, then save.</summary>
    public static readonly Scenario LogStrength = new(
        "Log a strength workout",
        "Add exercises, fill in sets, add and remove a set, then save.",
        [
            Click("Start a new workout", "new-workout"),
            Show("The timer starts", "screen-subtitle"),
            Click("Add exercise", "add-exercise"),
            Click("Pick Barbell Back Squat", "picker-item-6"),
            Enter("Set 1: 8 reps", "exercise-1-set-1-reps", "8"),
            Enter("Set 1: 60 kg", "exercise-1-set-1-weight", "60"),
            Click("Add set", "exercise-1-add-set"),
            Enter("Set 2: 8 reps", "exercise-1-set-2-reps", "8"),
            Enter("Set 2: 62.5 kg", "exercise-1-set-2-weight", "62.5"),
            Click("Add a third set", "exercise-1-add-set"),
            Click("Remove set 3", "exercise-1-set-3-remove"),
            Click("Add exercise", "add-exercise"),
            Click("Pick Dumbbell Curl", "picker-item-132"),
            Enter("Set 1: 12 reps", "exercise-2-set-1-reps", "12"),
            Enter("Set 1: 12.5 kg", "exercise-2-set-1-weight", "12.5"),
            Click("Save", "save-workout"),
            Show("The Dashboard shows today's workout", "last-workout"),
            Show("The week count goes up", "count-week"),
        ]);

    /// <summary>Two cardio entries, including pace, then save.</summary>
    public static readonly Scenario LogCardio = new(
        "Log a cardio workout",
        "Duration, distance, incline and pace, then save.",
        [
            Click("Start a new workout", "new-workout"),
            Click("Add exercise", "add-exercise"),
            Click("Pick Treadmill", "picker-item-300"),
            Enter("Duration: 0 h", "exercise-1-duration-h", "0"),
            Enter("20 min", "exercise-1-duration-mm", "20"),
            Enter("00 s", "exercise-1-duration-ss", "00"),
            Enter("Distance: 3.2 km", "exercise-1-distance", "3.2"),
            Enter("Incline: 1.5 %", "exercise-1-incline", "1.5"),
            Click("Add exercise", "add-exercise"),
            Click("Pick Outdoor Run", "picker-item-311"),
            Enter("Duration: 0 h", "exercise-2-duration-h", "0"),
            Enter("25 min", "exercise-2-duration-mm", "25"),
            Enter("30 s", "exercise-2-duration-ss", "30"),
            Enter("Distance: 5 km", "exercise-2-distance", "5"),
            Enter("Pace: 5 min", "exercise-2-rate-m", "5"),
            Enter("06 s per km", "exercise-2-rate-ss", "06"),
            Click("Save", "save-workout"),
            Show("The Dashboard shows today's workout", "last-workout"),
        ]);

    /// <summary>Add set blocked by a set with errors; then several errors per exercise, set and entry on save, fixed and saved.</summary>
    public static readonly Scenario SaveWithMistakes = new(
        "Save with mistakes",
        "Add set waits for a valid set. On save, each faulty field is outlined, with one message per problem under its set or entry.",
        [
            Click("Start a new workout", "new-workout"),
            Click("Save with no exercises", "save-workout"),
            Show("Error: add at least one exercise", "workout-error"),
            Click("Add exercise", "add-exercise"),
            Click("Pick Barbell Back Squat", "picker-item-6"),
            Enter("Set 1: 60 kg, no reps", "exercise-1-set-1-weight", "60"),
            Click("Add set is blocked", "exercise-1-add-set"),
            Show("Set 1 shows why: enter reps", "exercise-1-set-1-error"),
            Enter("Set 1: 8 reps", "exercise-1-set-1-reps", "8"),
            Click("Add set", "exercise-1-add-set"),
            Enter("Set 2: 0 reps", "exercise-1-set-2-reps", "0"),
            Enter("Set 2: 60 kg", "exercise-1-set-2-weight", "60"),
            Click("Add exercise", "add-exercise"),
            Click("Pick Farmer's Walk", "picker-item-187"),
            Enter("No distance; 32 kg", "exercise-2-set-1-weight", "32"),
            Enter("Duration: 0 h", "exercise-2-set-1-duration-h", "0"),
            Enter("00 min", "exercise-2-set-1-duration-mm", "00"),
            Enter("75 s", "exercise-2-set-1-duration-ss", "75"),
            Click("Add set is blocked", "exercise-2-add-set"),
            Show("Two errors keep set 1 open", "exercise-2-set-1-error"),
            Click("Add exercise", "add-exercise"),
            Click("Pick Dumbbell Curl", "picker-item-132"),
            Click("Remove its only set", "exercise-3-set-1-remove"),
            Click("Add exercise", "add-exercise"),
            Click("Pick Outdoor Run", "picker-item-311"),
            Enter("No duration; distance 5 km", "exercise-4-distance", "5"),
            Enter("Pace: 5 min", "exercise-4-rate-m", "5"),
            Enter("75 s per km", "exercise-4-rate-ss", "75"),
            Click("Save", "save-workout"),
            Show("Squat set 2: reps must be more than 0", "exercise-1-set-2-error"),
            Show("Farmer's Walk: two errors on one set", "exercise-2-set-1-error"),
            Show("Dumbbell Curl: add at least one set", "exercise-3-error"),
            Show("Outdoor Run: two errors on one entry", "exercise-4-entry-error"),
            Enter("Squat set 2: 8 reps", "exercise-1-set-2-reps", "8"),
            Enter("Farmer's Walk: 40 m", "exercise-2-set-1-distance", "40"),
            Enter("Duration: 45 s", "exercise-2-set-1-duration-ss", "45"),
            Click("Dumbbell Curl: add set", "exercise-3-add-set"),
            Enter("Set 1: 12 reps", "exercise-3-set-1-reps", "12"),
            Enter("Outdoor Run duration: 0 h", "exercise-4-duration-h", "0"),
            Enter("25 min", "exercise-4-duration-mm", "25"),
            Enter("30 s", "exercise-4-duration-ss", "30"),
            Enter("Pace: 30 s per km", "exercise-4-rate-ss", "30"),
            Click("Save", "save-workout"),
            Show("Saved: the Dashboard shows today's workout", "last-workout"),
        ]);

    /// <summary>Picker search by name and nickname, no match, close with back.</summary>
    public static readonly Scenario FindExercise = new(
        "Find an exercise",
        "Search by name or nickname, in any word order.",
        [
            Click("Start a new workout", "new-workout"),
            Click("Add exercise", "add-exercise"),
            Show("No search text: Strength then Cardio, A–Z", "picker-group-Strength"),
            Enter("Search \"chest\"", "picker-search", "chest"),
            Show("Name matches: Machine Chest Fly", "picker-item-221"),
            Enter("Search the nickname \"pec deck\"", "picker-search", "pec deck"),
            Show("Finds Machine Chest Fly", "picker-item-221"),
            Enter("Search \"gravitron\"", "picker-search", "gravitron"),
            Show("Finds Machine Assisted Pull-up", "picker-item-218"),
            Enter("Search \"zumba\"", "picker-search", "zumba"),
            Show("No exercises match", "picker-none"),
            Back("Android back closes the picker"),
            Click("Add exercise", "add-exercise"),
            Enter("Search \"rower\"", "picker-search", "rower"),
            Click("Pick Rowing Machine", "picker-item-305"),
            Show("Added to the workout", "exercise-1"),
        ]);

    /// <summary>Discard prompt on a new workout: cancel, then discard.</summary>
    public static readonly Scenario AbandonWorkout = new(
        "Abandon a new workout",
        "Back asks before dropping a new workout.",
        [
            Click("Start a new workout", "new-workout"),
            Click("Add exercise", "add-exercise"),
            Click("Pick Barbell Back Squat", "picker-item-6"),
            Enter("Set 1: 5 reps", "exercise-1-set-1-reps", "5"),
            Click("Back", "back"),
            Show("Discard this workout?", "confirm-dialog"),
            Click("Cancel keeps the workout", "confirm-dialog-cancel"),
            Back("Android back asks again"),
            Click("Discard", "confirm-dialog-confirm"),
            Show("Nothing saved: the last workout is still yesterday's", "last-workout"),
        ]);

    /// <summary>History from the Dashboard and back again.</summary>
    public static readonly Scenario ViewHistory = new(
        "View history",
        "Saved workouts, newest first.",
        [
            Show("The Dashboard shows the last workout", "last-workout"),
            Click("Open History", "history-link"),
            Show("Newest first", "history-row-3"),
            Show("Oldest last", "history-row-1"),
            Click("Back to the Dashboard", "back"),
            Click("Open History again", "history-link"),
            Back("Android back also returns to the Dashboard"),
        ]);

    /// <summary>Change values, add a set, swap an exercise, save, reopen.</summary>
    public static readonly Scenario EditWorkout = new(
        "Edit a workout",
        "Open a saved workout, change it and save.",
        [
            Click("Open History", "history-link"),
            Click("Open yesterday's workout", "history-row-3"),
            Show("Edit shows the total time", "screen-subtitle"),
            Enter("Squat set 3: 65 kg", "exercise-1-set-3-weight", "65"),
            Click("Add set", "exercise-1-add-set"),
            Enter("Set 4: 5 reps", "exercise-1-set-4-reps", "5"),
            Enter("Set 4: 65 kg", "exercise-1-set-4-weight", "65"),
            Click("Remove Plank", "exercise-3-remove"),
            Click("Add exercise", "add-exercise"),
            Enter("Search \"incline\"", "picker-search", "incline"),
            Click("Pick Dumbbell Incline Press", "picker-item-144"),
            Enter("Set 1: 10 reps", "exercise-4-set-1-reps", "10"),
            Enter("Set 1: 22 kg", "exercise-4-set-1-weight", "22"),
            Click("Save", "save-workout"),
            Click("Open History", "history-link"),
            Click("Reopen the workout", "history-row-3"),
            Show("The new set is saved", "exercise-1-set-4"),
            Show("Dumbbell Incline Press replaced Plank", "exercise-4"),
            Click("No changes: back goes straight to History", "back"),
        ]);

    /// <summary>Discard changes prompt on an edit: cancel, then discard.</summary>
    public static readonly Scenario DiscardEdits = new(
        "Discard edits",
        "Back asks before dropping unsaved changes.",
        [
            Click("Open History", "history-link"),
            Click("Open the cardio workout", "history-row-2"),
            Enter("Treadmill distance: 4 km", "exercise-1-distance", "4"),
            Click("Back", "back"),
            Show("Discard your changes?", "confirm-dialog"),
            Click("Cancel keeps editing", "confirm-dialog-cancel"),
            Back("Android back asks again"),
            Click("Discard changes", "confirm-dialog-confirm"),
            Show("Back on History", "history-row-2"),
            Click("Reopen the workout", "history-row-2"),
            Show("The distance is unchanged", "exercise-1-distance"),
        ]);

    /// <summary>Delete a workout after confirming.</summary>
    public static readonly Scenario DeleteWorkout = new(
        "Delete a workout",
        "Delete asks first, then the workout leaves History.",
        [
            Click("Open History", "history-link"),
            Click("Open the oldest workout", "history-row-1"),
            Click("Delete", "delete-workout"),
            Show("Delete this workout?", "confirm-dialog"),
            Click("Cancel keeps it", "confirm-dialog-cancel"),
            Click("Delete again", "delete-workout"),
            Click("Confirm delete", "confirm-dialog-confirm"),
            Show("Back on History: two workouts left", "history-row-2"),
        ]);
}
