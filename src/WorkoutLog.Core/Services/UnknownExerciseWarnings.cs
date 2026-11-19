using System.Collections.Concurrent;
using WorkoutLog.Core.Logging;

namespace WorkoutLog.Core.Services;

/// <summary>Remembers which unknown exercise ids the period services have warned about, so each is warned about once per app run for each event.</summary>
internal sealed class UnknownExerciseWarnings
{
    private readonly ConcurrentDictionary<(CoreEvent Event, int ExerciseId), byte> reported = new();

    /// <summary>Whether this is the first time <paramref name="exerciseId"/> is reported under <paramref name="warning"/>; false after that.</summary>
    public bool IsFirst(CoreEvent warning, int exerciseId) => reported.TryAdd((warning, exerciseId), 0);
}
