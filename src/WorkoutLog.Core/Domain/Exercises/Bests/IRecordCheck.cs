using WorkoutLog.Core.Domain.Workouts;

namespace WorkoutLog.Core.Domain.Exercises.Bests;

/// <summary>Finds the sessions of an exercise that set a record.</summary>
internal interface IRecordCheck
{
    /// <summary>
    /// Returns the sessions whose best beats the best of every earlier session, oldest first. Bests come from the exercise's
    /// <see cref="IBestRule"/>; a tie isn't a record, and the first session with a best isn't one.
    /// </summary>
    IReadOnlyList<SessionRecord> Records(ExerciseDefinition exercise, IEnumerable<WorkoutSession> sessions);
}
