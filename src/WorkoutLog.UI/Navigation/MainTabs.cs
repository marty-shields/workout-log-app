namespace WorkoutLog.UI.Navigation;

/// <summary>Where the tabs of the tab bar lead.</summary>
public static class MainTabs
{
    /// <summary>Back from another tab goes here; back from here leaves the app.</summary>
    public const MainTab Start = MainTab.ThisWeek;

    /// <summary>The App route of <paramref name="tab"/>; empty for This week.</summary>
    public static string Route(MainTab tab) => tab == MainTab.Trends ? "trends" : string.Empty;
}
