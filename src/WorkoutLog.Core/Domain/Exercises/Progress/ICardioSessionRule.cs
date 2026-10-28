using WorkoutLog.Core.Domain.Workouts;

namespace WorkoutLog.Core.Domain.Exercises.Progress;

/// <summary>Picks a cardio session's best values.</summary>
internal interface ICardioSessionRule
{
    /// <summary>The best value of each chart metric across <paramref name="entries"/>.</summary>
    CardioBest Best(IReadOnlyList<CardioEntry> entries);
}
