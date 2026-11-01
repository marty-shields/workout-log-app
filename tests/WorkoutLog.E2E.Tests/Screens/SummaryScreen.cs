namespace WorkoutLog.E2E.Tests.Screens;

/// <summary>The Workout Summary shown after saving a new workout.</summary>
public sealed class SummaryScreen(WebView web)
{
    public const string Title = "Workout saved";

    /// <summary>Waits until the summary or the load error shows.</summary>
    public void WaitUntilLoaded() =>
        web.WaitUntil(() => web.Exists("summary-stats") || web.Exists("summary-error"), "the Workout Summary didn't load");

    public string Exercises() => web.Text("summary-exercises");

    public string Sets() => web.Text("summary-sets");

    public bool HasFirstWorkoutNote() => web.Exists("summary-first-workout");

    /// <summary>The text of the record row for the exercise, or null when it has none.</summary>
    public string? Record(int exerciseId) => web.Exists($"summary-record-{exerciseId}") ? web.Text($"summary-record-{exerciseId}") : null;

    /// <summary>The text of the exercise's row under Since last time, or null when it has none.</summary>
    public string? Change(int exerciseId) => web.Exists($"summary-change-{exerciseId}") ? web.Text($"summary-change-{exerciseId}") : null;

    public void Done() => web.Tap("done");
}
