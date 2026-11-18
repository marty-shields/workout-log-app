namespace WorkoutLog.E2E.Tests.Screens;

/// <summary>The navigation bar on This week, Workouts and Exercises. A destination is "Dashboard", "Workouts" or "Exercises".</summary>
public sealed class NavigationBar(WebView web)
{
    public void Open(string destination) => web.Tap(TestId(destination));

    public bool Has(string destination) => web.Exists(TestId(destination));

    private static string TestId(string destination) => $"{destination.ToLowerInvariant()}-link";
}
