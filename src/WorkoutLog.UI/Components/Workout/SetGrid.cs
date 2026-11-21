using WorkoutLog.UI.Models.Fields;

namespace WorkoutLog.UI.Components.Workout;

/// <summary>Set layout shared by the set header and set rows.</summary>
internal static class SetGrid
{
    public static bool OpensAsPanel(IReadOnlyList<FieldSpec> fields) =>
        fields.Count > 2 && fields.Any(f => f.Kind == FieldKind.Duration);

    /// <summary>True when last time shows on a line under the row: a duration beside another field leaves no room for a Last column.</summary>
    public static bool LastBelow(IReadOnlyList<FieldSpec> fields) => fields.Any(f => f.Kind == FieldKind.Duration);

    /// <summary>Whether the set header and rows have a Last column.</summary>
    public static bool LastColumn(IReadOnlyList<FieldSpec> fields, bool hasLast) => hasLast && !LastBelow(fields);

    /// <summary>True when a header label has no room for its unit: the Last column or a wide duration column leaves a field one narrow column at the narrowest card. The field's own label still names the unit.</summary>
    public static bool ShortHeaders(IReadOnlyList<FieldSpec> fields, bool hasLast) => hasLast || LastBelow(fields);

    /// <summary>CSS grid-template-columns: set number, the Last column when <see cref="LastColumn"/>, one column per field, remove button.</summary>
    public static string Columns(IReadOnlyList<FieldSpec> fields, bool hasLast)
    {
        var lastColumn = LastColumn(fields, hasLast);
        // A duration takes 3fr: the kit's three boxes pad 4px a side, so at the narrowest card 2fr clips mm and ss.
        var fieldColumns = fields.Select(field => field.Kind == FieldKind.Duration ? "minmax(0, 3fr)" : "minmax(0, 1fr)");
        var last = lastColumn ? "var(--set-last-width) " : null;
        return $"var(--set-number-width) {last}{string.Join(' ', fieldColumns)} var(--touch-target)";
    }
}
