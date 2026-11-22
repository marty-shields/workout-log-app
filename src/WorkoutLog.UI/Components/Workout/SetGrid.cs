using WorkoutLog.UI.Models.Fields;

namespace WorkoutLog.UI.Components.Workout;

/// <summary>Which set layout the exercise card uses: a panel or a row, and where last time shows.</summary>
internal static class SetGrid
{
    public static bool OpensAsPanel(IReadOnlyList<FieldSpec> fields) =>
        fields.Count > 2 && fields.Any(f => f.Kind == FieldKind.Duration);

    /// <summary>
    /// Longest last-time text that fits the kit's aside column (<c>--entry-grid-aside-width</c>, 3.5rem) at the narrowest card without overflowing:
    /// 62.5×6 fits, 25kg×10 does not. Measured in Chrome with the wide system font, so a phone's own font has room to spare.
    /// </summary>
    internal const int MaxColumnText = 6;

    /// <summary>
    /// True when last time shows on a line under the row, not in the Last column: a duration beside another field leaves no room for the column,
    /// and neither does a text longer than <see cref="MaxColumnText"/> (an assisted 57.25kg×12).
    /// </summary>
    public static bool LastBelow(IReadOnlyList<FieldSpec> fields, string? last) => HasDuration(fields) || last?.Length > MaxColumnText;

    /// <summary>Whether the set header and rows have a Last column: there is last time and <see cref="LastBelow"/> is false.</summary>
    public static bool LastColumn(IReadOnlyList<FieldSpec> fields, bool hasLast, string? last) => hasLast && !LastBelow(fields, last);

    /// <summary>True when a header label has no room for its unit: the Last column or a duration beside another field leaves a field one narrow column at the narrowest card. The field's own label still names the unit.</summary>
    public static bool ShortHeaders(IReadOnlyList<FieldSpec> fields, bool lastColumn) => lastColumn || HasDuration(fields);

    private static bool HasDuration(IReadOnlyList<FieldSpec> fields) => fields.Any(f => f.Kind == FieldKind.Duration);
}
