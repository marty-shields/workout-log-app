namespace WorkoutLog.UI.Models.Fields;

/// <summary>How a field's value is entered.</summary>
public enum FieldKind
{
    /// <summary>Whole number, on the numeric keyboard.</summary>
    Whole,

    /// <summary>Decimal number, on the decimal keyboard.</summary>
    Decimal,

    /// <summary>Duration in three boxes: h, mm, ss.</summary>
    Duration,

    /// <summary>Pace in two boxes: m, ss.</summary>
    Pace,
}
