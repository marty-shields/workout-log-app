namespace WorkoutLog.E2E.Tests.Screens;

/// <summary>History: one row per saved workout, newest first.</summary>
public sealed class HistoryScreen(WebView web)
{
    public const string Title = "History";

    /// <summary>Row texts, in order, once loaded; empty when History is empty.</summary>
    public IReadOnlyList<string> Rows()
    {
        web.WaitUntil(() => web.Exists("history-empty") || web.FindAll(RowSelector).Count > 0, "History didn't load");
        return web.FindAll($"{RowSelector} .text").Select(e => e.Text.Trim()).ToList();
    }

    /// <summary>Workout ids of the rows, in order, once loaded.</summary>
    public IReadOnlyList<int> RowIds()
    {
        Rows();
        return web.FindAll(RowSelector).Select(e => int.Parse(e.GetDomAttribute("data-testid")!["history-row-".Length..])).ToList();
    }

    public string? EmptyText() => web.Exists("history-empty") ? web.Text("history-empty") : null;

    public void Open(int workoutId) => web.Tap($"history-row-{workoutId}");

    public void Back() => web.Tap("back");

    private const string RowSelector = "[data-testid^=\"history-row-\"]";
}
