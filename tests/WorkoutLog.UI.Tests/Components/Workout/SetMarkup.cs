namespace WorkoutLog.UI.Tests.Components.Workout;

/// <summary>Selectors for the three looks of a set, which are kit components: the open row is a kit EntryGridRow (a grid with inline columns), a closed set a kit ListRow, an open panel a selected kit Card.</summary>
internal static class SetMarkup
{
    public const string OpenRow = ".row[style][data-testid]:not(.header)";

    public const string Header = ".row.header";

    public const string Summary = ".row[data-testid]:not([style])";

    public const string Panel = ".card.selected";

    public const string AnyLast = "[data-testid$=-last]";

    public const string RowLast = ".row[style][data-testid]:not(.header) .aside";
}
