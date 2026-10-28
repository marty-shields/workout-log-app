using WorkoutLog.Core.Domain.Workouts;

namespace WorkoutLog.Core.Domain.Exercises.Progress;

/// <summary>Longest distance and duration, fastest (lowest) pace, most floors and calories.</summary>
internal sealed class CardioSessionRule : ICardioSessionRule
{
    /// <inheritdoc />
    public CardioBest Best(IReadOnlyList<CardioEntry> entries) => new(
        entries.Max(e => e.Distance),
        entries.Max(e => e.Duration),
        entries.Min(e => e.Pace),
        entries.Max(e => e.Floors),
        entries.Max(e => e.Calories));
}
