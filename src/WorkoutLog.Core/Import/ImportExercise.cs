using WorkoutLog.Core.Domain.Workouts;

namespace WorkoutLog.Core.Import;

/// <summary>An exercise read from the file: sets for strength, an entry for cardio.</summary>
/// <param name="Exercise">The exercise. Required.</param>
/// <param name="Sets">Sets in the order added.</param>
/// <param name="Entry">The entry.</param>
/// <param name="Note">The exercise's note.</param>
internal sealed record ImportExercise(ImportExerciseInfo Exercise, IReadOnlyList<ImportSet>? Sets = null, ImportEntry? Entry = null, string? Note = null)
{
    /// <summary>True when the exercise has sets or an entry, not both, and no null set.</summary>
    public bool IsWellFormed => (Sets, Entry) switch
    {
        ({ } sets, null) => !sets.Contains(null!),
        (null, { }) => true,
        _ => false,
    };

    /// <summary>
    /// The domain exercise, with its note normalized by <see cref="NoteText"/>: strength when it has sets, otherwise cardio.
    /// Only for a well-formed exercise.
    /// </summary>
    public WorkoutExercise ToDomain() => (Sets, Entry) switch
    {
        ({ } sets, _) => new StrengthExercise(
            Exercise.Id, sets.Select(s => new StrengthSet(s.Reps, s.WeightKg, s.Duration, s.Distance)).ToList(), NoteText.Normalize(Note)),
        (_, { } e) => new CardioExercise(
            Exercise.Id, new CardioEntry(e.Duration, e.Distance, e.Floors, e.Level, e.Incline, e.Calories, e.Rate), NoteText.Normalize(Note)),
        _ => throw new InvalidOperationException("The exercise has no sets or entry"),
    };
}
