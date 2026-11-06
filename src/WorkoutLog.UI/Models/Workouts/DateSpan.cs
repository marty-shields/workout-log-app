namespace WorkoutLog.UI.Models.Workouts;

/// <summary>Days the Workouts list shows, both ends included.</summary>
/// <param name="From">First day. Never after <paramref name="To"/>.</param>
/// <param name="To">Last day.</param>
public readonly record struct DateSpan(DateOnly From, DateOnly To)
{
    /// <summary>True when the span is a single day.</summary>
    public bool IsOneDay => From == To;

    /// <summary>True when <paramref name="day"/> is in the span.</summary>
    public bool Contains(DateOnly day) => day >= From && day <= To;
}
