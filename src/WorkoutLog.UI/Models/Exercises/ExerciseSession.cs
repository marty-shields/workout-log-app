namespace WorkoutLog.UI.Models.Exercises;

/// <summary>A workout's sets of one exercise, in the Exercise sessions list.</summary>
/// <param name="WorkoutId">Workout id.</param>
/// <param name="Date">Workout date, e.g. Tue 20 Oct.</param>
/// <param name="Headline">The session's best, e.g. 70kg × 3.</param>
/// <param name="Detail">Every set, or the cardio entry, e.g. 60×8 · 62.5×8 · 70×3.</param>
public sealed record ExerciseSession(int WorkoutId, string Date, string Headline, string Detail);
