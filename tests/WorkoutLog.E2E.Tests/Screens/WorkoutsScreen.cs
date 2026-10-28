namespace WorkoutLog.E2E.Tests.Screens;

/// <summary>Workouts: one row per saved workout, newest first.</summary>
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

    public string? EmptyText() => web.Exists("workouts-empty") ? web.Text("workouts-empty") : null;

    public void Open(int workoutId) => web.Tap($"workouts-row-{workoutId}");

    public void Back() => web.Tap("back");

    private const string RowSelector = "[data-testid^=\"workouts-row-\"]";
}
