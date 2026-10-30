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

    /// <summary>Each Recent records row's text and how long ago, in order, once the card shows.</summary>
    public IReadOnlyList<(string Record, string When)> Records()
    {
        web.WaitUntil(() => web.Exists("recent-records"), "the Recent records card didn't show");
        return web.FindAll("[data-testid^=\"recent-records-row-\"]")
            .Select(r => (r.FindElement(By.CssSelector(".text")).Text.Trim(), r.FindElement(By.CssSelector(".detail")).Text.Trim()))
            .ToList();
    }

    /// <summary>The Recent records card's message when it has none; null while it has rows.</summary>
    public string? NoRecordsText()
    {
        web.WaitUntil(() => web.Exists("recent-records"), "the Recent records card didn't show");
        return Read("recent-records-none");
    }

    /// <summary>Taps the Recent records row of the exercise.</summary>
    public void OpenRecord(int exerciseId) => web.Tap($"recent-records-row-{exerciseId}");

    private string Count(string testId) => web.Find(testId).FindElement(By.CssSelector(".value")).Text.Trim();

    private string? Read(string testId) => web.Exists(testId) ? web.Text(testId) : null;
}
