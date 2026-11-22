using Shields.UI.Components.Inputs;
using WorkoutLog.UI.Models.Fields;
using WorkoutLog.UI.Models.Workout;

namespace WorkoutLog.UI.Components.Workout;

/// <summary>Which set layout the exercise card uses: a panel or a row, and where last time shows.</summary>
internal static class SetGrid
{
    public static bool OpensAsPanel(IReadOnlyList<FieldSpec> fields) =>
        fields.Count > 2 && fields.Any(f => f.Kind == FieldKind.Duration);

    /// <summary>
    /// Whether a last-time text is too wide for the kit's wide aside column (<see cref="EntryGridAsideWidth.Wide"/>, 6rem, 96px) at the narrowest card:
    /// more than 19 half digit widths (see <see cref="HalfDigits"/>). Measured in Chrome (one digit width 9.1px): 57.25kg×12 is 86.6px and fits;
    /// 102.25kg×12 is 95.5px, which leaves no room for a wider phone font, so it goes below.
    /// </summary>
    internal static bool TooWideForColumn(string text) => HalfDigits(text) > 19;

    /// <summary>
    /// The aside width for the exercise's header and rows: <see cref="EntryGridAsideWidth.Normal"/> (3.5rem, 56px) while every set's text fits it
    /// (11 half digit widths: 62.5×6 is 51.8px), else <see cref="EntryGridAsideWidth.Wide"/>. Wide leaves each of two fields 33px at the narrowest card,
    /// room for only two characters (100 and 62.5 scroll), so it is used only when a text needs it (140×10, 25kg×10, 57.25kg×12).
    /// </summary>
    public static EntryGridAsideWidth AsideWidth(LastTime? last) =>
        last?.Entries.Any(text => HalfDigits(text) > 11) == true ? EntryGridAsideWidth.Wide : EntryGridAsideWidth.Normal;

    /// <summary>A text's width in half digit widths: a digit, × or letter is 2, a dot 1.</summary>
    internal static int HalfDigits(string text) => text.Sum(c => c == '.' ? 1 : 2);

    /// <summary>
    /// True when last time shows on a line under each row, not in the Last column: a duration beside another field leaves no room for the column,
    /// and neither does any set's text that is too wide for it (102.25kg×12). Decided once for the exercise, so rows and header never differ.
    /// </summary>
    public static bool LastBelow(IReadOnlyList<FieldSpec> fields, LastTime? last) => HasDuration(fields) || last?.Entries.Any(TooWideForColumn) == true;

    /// <summary>Whether the set header and rows have a Last column: there is last time and <see cref="LastBelow"/> is false.</summary>
    public static bool LastColumn(IReadOnlyList<FieldSpec> fields, LastTime? last) => last is not null && !LastBelow(fields, last);

    /// <summary>True when a header label has no room for its unit: the Last column or a duration beside another field leaves a field one narrow column at the narrowest card. The field's own label still names the unit.</summary>
    public static bool ShortHeaders(IReadOnlyList<FieldSpec> fields, bool lastColumn) => lastColumn || HasDuration(fields);

    public static bool HasDuration(IReadOnlyList<FieldSpec> fields) => fields.Any(f => f.Kind == FieldKind.Duration);
}
