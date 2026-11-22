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

    /// <summary>The dates shown in the date field, e.g. "2 Nov – 8 Nov 2026".</summary>
    public string Dates() => web.Text("workouts-dates-dates");

    public void Back() => web.Tap("back");

    public void Open(int workoutId) => web.Tap($"workouts-row-{workoutId}");

    /// <summary>Taps a quick range's segment, e.g. 1y.</summary>
    public void Range(string key) => web.Find($"workouts-range-{key}").FindElement(By.XPath("..")).Click();

    /// <summary>Picks one day in the calendar: its year and month from the month grid, then the day twice, then Show workouts.</summary>
    public void PickDay(DateTime day)
    {
        web.Tap("workouts-dates");
        web.Tap("workouts-dates-calendar-month-name");
        while (int.Parse(web.Text("workouts-dates-calendar-year"), CultureInfo.InvariantCulture) > day.Year)
        {
            web.Tap("workouts-dates-calendar-prev-year");
        }

        web.Tap($"workouts-dates-calendar-month-{day:yyyy-MM}");
        web.Tap($"workouts-dates-calendar-day-{day:yyyy-MM-dd}");
        web.Tap($"workouts-dates-calendar-day-{day:yyyy-MM-dd}");
        web.Tap("workouts-dates-calendar-apply");
    }

    private const string RowSelector = "[data-testid^=\"workouts-row-\"]";
}
