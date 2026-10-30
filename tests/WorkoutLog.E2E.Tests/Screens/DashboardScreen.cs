using OpenQA.Selenium;

namespace WorkoutLog.E2E.Tests.Screens;

/// <summary>The Dashboard: last workout, counts, Workouts, Exercises, New Workout and the Settings gear.</summary>
public sealed class DashboardScreen(WebView web)
{
    public const string Title = "Workout Log";

    /// <summary>The last workout's date, or "No workouts yet", once loaded.</summary>
    public string LastWorkout()
    {
        string? text = null;
        web.WaitUntil(() => (text = Read("last-workout-date") ?? Read("last-workout-empty")) is not null, "the Dashboard didn't load");
        return text!;
    }

    /// <summary>The last workout's days since and duration, e.g. 3 days ago · 1:12:00.</summary>
    public string LastWorkoutWhen() => web.Text("last-workout-when");

    /// <summary>The last workout's exercises, e.g. Barbell Bench Press, Cable Fly +1.</summary>
    public string LastWorkoutExercises() => web.Text("last-workout-exercises");

    public void OpenLastWorkout() => web.Tap("last-workout");

    public string ThisWeek() => Count("count-week");

    public string ThisMonth() => Count("count-month");

    public bool HasWorkoutsLink() => web.Exists("workouts-link");

    public bool HasExercisesLink() => web.Exists("exercises-link");

    public bool HasNewWorkout() => web.Exists("new-workout");

    public bool HasStartupError() => web.Exists("startup-error");

    public void OpenWorkouts() => web.Tap("workouts-link");

    public void OpenExercises() => web.Tap("exercises-link");

    public void OpenSettings() => web.Tap("settings");

    public void NewWorkout() => web.Tap("new-workout");

    private string Count(string testId) => web.Find(testId).FindElement(By.CssSelector(".value")).Text.Trim();

    private string? Read(string testId) => web.Exists(testId) ? web.Text(testId) : null;
}
