using OpenQA.Selenium;

namespace WorkoutLog.E2E.Tests.Screens;

/// <summary>Exercises: one row per logged exercise, latest first.</summary>
public sealed class ExercisesScreen(WebView web)
{
    public const string Title = "Exercises";

    private const string RowSelector = "[data-testid^=\"exercises-row-\"]";

    /// <summary>Waits until the list, "No exercises logged yet" or the load error shows.</summary>
    public void WaitUntilLoaded() =>
        web.WaitUntil(() => web.Exists("exercises-search") || web.Exists("exercises-empty") || web.Exists("exercises-error"), "Exercises didn't load");

    /// <summary>Row names, in order, once loaded.</summary>
    public IReadOnlyList<string> Names()
    {
        WaitUntilLoaded();
        return web.FindAll($"{RowSelector} .text").Select(e => e.Text.Trim()).ToList();
    }

    /// <summary>The "Last …" line of each row, in order; empty for a row without one.</summary>
    public IReadOnlyList<string> Lasts()
    {
        WaitUntilLoaded();
        return web.FindAll(RowSelector).Select(r => r.FindElements(By.CssSelector(".detail")).SingleOrDefault()?.Text.Trim() ?? "").ToList();
    }

    /// <summary>The kind at the right of each row, in order.</summary>
    public IReadOnlyList<string> Kinds()
    {
        WaitUntilLoaded();
        return web.FindAll($"{RowSelector} .aside").Select(e => e.Text.Trim()).ToList();
    }

    /// <summary>Names of the rows that are buttons, in order.</summary>
    public IReadOnlyList<string> TappableNames()
    {
        WaitUntilLoaded();
        return web.FindAll($"button{RowSelector} .text").Select(e => e.Text.Trim()).ToList();
    }

    /// <summary>Taps the row of the exercise.</summary>
    public void Open(int exerciseId) => web.Tap($"exercises-row-{exerciseId}");

    public string? EmptyText() => web.Exists("exercises-empty") ? web.Text("exercises-empty") : null;

    public string? NoMatchText() => web.Exists("exercises-none") ? web.Text("exercises-none") : null;

    public void Search(string text) => web.Type("exercises-search", text);

    /// <summary>Taps a chip's label: the radio inside it is hidden.</summary>
    public void Filter(string kind) => web.Find($"exercises-filter-{kind.ToLowerInvariant()}").FindElement(By.XPath("..")).Click();

    public void Back() => web.Tap("back");
}
