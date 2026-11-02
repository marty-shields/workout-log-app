using WorkoutLog.UI.Models.Fields;

namespace WorkoutLog.UI.Components.Workout;

/// <summary>Set layout shared by the set header and set rows.</summary>
internal static class SetGrid
{
    /// <summary>True when a set doesn't fit one row: more than two fields, one of them a duration. Its sets show as summaries and one open panel.</summary>
    public static bool Collapses(IReadOnlyList<FieldSpec> fields) =>
        fields.Count > 2 && fields.Any(f => f.Kind == FieldKind.Duration);

    /// <summary>CSS grid-template-columns: set number, the Last column when <paramref name="hasLast"/>, one column per field, remove button.</summary>
    public static string Columns(IReadOnlyList<FieldSpec> fields, bool hasLast)
    {
        var fieldColumns = fields.Select(field => field.Kind switch
        {
            FieldKind.Duration => "minmax(0, 2fr)",
            _ => "minmax(0, 1fr)",
        });
        var last = hasLast ? "minmax(0, 0.7fr) " : null;
        return $"var(--set-number-width) {last}{string.Join(' ', fieldColumns)} var(--touch-target)";
    }
}
