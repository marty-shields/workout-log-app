using WorkoutLog.E2E.Tests.Device;
using WorkoutLog.E2E.Tests.Screens;

namespace WorkoutLog.E2E.Tests.Support;

/// <summary>What a user does across screens: start, log and save a workout, open History or a saved workout, go back.</summary>
public sealed class Journeys(
    WebView web, Native native, DeviceClock clock, ScenarioState state,
    DashboardScreen dashboard, WorkoutScreen workout, PickerScreen picker, HistoryScreen history, ConfirmDialog dialog)
{
    public enum Screen
    {
        Dashboard,
        History,
        Settings,
        ImportReview,
        Workout,
        Other,
    }

    /// <summary>The screen shown, by its title or its Save button.</summary>
    public Screen Current()
    {
        var title = web.Text("screen-title");
        return title switch
        {
            DashboardScreen.Title => Screen.Dashboard,
            HistoryScreen.Title => Screen.History,
            SettingsScreen.Title => Screen.Settings,
            ImportReviewScreen.Title => Screen.ImportReview,
            _ when workout.IsShown() => Screen.Workout,
            _ => Screen.Other,
        };
    }

    /// <summary>Waits until <paramref name="screen"/> shows.</summary>
    public void WaitFor(Screen screen) => web.WaitUntil(() => Current() == screen, $"{screen} didn't show");

    public void StartNewWorkout()
    {
        WaitFor(Screen.Dashboard);
        var from = clock.LocalNow();
        dashboard.NewWorkout();
        WaitFor(Screen.Workout);
        state.Started = (from, clock.LocalNow());
    }

    /// <summary>Opens the picker and taps the exercise; a strength exercise already in the workout gets a set (D42).</summary>
    public void AddExercise(string name)
    {
        workout.OpenPicker();
        web.WaitUntil(picker.IsOpen, "the picker didn't open");
        picker.Pick(Exercises.Named(name).Id);
        web.WaitUntil(() => !picker.IsOpen(), "the picker didn't close");
        state.LastAdded = name;
    }

    /// <summary>
    /// Types the values into a set (strength) or the entry (cardio). The exercise is added first if it isn't in the workout,
    /// and Add set is tapped for the set after the last.
    /// </summary>
    public void Log(string name, int set, EntryValues values)
    {
        if (!workout.Has(name))
        {
            AddExercise(name);
        }

        var cardio = WorkoutScreen.IsCardio(name);
        if (!cardio && set > workout.SetCount(name))
        {
            workout.AddSet(name);
            web.Eventually(() => workout.SetCount(name), Is.EqualTo(set), $"{name} sets");
        }

        if (!cardio && workout.IsSummary(name, set))
        {
            workout.OpenSummary(name, set);
        }

        workout.EnterAll(name, cardio ? null : set, values.Values);
        state.RecordLogged(name, set, values.Values);
    }

    /// <summary>Logs each set in turn.</summary>
    public void LogAll(ExerciseValues exercise)
    {
        for (var i = 0; i < exercise.Entries.Count; i++)
        {
            Log(exercise.Name, i + 1, exercise.Entries[i]);
        }
    }

    public void Save() => workout.Save();

    /// <summary>Saves a valid workout and waits for the Dashboard.</summary>
    public void SaveAndWait()
    {
        workout.Save();
        WaitFor(Screen.Dashboard);
    }

    /// <summary>Starts, logs and saves a workout.</summary>
    public void SaveWorkout(IEnumerable<ExerciseValues> exercises)
    {
        StartNewWorkout();
        foreach (var exercise in exercises)
        {
            LogAll(exercise);
        }

        native.HideKeyboard();
        SaveAndWait();
    }

    /// <summary>Goes to History from the Dashboard, History or an unchanged workout.</summary>
    public void GoToHistory()
    {
        switch (Current())
        {
            case Screen.Dashboard:
                dashboard.OpenHistory();
                break;
            case Screen.Workout:
                workout.Back();
                break;
        }

        WaitFor(Screen.History);
        history.Rows();
    }

    /// <summary>Goes to the Dashboard from History, or from an unchanged workout through History.</summary>
    public void GoToDashboard()
    {
        if (Current() == Screen.Workout)
        {
            GoToHistory();
        }

        if (Current() == Screen.History)
        {
            history.Back();
        }

        WaitFor(Screen.Dashboard);
        dashboard.LastWorkout();
    }

    /// <summary>Opens a saved workout from History and waits for it to load.</summary>
    public void OpenWorkout(int id)
    {
        GoToHistory();
        history.Open(id);
        WaitForWorkout();
    }

    /// <summary>Opens the top row in History.</summary>
    public void OpenLatestWorkout()
    {
        GoToHistory();
        history.Open(history.RowIds()[0]);
        WaitForWorkout();
    }

    /// <summary>Goes back: "the back arrow" or "Android back" (the back key).</summary>
    public void Back(string how)
    {
        switch (how)
        {
            case "the back arrow":
                web.Tap("back");
                break;
            case "Android back":
                native.BackKey();
                break;
            default:
                throw new ArgumentException($"Unknown way back: {how}.", nameof(how));
        }
    }

    /// <summary>Back arrow, then Discard on the prompt.</summary>
    public void Discard()
    {
        web.Tap("back");
        web.WaitUntil(dialog.IsOpen, "the prompt didn't show");
        dialog.Confirm();
    }

    /// <summary>Delete, then Delete on the prompt; waits for History.</summary>
    public void Delete()
    {
        workout.Delete();
        web.WaitUntil(dialog.IsOpen, "the prompt didn't show");
        dialog.Confirm();
        WaitFor(Screen.History);
    }

    private void WaitForWorkout()
    {
        WaitFor(Screen.Workout);
        web.WaitUntil(() => workout.ExerciseNames().Count > 0 || workout.ShowsNoExercises(), "the workout didn't load");
    }
}
