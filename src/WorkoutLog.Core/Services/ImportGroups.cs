using WorkoutLog.Core.Domain.Workouts;

namespace WorkoutLog.Core.Services;

/// <summary>The checked workouts of a file, by group, in file order.</summary>
/// <param name="New">No phone workout has the start.</param>
/// <param name="Already">A phone workout has the start and the same contents.</param>
/// <param name="Different">A phone workout has the start and different contents.</param>
internal sealed record ImportGroups(IReadOnlyList<NewWorkout> New, IReadOnlyList<NewWorkout> Already, IReadOnlyList<NewWorkout> Different)
{
    /// <summary>No workouts.</summary>
    public static readonly ImportGroups Empty = new([], [], []);
}
