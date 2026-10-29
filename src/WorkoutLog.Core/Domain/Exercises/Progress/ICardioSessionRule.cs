using WorkoutLog.Core.Domain.Workouts;

namespace WorkoutLog.Core.Domain.Exercises.Progress;

/// <summary>Works out a cardio session's chart values.</summary>
internal interface ICardioSessionRule
{
    /// <summary>The chart values of <paramref name="entry"/>.</summary>
    CardioBest Best(CardioEntry entry);
}
