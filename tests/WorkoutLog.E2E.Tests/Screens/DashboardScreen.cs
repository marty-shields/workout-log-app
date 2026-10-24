using OpenQA.Selenium;

namespace WorkoutLog.E2E.Tests.Screens;

/// <summary>The Dashboard: last workout, counts, History and New Workout.</summary>
public sealed class DashboardScreen(WebView web)
{
    public const string Title = "Workout Log";

    /// <summary>The last workout's date, or "No workouts yet", once loaded.</summary>
    public string LastWorkout()
    {
        string? text = null;
        web.WaitUntil(() => (text = Read("last-workout") ?? Read("last-workout-empty")) is not null, "the Dashboard didn't load");
        return text!;
    }

    public string ThisWeek() => Count("count-week");

    public string ThisMonth() => Count("count-month");

    public bool HasHistoryLink() => web.Exists("history-link");

    public bool HasNewWorkout() => web.Exists("new-workout");

    public bool HasStartupError() => web.Exists("startup-error");

    public void OpenHistory() => web.Tap("history-link");

    public void NewWorkout() => web.Tap("new-workout");

    private string Count(string testId) => web.Find(testId).FindElement(By.CssSelector(".value")).Text.Trim();

    private string? Read(string testId) => web.Exists(testId) ? web.Text(testId) : null;
}
