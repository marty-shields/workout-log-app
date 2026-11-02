using WorkoutLog.Core.Domain.Exercises;
using WorkoutLog.Core.Domain.Workouts;
using WorkoutLog.Core.Services;
using WorkoutLog.UI.Models.Workout;

namespace WorkoutLog.UI.Models.Exercises;

/// <summary>
/// Builds an exercise's <see cref="LastTime"/> from its last session, in the text the Exercise page uses for that kind of exercise.
/// Holds and carries join their values with " · ".
/// </summary>
public static class LastTimeItems
{
    /// <summary>
    /// One text per set, or the one cardio entry. Null when there is no last session, loading failed, or the session isn't the exercise's kind.
    /// </summary>
    public static LastTime? From(ExerciseDefinition exercise, LastSessionResult last) => (last.Exercise, last.Best) switch
    {
        (StrengthExercise strength, _) when exercise.Category != ExerciseCategory.Cardio =>
            new LastTime([.. strength.Sets.Select(set => SetText(exercise, set))]),
        (CardioExercise cardio, { } best) when exercise.Category == ExerciseCategory.Cardio =>
            new LastTime([CardioProgressItems.EntryText(exercise, cardio.Entry, best, skip: null) is { Length: > 0 } text ? text : "—"]),
        _ => null,
    };

    private static string SetText(ExerciseDefinition exercise, StrengthSet set) => exercise.Category switch
    {
        ExerciseCategory.Bodyweight => BodyweightProgressItems.SetText(set),
        ExerciseCategory.Assisted => AssistedProgressItems.SetText(set),
        ExerciseCategory.Hold => HoldProgressItems.LastText(set),
        ExerciseCategory.Carry => CarryProgressItems.LastText(set, exercise),
        _ => ExerciseProgressItems.SetText(set),
    };
}
