using WorkoutLog.Core.Domain.Workouts;

namespace WorkoutLog.Core.Domain.Exercises.Progress;

/// <summary>The entry's distance, duration, floors and calories, with its pace and speed worked out (<see cref="WorkedOut"/>).</summary>
internal sealed class CardioSessionRule : ICardioSessionRule
{
    /// <inheritdoc />
    public CardioBest Best(CardioEntry entry) => new(
        entry.Distance,
        entry.Duration,
        WorkedOut.Pace(entry.Duration, entry.Distance),
        WorkedOut.Speed(entry.Duration, entry.Distance),
        entry.Floors,
        entry.Calories);
}
