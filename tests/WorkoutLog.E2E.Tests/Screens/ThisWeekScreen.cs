using System.Text.RegularExpressions;
using OpenQA.Selenium;

namespace WorkoutLog.E2E.Tests.Screens;

/// <summary>This week: the day strip, the tiles against last week, the week's workouts, records and sets per muscle group and the Settings gear.</summary>
public sealed partial class ThisWeekScreen(WebView web)
{
    public const string Title = "Workout Log";

    /// <summary>Waits until the week or the load error shows.</summary>
    public void WaitUntilLoaded() =>
        web.WaitUntil(() => web.Exists("week-tiles") || web.Exists("this-week-error"), "This week didn't load");

    /// <summary>Each of the week's workouts in order, newest first: when it started, its exercises and how long it took, e.g. (Wed 18 Nov · 07:10, Squat, Leg press +1, 0:58).</summary>
    public IReadOnlyList<(string Started, string Exercises, string Duration)> Workouts()
    {
        WaitUntilLoaded();
        return web.FindAll("[data-testid^=\"week-workouts-row-\"]")
            .Select(r => (Part(r, ".text"), Part(r, ".detail"), Part(r, ".aside")))
            .ToList();
    }

    /// <summary>The workouts card's message when the week has none; null while it has rows.</summary>
    public string? NoWorkoutsText()
    {
        WaitUntilLoaded();
        return Read("week-workouts-none");
    }

    /// <summary>Taps the newest workout.</summary>
    public void OpenNewestWorkout()
    {
        WaitUntilLoaded();
        web.Tap(web.FindAll("[data-testid^=\"week-workouts-row-\"]").First().GetDomAttribute("data-testid")!);
    }

    /// <summary>A tile, e.g. "workouts": its value and its change as screen readers hear it, e.g. "down 1".</summary>
    public (string Value, string Change) Tile(string tile) =>
        (web.Find($"week-tiles-{tile}").FindElement(By.CssSelector(".value")).Text.Trim(), Content(web.Find($"week-tiles-{tile}-change")).Replace("—", "", StringComparison.Ordinal).Trim());

    /// <summary>The weekdays the strip marks as trained, e.g. "Monday".</summary>
    public IReadOnlyList<string> TrainedDays() =>
        web.FindAll("li[data-testid^=\"day-strip-\"]")
            .Where(d => (d.GetDomAttribute("class") ?? "").Split(' ').Contains("done"))
            .Select(d => d.GetDomAttribute("data-testid")!["day-strip-".Length..])
            .Select(day => char.ToUpperInvariant(day[0]) + day[1..])
            .ToList();

    /// <summary>The streak, e.g. "6-week streak", or null when hidden.</summary>
    public string? Streak() => Read("this-week-streak");

    /// <summary>The Last week row's totals, e.g. "1 workout · 25m · 1 set", or null when the row isn't shown.</summary>
    public string? LastWeek() => Read("last-week-row");

    /// <summary>Each muscle group's name and sets, in order, once loaded.</summary>
    public IReadOnlyList<(string Group, string Sets)> MuscleRows()
    {
        WaitUntilLoaded();
        return web.FindAll("li[data-testid^=\"week-muscles-\"]")
            .Select(r => (Part(r, ".name"), Part(r, ".sets")))
            .ToList();
    }

    public bool HasStartupError() => web.Exists("startup-error");

    public void OpenSettings() => web.Tap("settings");

    /// <summary>Taps the Last week row, which opens last week in Trends.</summary>
    public void OpenLastWeek() => web.Tap("last-week-row");

    /// <summary>Each Records this week row's text and the weekday it was set, in order, once the card shows.</summary>
    public IReadOnlyList<(string Record, string Day)> Records()
    {
        WaitForRecords();
        return web.FindAll("[data-testid^=\"week-records-row-\"]")
            .Select(r => (Part(r, ".text"), Part(r, ".detail")))
            .ToList();
    }

    /// <summary>The Records this week card's message when it has none; null while it has rows.</summary>
    public string? NoRecordsText()
    {
        WaitForRecords();
        return Read("week-records-none");
    }

    /// <summary>The Records this week card's "n more" line; null when every record shows.</summary>
    public string? MoreRecordsText()
    {
        WaitForRecords();
        return Read("week-records-more");
    }

    /// <summary>Taps the Records this week row of the exercise.</summary>
    public void OpenRecord(int exerciseId) => web.Tap($"week-records-row-{exerciseId}");

    private void WaitForRecords() => web.WaitUntil(() => web.Exists("week-records"), "the Records this week card didn't show");

    private static string Part(IWebElement row, string selector) => Content(row.FindElement(By.CssSelector(selector)));

    private static string Content(IWebElement element) => Whitespace().Replace(element.GetDomProperty("textContent") ?? "", " ").Trim();

    private string? Read(string testId) => web.Exists(testId) ? web.Text(testId) : null;

    [GeneratedRegex(@"\s+")]
    private static partial Regex Whitespace();
}
