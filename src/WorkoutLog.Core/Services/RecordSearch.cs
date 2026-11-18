using WorkoutLog.Core.Domain.Exercises;
using WorkoutLog.Core.Domain.Exercises.Bests;
using WorkoutLog.Core.Domain.Workouts;

namespace WorkoutLog.Core.Services;

/// <summary>Finds the records in saved workouts for the services that report them.</summary>
internal static class RecordSearch
{
    /// <summary>
    /// Returns each known exercise's records, oldest first, in the order the exercises first appear in <paramref name="workouts"/>.
    /// <paramref name="unknown"/> gets the id of an exercise missing from <paramref name="definitions"/>, which is left out;
    /// <paramref name="failed"/> gets the id and exception of an exercise whose check threw <see cref="InvalidOperationException"/>, which is also left out.
    /// </summary>
    public static List<(ExerciseDefinition Definition, IReadOnlyList<SessionRecord> Records)> Find(
        IEnumerable<Workout> workouts,
        IReadOnlyDictionary<int, ExerciseDefinition> definitions,
        IRecordCheck recordCheck,
        Action<int> unknown,
        Action<int, Exception> failed)
    {
        var found = new List<(ExerciseDefinition, IReadOnlyList<SessionRecord>)>();
        var sessions = workouts.SelectMany(w => w.Exercises.Select(e => new WorkoutSession(w.Id, w.StartedAt, e)));
        foreach (var exercise in sessions.GroupBy(s => s.Exercise.ExerciseId))
        {
            if (!definitions.TryGetValue(exercise.Key, out var definition))
            {
                unknown(exercise.Key);
                continue;
            }

            try
            {
                found.Add((definition, recordCheck.Records(definition, exercise)));
            }
            catch (InvalidOperationException exception)
            {
                failed(exercise.Key, exception);
            }
        }

        return found;
    }
}
