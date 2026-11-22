using WorkoutLog.UI.Models.Fields;
using WorkoutLog.UI.Models.Workout;

namespace WorkoutLog.UI.Components.Workout;

/// <summary>Which set layout the exercise card uses: a panel or a row, and where last time shows.</summary>
internal static class SetGrid
{
    public static bool OpensAsPanel(IReadOnlyList<FieldSpec> fields) =>
        fields.Count > 2 && fields.Any(f => f.Kind == FieldKind.Duration);

    /// <summary>
    /// Whether a last-time text is too wide for the kit's aside column (<c>--entry-grid-aside-width</c>, 3.5rem, 56px) at the narrowest card.
    /// Width in half digit widths: a digit, × or letter is 2, a dot 1; more than 11 (5.5 digits) is too wide. Measured in Chrome with the wide system font
    /// (one digit width 9.4px): 62.5×6 is 51.8px and fits, 140×10 is 56.3px, 5kg×10 is 55.5px and 102.5×8 is 60.7px, so they go below.
    /// </summary>
    internal static bool TooWideForColumn(string text) => text.Sum(c => c == '.' ? 1 : 2) > 11;

    /// <summary>
    /// True when last time shows on a line under each row, not in the Last column: a duration beside another field leaves no room for the column,
    /// and neither does any set's text that is too wide for it (an assisted 57.25kg×12). Decided once for the exercise, so rows and header never differ.
    /// </summary>
    public static bool LastBelow(IReadOnlyList<FieldSpec> fields, LastTime? last) => HasDuration(fields) || last?.Entries.Any(TooWideForColumn) == true;

    /// <summary>Whether the set header and rows have a Last column: there is last time and <see cref="LastBelow"/> is false.</summary>
    public static bool LastColumn(IReadOnlyList<FieldSpec> fields, LastTime? last) => last is not null && !LastBelow(fields, last);

    /// <summary>True when a header label has no room for its unit: the Last column or a duration beside another field leaves a field one narrow column at the narrowest card. The field's own label still names the unit.</summary>
    public static bool ShortHeaders(IReadOnlyList<FieldSpec> fields, bool lastColumn) => lastColumn || HasDuration(fields);

    public static bool HasDuration(IReadOnlyList<FieldSpec> fields) => fields.Any(f => f.Kind == FieldKind.Duration);
}
