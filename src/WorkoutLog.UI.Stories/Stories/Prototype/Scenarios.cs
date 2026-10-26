using static WorkoutLog.UI.Stories.Stories.Prototype.ScenarioStep;

namespace WorkoutLog.UI.Stories.Stories.Prototype;

/// <summary>Typical user tasks, played on the prototype's sample data (docs/05).</summary>
public static class Scenarios
{
    /// <summary>The Startup screen while the database opens, then the Dashboard.</summary>
    public static readonly Scenario OpenApp = new(
        "Open the app",
        "The Startup screen shows while the database opens, then the Dashboard.",
        [
            Show("The app opens on the Startup screen", "startup-loading"),
            Show("Then the Dashboard shows", "last-workout"),
        ],
        StartupDelay: TimeSpan.FromSeconds(2));

    /// <summary>The Startup screen, then its error when the database can't be opened (D133).</summary>
    public static readonly Scenario StartupFails = new(
        "Startup fails",
        "If the database can't be opened, the Startup screen shows an error and the app goes no further.",
        [
            Show("The app opens on the Startup screen", "startup-loading"),
            Show("Couldn't open the workout database.", "startup-error", "Couldn't open the workout database."),
        ],
        StartupDelay: TimeSpan.FromSeconds(2),
        StartupFails: true);

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
            Show("Dumbbell Curl goes on top", "exercise-1", "Dumbbell Curl"),
            Enter("Set 1: 12 reps", "exercise-1-set-1-reps", "12"),
            Enter("Set 1: 12.5 kg", "exercise-1-set-1-weight", "12.5"),
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
            Enter("Duration: 0 h", "exercise-1-duration-h", "0"),
            Enter("25 min", "exercise-1-duration-mm", "25"),
            Enter("30 s", "exercise-1-duration-ss", "30"),
            Enter("Distance: 5 km", "exercise-1-distance", "5"),
            Enter("Pace: 5 min", "exercise-1-pace-m", "5"),
            Enter("06 s per km", "exercise-1-pace-ss", "06"),
            Click("Save", "save-workout"),
            Show("The Dashboard shows today's workout", "last-workout"),
        ]);

    /// <summary>Workout and exercise errors on save: no exercises, then an exercise with no sets (D171).</summary>
    public static readonly Scenario WorkoutMistakes = new(
        "Workout mistakes",
        "Save checks the workout and each exercise. Focus moves to the first error.",
        [
            Click("Start a new workout", "new-workout"),
            Click("Save with no exercises", "save-workout"),
            Show("Error: add at least one exercise; focus moves to Add exercise", "workout-error", "Add at least one exercise"),
            Click("Add exercise", "add-exercise"),
            Click("Pick Barbell Back Squat: the error goes", "picker-item-6"),
            Click("Remove its only set", "exercise-1-set-1-remove"),
            Click("Save", "save-workout"),
            Show("Error: add at least one set; focus moves to Add set", "exercise-1-error", "Add at least one set"),
            Click("Add set clears it", "exercise-1-add-set"),
            Enter("Set 1: 5 reps", "exercise-1-set-1-reps", "5"),
            Click("Save", "save-workout"),
            Show("Saved: the Dashboard shows today's workout", "last-workout"),
        ]);

    /// <summary>Strength set errors: format errors and Core's rules on a lift, an assisted machine, a hold and a carry.</summary>
    public static readonly Scenario StrengthMistakes = new(
        "Strength mistakes",
        "Add set waits for a valid set. On save, each faulty field is outlined, with one message per problem under its set.",
        [
            Click("Start a new workout", "new-workout"),
            Click("Add exercise", "add-exercise"),
            Click("Pick Barbell Back Squat", "picker-item-6"),
            Enter("Set 1: 60.255 kg, no reps", "exercise-1-set-1-weight", "60.255"),
            Click("Add set is blocked; focus moves to reps", "exercise-1-add-set"),
            Show("Enter reps; weight has up to 2 decimal places", "exercise-1-set-1-error", "Enter reps", "Weight can have up to 2 decimal places"),
            Enter("Reps 8.5", "exercise-1-set-1-reps", "8.5"),
            Show("Reps must be a whole number", "exercise-1-set-1-error", "Reps must be a whole number"),
            Enter("Reps 8.", "exercise-1-set-1-reps", "8."),
            Show("Reps must be a number", "exercise-1-set-1-error", "Reps must be a number"),
            Enter("Reps 0", "exercise-1-set-1-reps", "0"),
            Show("Reps must be more than 0", "exercise-1-set-1-error", "Reps must be more than 0"),
            Enter("Reps 1001", "exercise-1-set-1-reps", "1001"),
            Show("Reps must be 1,000 or less", "exercise-1-set-1-error", "Reps must be 1,000 or less"),
            Enter("Weight -5", "exercise-1-set-1-weight", "-5"),
            Show("Weight must be a number", "exercise-1-set-1-error", "Weight must be a number"),
            Enter("Reps 8", "exercise-1-set-1-reps", "8"),
            Enter("Weight 60: the errors go", "exercise-1-set-1-weight", "60"),
            Click("Add exercise", "add-exercise"),
            Click("Pick Machine Assisted Pull-up", "picker-item-218"),
            Enter("Set 1: 8 reps", "exercise-1-set-1-reps", "8"),
            Enter("Assist 1001 kg", "exercise-1-set-1-weight", "1001"),
            Click("Add exercise", "add-exercise"),
            Click("Pick Plank", "picker-item-258"),
            Enter("Duration: 0 h", "exercise-1-set-1-duration-h", "0"),
            Enter("00 min", "exercise-1-set-1-duration-mm", "00"),
            Enter("00 s", "exercise-1-set-1-duration-ss", "00"),
            Click("Add exercise", "add-exercise"),
            Click("Pick Farmer's Walk", "picker-item-187"),
            Enter("No distance; 32 kg", "exercise-1-set-1-weight", "32"),
            Enter("Duration: 45 s", "exercise-1-set-1-duration-ss", "45"),
            Click("Add set is blocked; focus moves to distance", "exercise-1-add-set"),
            Show("Enter distance", "exercise-1-set-1-error", "Enter distance"),
            Enter("Distance 40.125 m", "exercise-1-set-1-distance", "40.125"),
            Show("Distance has up to 2 decimal places", "exercise-1-set-1-error", "Distance can have up to 2 decimal places"),
            Click("Save: focus moves to the first error, the Farmer's Walk distance", "save-workout"),
            Show("Farmer's Walk: distance still at fault", "exercise-1-set-1-error", "Distance can have up to 2 decimal places"),
            Show("Plank: duration must be more than 0", "exercise-2-set-1-error", "Duration must be more than 0"),
            Enter("Plank: 75 min", "exercise-2-set-1-duration-mm", "75"),
            Show("Duration minutes and seconds must be 00–59", "exercise-2-set-1-error", "Duration minutes and seconds must be 00–59"),
            Show("Assist must be 1,000 or less", "exercise-3-set-1-error", "Assist must be 1,000 or less"),
            Enter("Farmer's Walk: 40 m", "exercise-1-set-1-distance", "40"),
            Enter("Plank: 01 min", "exercise-2-set-1-duration-mm", "01"),
            Enter("Assist 25 kg", "exercise-3-set-1-weight", "25"),
            Click("Save", "save-workout"),
            Show("Saved: the Dashboard shows today's workout", "last-workout"),
        ]);

    /// <summary>Cardio entry errors on save: format errors and Core's rules on four machines, pace included.</summary>
    public static readonly Scenario CardioMistakes = new(
        "Cardio mistakes",
        "Cardio entries are checked on save. Each faulty field is outlined, with one message per problem under its entry.",
        [
            Click("Start a new workout", "new-workout"),
            Click("Add exercise", "add-exercise"),
            Click("Pick Treadmill", "picker-item-300"),
            Enter("No duration; distance 1001 km", "exercise-1-distance", "1001"),
            Enter("Incline 1.255 %", "exercise-1-incline", "1.255"),
            Click("Add exercise", "add-exercise"),
            Click("Pick Rowing Machine", "picker-item-305"),
            Enter("Duration: 0 h", "exercise-1-duration-h", "0"),
            Enter("10 min", "exercise-1-duration-mm", "10"),
            Enter("00 s", "exercise-1-duration-ss", "00"),
            Enter("Damper 5.5", "exercise-1-level", "5.5"),
            Enter("Calories abc", "exercise-1-calories", "abc"),
            Click("Add exercise", "add-exercise"),
            Click("Pick Outdoor Run", "picker-item-311"),
            Enter("Duration: 0 h", "exercise-1-duration-h", "0"),
            Enter("25 min", "exercise-1-duration-mm", "25"),
            Enter("30 s", "exercise-1-duration-ss", "30"),
            Enter("Pace: 5 min", "exercise-1-pace-m", "5"),
            Enter("75 s per km", "exercise-1-pace-ss", "75"),
            Click("Add exercise", "add-exercise"),
            Click("Pick Stair Climber", "picker-item-309"),
            Enter("Duration: 0 h", "exercise-1-duration-h", "0"),
            Enter("15 min", "exercise-1-duration-mm", "15"),
            Enter("00 s", "exercise-1-duration-ss", "00"),
            Enter("Floors 1001", "exercise-1-floors", "1001"),
            Enter("Speed level 101", "exercise-1-level", "101"),
            Enter("Rate 301 steps/min", "exercise-1-rate", "301"),
            Click("Save: focus moves to the Stair Climber's floors", "save-workout"),
            Show("Stair Climber: floors, speed level and rate too large", "exercise-1-entry-error", "Floors must be 1,000 or less", "Speed level must be 100 or less", "Rate must be 300 or less"),
            Show("Outdoor Run: pace seconds must be 00–59", "exercise-2-entry-error", "Pace seconds must be 00–59"),
            Show("Rowing Machine: damper must be a whole number; calories must be a number", "exercise-3-entry-error", "Damper must be a whole number", "Calories must be a number"),
            Show("Treadmill: enter duration; distance 1,000 or less; incline up to 2 decimal places", "exercise-4-entry-error", "Enter duration", "Distance must be 1,000 or less", "Incline can have up to 2 decimal places"),
            Enter("Run pace: 61 min", "exercise-2-pace-m", "61"),
            Enter("00 s", "exercise-2-pace-ss", "00"),
            Show("Pace must be 60:00 or less", "exercise-2-entry-error", "Pace must be 60:00 or less"),
            Enter("Floors 50", "exercise-1-floors", "50"),
            Enter("Speed level 8", "exercise-1-level", "8"),
            Enter("Rate 90 steps/min", "exercise-1-rate", "90"),
            Enter("Pace: 5 min", "exercise-2-pace-m", "5"),
            Enter("30 s per km", "exercise-2-pace-ss", "30"),
            Enter("Damper 5", "exercise-3-level", "5"),
            Enter("Calories 120", "exercise-3-calories", "120"),
            Enter("Treadmill duration: 0 h", "exercise-4-duration-h", "0"),
            Enter("20 min", "exercise-4-duration-mm", "20"),
            Enter("Distance 3.2 km", "exercise-4-distance", "3.2"),
            Enter("Incline 1.5 %", "exercise-4-incline", "1.5"),
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

    /// <summary>Picking an exercise already in the workout moves its card to the top (D154, D236).</summary>
    public static readonly Scenario PickAgain = new(
        "Pick an exercise again",
        "An exercise already in the workout isn't added twice: its card moves to the top; strength gets a new set, cardio focuses its entry.",
        [
            Click("Start a new workout", "new-workout"),
            Click("Add exercise", "add-exercise"),
            Enter("Search \"squat\"", "picker-search", "squat"),
            Click("Pick Barbell Back Squat", "picker-item-6"),
            Enter("Set 1: 5 reps", "exercise-1-set-1-reps", "5"),
            Click("Add exercise", "add-exercise"),
            Enter("Search \"rower\"", "picker-search", "rower"),
            Click("Pick Rowing Machine", "picker-item-305"),
            Show("Rowing Machine goes on top", "exercise-1", "Rowing Machine"),
            Click("Add exercise", "add-exercise"),
            Enter("Search \"squat\" again", "picker-search", "squat"),
            Click("Pick Barbell Back Squat again", "picker-item-6"),
            Show("Barbell Back Squat moves to the top", "exercise-1", "Barbell Back Squat"),
            Show("Set 2 is added to the same card", "exercise-1-set-2"),
            Enter("Set 2: 5 reps", "exercise-1-set-2-reps", "5"),
            Click("Add exercise", "add-exercise"),
            Enter("Search \"rower\" again", "picker-search", "rower"),
            Click("Pick Rowing Machine again", "picker-item-305"),
            Show("Still two exercises: Rowing Machine moves back to the top, its entry focused", "exercise-1-duration-h"),
            Show("Barbell Back Squat is second", "exercise-2", "Barbell Back Squat"),
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
            Show("Newest first: Farmer's Walk on top", "exercise-1", "Farmer's Walk"),
            Enter("Squat set 3: 65 kg", "exercise-4-set-3-weight", "65"),
            Click("Add set", "exercise-4-add-set"),
            Enter("Set 4: 5 reps", "exercise-4-set-4-reps", "5"),
            Enter("Set 4: 65 kg", "exercise-4-set-4-weight", "65"),
            Click("Remove Plank", "exercise-2-remove"),
            Click("Add exercise", "add-exercise"),
            Enter("Search \"incline\"", "picker-search", "incline"),
            Click("Pick Dumbbell Incline Press", "picker-item-144"),
            Enter("Set 1: 10 reps", "exercise-1-set-1-reps", "10"),
            Enter("Set 1: 22 kg", "exercise-1-set-1-weight", "22"),
            Click("Save", "save-workout"),
            Click("Open History", "history-link"),
            Click("Reopen the workout", "history-row-3"),
            Show("The new set is saved", "exercise-4-set-4"),
            Show("Dumbbell Incline Press replaced Plank, on top", "exercise-1", "Dumbbell Incline Press"),
            Click("No changes: back goes straight to History", "back"),
        ]);

    /// <summary>Discard prompt on an edit: cancel, then discard.</summary>
    public static readonly Scenario DiscardEdits = new(
        "Discard edits",
        "Back asks before dropping unsaved changes.",
        [
            Click("Open History", "history-link"),
            Click("Open the cardio workout", "history-row-2"),
            Enter("Treadmill distance: 4 km", "exercise-3-distance", "4"),
            Click("Back", "back"),
            Show("Discard changes?", "confirm-dialog"),
            Click("Cancel keeps editing", "confirm-dialog-cancel"),
            Back("Android back asks again"),
            Click("Discard", "confirm-dialog-confirm"),
            Show("Back on History", "history-row-2"),
            Click("Reopen the workout", "history-row-2"),
            Show("The distance is unchanged", "exercise-3-distance"),
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

    /// <summary>An unhandled error shows the Error screen; Restart goes back to the Dashboard.</summary>
    public static readonly Scenario RecoverFromError = new(
        "Recover from an error",
        "An unexpected error shows the Error screen; Restart goes back to the Dashboard.",
        [
            Click("Open History", "history-link"),
            Click("Simulate an error", "simulate-error"),
            Show("Something went wrong.", "error-message", "Something went wrong."),
            Click("Restart", "error-restart"),
            Show("Back on the Dashboard", "last-workout"),
        ]);
}
