using WorkoutLog.E2E.Tests.Device;
using WorkoutLog.E2E.Tests.Screens;

namespace WorkoutLog.E2E.Tests.Support;

/// <summary>What a user does across screens: start, log and save a workout, open Workouts, Exercises or a saved workout, go back.</summary>
public sealed class Journeys(
    WebView web, Native native, DeviceClock clock, ScenarioState state,
    ThisWeekScreen thisWeek, WorkoutScreen workout, AddExerciseScreen addExercise, WorkoutsScreen workouts, ExercisesScreen exercises,
    ConfirmDialog dialog, SummaryScreen summary, NavigationBar navigation)
{
    private static readonly TimeSpan SaveGrace = TimeSpan.FromSeconds(3);

    public enum Screen
    {
        ThisWeek,
        Workouts,
        Exercises,
        Settings,
        ImportReview,
        Summary,
        Workout,
        Other,
    }

    /// <summary>The screen shown, by its title or its Save button.</summary>
    public Screen Current()
    {
        var title = web.Text("screen-title");
        return title switch
        {
            ThisWeekScreen.Title => Screen.ThisWeek,
            WorkoutsScreen.Title => Screen.Workouts,
            ExercisesScreen.Title => Screen.Exercises,
            SettingsScreen.Title => Screen.Settings,
            ImportReviewScreen.Title => Screen.ImportReview,
            SummaryScreen.Title => Screen.Summary,
            _ when workout.IsShown() => Screen.Workout,
            _ => Screen.Other,
        };
    }

    /// <summary>Waits until <paramref name="screen"/> shows.</summary>
    public void WaitFor(Screen screen) => web.WaitUntil(() => Current() == screen, $"{screen} didn't show");

    public void StartNewWorkout()
    {
        WaitFor(Screen.ThisWeek);
        var from = clock.LocalNow();
        thisWeek.NewWorkout();
        WaitFor(Screen.Workout);
        state.Started = (from, clock.LocalNow());
    }

    /// <summary>Opens the add-exercise list and taps the exercise; a strength exercise already in the workout gets a set.</summary>
    public void AddExercise(string name)
    {
        workout.OpenAddExercise();
        web.WaitUntil(addExercise.IsLoaded, "the add-exercise list didn't open");
        addExercise.Pick(Exercises.Named(name).Id);
        web.WaitUntil(() => !addExercise.IsOpen(), "the add-exercise list didn't close");
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

    public void LogAll(ExerciseValues exercise)
    {
        for (var i = 0; i < exercise.Entries.Count; i++)
        {
            Log(exercise.Name, i + 1, exercise.Entries[i]);
        }

        if (exercise.Note is not null)
        {
            workout.EnterNote(exercise.Name, exercise.Note);
        }
    }

    /// <summary>
    /// Taps Save. A new workout opens the Workout Summary, which Done dismisses. A save that is blocked leaves the workout showing,
    /// so this waits a few seconds for the screen to change and then carries on.
    /// </summary>
    public void Save()
    {
        workout.Save();
        var deadline = DateTime.UtcNow + SaveGrace;
        web.WaitUntil(() => Current() != Screen.Workout || DateTime.UtcNow > deadline, "the screen didn't settle after Save");
        web.Settle();
        if (Current() == Screen.Summary)
        {
            summary.Done();
        }
    }

    /// <summary>Saves a valid workout, dismisses the Workout Summary a new one opens and waits for This week.</summary>
    public void SaveAndWait()
    {
        workout.Save();
        web.WaitUntil(() => Current() is Screen.Summary or Screen.ThisWeek, "neither the Workout Summary nor This week showed");
        if (Current() == Screen.Summary)
        {
            summary.Done();
        }

        WaitFor(Screen.ThisWeek);
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

    /// <summary>Goes to Workouts from This week, Workouts, Exercises or an unchanged workout.</summary>
    public void GoToWorkouts()
    {
        switch (Current())
        {
            case Screen.ThisWeek or Screen.Exercises:
                navigation.Open("Workouts");
                break;
            case Screen.Workout:
                workout.Back();
                break;
        }

        WaitFor(Screen.Workouts);
        workouts.Rows();
    }

    /// <summary>Goes to Exercises from This week and waits for it to load.</summary>
    public void GoToExercises()
    {
        WaitFor(Screen.ThisWeek);
        navigation.Open("Exercises");
        WaitFor(Screen.Exercises);
        exercises.WaitUntilLoaded();
    }

    /// <summary>Goes to This week from Workouts or Exercises, or from an unchanged workout through Workouts.</summary>
    public void GoToThisWeek()
    {
        if (Current() == Screen.Workout)
        {
            GoToWorkouts();
        }

        if (Current() is Screen.Workouts or Screen.Exercises)
        {
            navigation.Open("Dashboard");
        }

        WaitFor(Screen.ThisWeek);
        thisWeek.WaitUntilLoaded();
    }

    /// <summary>Opens a saved workout from Workouts and waits for it to load.</summary>
    public void OpenWorkout(int id)
    {
        GoToWorkouts();
        workouts.Open(id);
        WaitForWorkout();
    }

    /// <summary>Opens the top row in Workouts.</summary>
    public void OpenLatestWorkout()
    {
        GoToWorkouts();
        workouts.Open(workouts.RowIds()[0]);
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

    /// <summary>Delete, then Delete on the prompt; waits for Workouts.</summary>
    public void Delete()
    {
        workout.Delete();
        web.WaitUntil(dialog.IsOpen, "the prompt didn't show");
        dialog.Confirm();
        WaitFor(Screen.Workouts);
    }

    private void WaitForWorkout()
    {
        WaitFor(Screen.Workout);
        web.WaitUntil(() => workout.ExerciseNames().Count > 0 || workout.ShowsNoExercises(), "the workout didn't load");
    }
}
