using OpenQA.Selenium;

namespace WorkoutLog.E2E.Tests.Screens;

/// <summary>
/// Trends: the Week, Month and Year views of one period, stepped with the arrows. A day, week row or bar with workouts opens them in Workouts;
/// a record or Most trained row opens its Exercise page.
/// </summary>
public sealed class TrendsScreen(WebView web)
{
    public const string Title = "Trends";

    private const int MaxSteps = 40;

    /// <summary>Waits until the period's cards or the load error show.</summary>
    public void WaitUntilLoaded() =>
        web.WaitUntil(() => web.Exists("trends-tiles") || web.Exists("trends-error"), "Trends didn't load");

    /// <summary>The period's name in the stepper, e.g. "9 – 15 Nov", "November 2026" or "2026".</summary>
    public string Label() => web.Text("trends-stepper-label");

    /// <summary>The view shown: "Week", "Month" or "Year".</summary>
    public string Kind() =>
        new[] { "Week", "Month", "Year" }.Single(k => web.Find($"trends-{k.ToLowerInvariant()}").Selected);

    /// <summary>Taps a view's segment: "Week", "Month" or "Year". The radio inside it is hidden, so its label is tapped.</summary>
    public void ChooseKind(string kind)
    {
        web.Find($"trends-{kind.ToLowerInvariant()}").FindElement(By.XPath("..")).Click();
        web.WaitUntil(() => Kind() == kind && !web.Exists("trends-loading"), $"the {kind} view didn't show");
    }

    public void Previous() => web.Tap("trends-stepper-previous");

    /// <summary>Taps the previous arrow <paramref name="times"/> times, waiting for the period to change each time.</summary>
    public void StepBack(int times)
    {
        for (var i = 0; i < times; i++)
        {
            var before = Label();
            Previous();
            web.WaitUntil(() => Label() != before, "the period didn't change");
        }
    }

    public void OpenSettings() => web.Tap("settings");

    public void Next() => web.Tap("trends-stepper-next");

    public bool CanGoPrevious() => web.Find("trends-stepper-previous").Enabled;

    public bool CanGoNext() => web.Find("trends-stepper-next").Enabled;

    /// <summary>The workouts tile's value, e.g. "4".</summary>
    public string Workouts() => web.Find("trends-tiles-workouts").FindElement(By.CssSelector(".value")).Text.Trim();

    /// <summary>Taps a day of the month in the calendar; only a day with a workout is tappable.</summary>
    public void OpenDay(DateOnly day) => web.Tap($"trends-calendar-days-day-{day:yyyy-MM-dd}");

    /// <summary>Taps a row of the Weeks card, counting from 1.</summary>
    public void OpenWeekRow(int row) => web.Tap($"trends-weeks-rows-select-{row - 1}");

    /// <summary>Taps a day's bar in the week view.</summary>
    public void OpenDayColumn(DateOnly day) => web.Tap($"trends-days-chart-bar-{day:yyyy-MM-dd}");

    /// <summary>Taps a month's bar in the year view, January being 1, of the year shown.</summary>
    public void OpenMonthColumn(int month) =>
        web.Tap($"trends-months-chart-bar-{int.Parse(Label(), System.Globalization.CultureInfo.InvariantCulture):0000}-{month:00}-01");

    /// <summary>The tappable day numbers of the month calendar.</summary>
    public IReadOnlyList<int> OpenDays() =>
        web.FindAll("button[data-testid^=\"trends-calendar-days-day-\"]")
            .Select(e => DateOnly.ParseExact(e.GetDomAttribute("data-testid")!["trends-calendar-days-day-".Length..], "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture).Day)
            .ToList();

    /// <summary>The tappable bars of the year view, January being 1.</summary>
    public IReadOnlyList<int> OpenMonthColumns() => Bars("trends-months-chart-").Select(day => day.Month).ToList();

    /// <summary>The tappable bars of the week view that draw this week's value; a day with only last week's time is a button that opens nothing.</summary>
    public IReadOnlyList<DateOnly> OpenDayColumns() => Bars("trends-days-chart-");

    /// <summary>Taps the Records row of the exercise.</summary>
    public void OpenRecord(int exerciseId) => web.Tap($"trends-records-row-{exerciseId}");

    /// <summary>Taps the Most trained row of the exercise, by name as the row's test id writes it.</summary>
    public void OpenMostTrained(string name) => web.Tap($"trends-most-trained-rows-select-{Slug(name)}");

    public bool HasMostTrained(string name) => web.Exists($"trends-most-trained-rows-select-{Slug(name)}");

    /// <summary>
    /// In the Year view, steps back from this year until a month with workouts shows, then taps its latest one, which opens Workouts.
    /// Fails when the years back to the first period have none.
    /// </summary>
    public void OpenAMonthWithWorkouts()
    {
        ChooseKind("Year");
        for (var step = 0; step < MaxSteps; step++)
        {
            WaitUntilLoaded();
            if (OpenMonthColumns().Count > 0)
            {
                OpenMonthColumn(OpenMonthColumns().Max());
                return;
            }

            if (!CanGoPrevious())
            {
                break;
            }

            var before = Label();
            Previous();
            web.WaitUntil(() => Label() != before, "the year didn't change");
        }

        throw new InvalidOperationException("No year has a month with workouts.");
    }

    /// <summary>In the Month view, steps back until the Most trained card lists the exercise, then returns; fails when no month back to the first has it.</summary>
    public void StepBackToMostTrained(string name)
    {
        ChooseKind("Month");
        for (var step = 0; step < MaxSteps; step++)
        {
            WaitUntilLoaded();
            if (HasMostTrained(name))
            {
                return;
            }

            if (!CanGoPrevious())
            {
                break;
            }

            var before = Label();
            Previous();
            web.WaitUntil(() => Label() != before, "the month didn't change");
        }

        throw new InvalidOperationException($"No month has {name} among its most trained exercises.");
    }

    /// <summary>The kit's row id for a label: lower case letters and digits, each run of other characters one dash.</summary>
    private static string Slug(string name) => System.Text.RegularExpressions.Regex.Replace(name.ToLowerInvariant(), "[^\\p{L}\\p{N}]+", "-").Trim('-');

    private IReadOnlyList<DateOnly> Bars(string prefix) =>
        web.FindAll($"[role=\"button\"][data-testid^=\"{prefix}bar-\"]")
            .Select(e => DateOnly.ParseExact(e.GetDomAttribute("data-testid")![$"{prefix}bar-".Length..], "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture))
            .Where(day => web.Exists($"{prefix}column-{day:yyyy-MM-dd}"))
            .ToList();
}
