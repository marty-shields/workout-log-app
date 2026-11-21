using Shields.UI.Components.Actions;
using Shields.UI.Components.Layout;

namespace WorkoutLog.UI.Navigation;

/// <summary>Where the tabs of the bottom navigation bar lead, and the bar's items.</summary>
public static class MainTabs
{
    /// <summary>Back from another tab goes here; back from here leaves the app.</summary>
    public const MainTab Start = MainTab.ThisWeek;

    /// <summary>The words under the raised button of the bar.</summary>
    public const string AddLabel = "New";

    /// <summary>The icon in the raised button of the bar.</summary>
    public const IconName AddIcon = IconName.Plus;

    private static readonly BottomNavItem ThisWeekItem = new("This week", IconName.Calendar, "this-week-link");
    private static readonly BottomNavItem TrendsItem = new("Trends", IconName.BarChart, "trends-link");

    /// <summary>The bar's items in order, <see cref="Start"/> first as the kit's AppScreen takes Android back to the first.</summary>
    public static IReadOnlyList<BottomNavItem> Items { get; } = [ThisWeekItem, TrendsItem];

    /// <summary>The bar's item of <paramref name="tab"/>; the kit's <c>BottomNav</c> matches its <c>Current</c> to the items by value equality.</summary>
    public static BottomNavItem Item(MainTab tab) => tab switch
    {
        MainTab.ThisWeek => ThisWeekItem,
        MainTab.Trends => TrendsItem,
        _ => throw new ArgumentOutOfRangeException(nameof(tab), tab, null),
    };

    /// <summary>The tab of <paramref name="item"/>, one of <see cref="Items"/>.</summary>
    public static MainTab Tab(BottomNavItem item) =>
        item == ThisWeekItem ? MainTab.ThisWeek
        : item == TrendsItem ? MainTab.Trends
        : throw new ArgumentOutOfRangeException(nameof(item), item, null);

    /// <summary>The App route of <paramref name="tab"/>; empty for This week.</summary>
    public static string Route(MainTab tab) => tab == MainTab.Trends ? "trends" : string.Empty;
}
