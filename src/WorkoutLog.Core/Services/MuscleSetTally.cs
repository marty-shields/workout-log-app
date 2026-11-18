using WorkoutLog.Core.Domain.Exercises;
using WorkoutLog.Core.Domain.Workouts;

namespace WorkoutLog.Core.Services;

/// <summary>Adds strength sets up per muscle group by each exercise's primary muscle.</summary>
internal static class MuscleSetTally
{
    /// <summary>
    /// Returns all six groups in <see cref="MuscleGroup"/> order, zero included, with the number of workouts that had a counted set.
    /// Cardio and exercises without a primary muscle are left out; <paramref name="unknown"/> gets each missing exercise id once.
    /// </summary>
    public static (IReadOnlyList<MuscleGroupCount> Groups, int Workouts) Count(
        IEnumerable<ExerciseSetCount> counts,
        IReadOnlyDictionary<int, ExerciseDefinition> definitions,
        MuscleGroupMap groups,
        Action<int> unknown)
    {
        var sets = Enum.GetValues<MuscleGroup>().ToDictionary(g => g, _ => 0);
        var missing = new HashSet<int>();
        var workouts = new HashSet<int>();
        foreach (var count in counts)
        {
            if (!definitions.TryGetValue(count.ExerciseId, out var definition))
            {
                if (missing.Add(count.ExerciseId))
                {
                    unknown(count.ExerciseId);
                }

                continue;
            }

            if (definition is { Type: ExerciseType.Strength, PrimaryMuscle: { } muscle })
            {
                sets[groups.GroupOf(muscle)] += count.Sets;
                workouts.Add(count.WorkoutId);
            }
        }

        return (Enum.GetValues<MuscleGroup>().Select(g => new MuscleGroupCount(g, sets[g])).ToList(), workouts.Count);
    }
}
