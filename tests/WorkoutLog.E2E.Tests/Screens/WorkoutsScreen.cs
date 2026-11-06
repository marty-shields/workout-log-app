using System.Globalization;
using OpenQA.Selenium;

namespace WorkoutLog.E2E.Tests.Screens;

/// <summary>Workouts: one row per saved workout in the dates shown, newest first.</summary>
public sealed class WorkoutsScreen(WebView web)
{
    public const string Title = "Workouts";

    /// <summary>Row texts, in order, once loaded; empty when Workouts is empty.</summary>
    public IReadOnlyList<string> Rows()
    {
        web.WaitUntil(() => web.Exists("workouts-empty") || web.FindAll(RowSelector).Count > 0, "Workouts didn't load");
        return web.FindAll($"{RowSelector} .text").Select(e => e.Text.Trim()).ToList();
    }

    /// <summary>Workout ids of the rows, in order, once loaded.</summary>
    public IReadOnlyList<int> RowIds()
    {
        Rows();
        return web.FindAll(RowSelector).Select(e => int.Parse(e.GetDomAttribute("data-testid")!["workouts-row-".Length..])).ToList();
    }

    public string? EmptyText() => web.Exists("workouts-empty") ? web.Text("workouts-empty-message") : null;

    public void Open(int workoutId) => web.Tap($"workouts-row-{workoutId}");

    /// <summary>Taps a quick range's segment, e.g. 1y.</summary>
    public void Range(string key) => web.Find($"workouts-range-{key}").FindElement(By.XPath("..")).Click();

    /// <summary>Picks one day in the calendar: its year and month from the month grid, then the day twice, then Show workouts.</summary>
    public void PickDay(DateTime day)
    {
        web.Tap("workouts-dates");
        web.Tap("range-month-name");
        while (int.Parse(web.Text("range-year"), CultureInfo.InvariantCulture) > day.Year)
        {
            web.Tap("range-prev-year");
        }

        web.Tap($"range-month-{day:yyyy-MM}");
        web.Tap($"range-day-{day:yyyy-MM-dd}");
        web.Tap($"range-day-{day:yyyy-MM-dd}");
        web.Tap("range-apply");
    }

    private const string RowSelector = "[data-testid^=\"workouts-row-\"]";
}
