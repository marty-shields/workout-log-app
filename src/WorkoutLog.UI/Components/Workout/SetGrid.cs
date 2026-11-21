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
        // TEMP (epic #262): remove when the kit's compact NumberField lands. With a Last column a decimal like 62.5 gets 3fr to a whole number's 2fr, so it is not clipped at the narrowest card.
        var fieldColumns = fields.Select(field => field.Kind switch
        {
            FieldKind.Duration => "minmax(0, 2fr)",
            FieldKind.Decimal when lastColumn => "minmax(0, 3fr)",
            _ when lastColumn => "minmax(0, 2fr)",
            _ => "minmax(0, 1fr)",
        });
        var last = lastColumn ? "var(--set-last-width) " : null;
        return $"var(--set-number-width) {last}{string.Join(' ', fieldColumns)} var(--touch-target)";
    }
}
