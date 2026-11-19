namespace WorkoutLog.UI.Components.ThisWeek;

/// <summary>One row of a <see cref="WeekWorkoutsCard"/>.</summary>
/// <param name="StartedAt">Start time on the phone's wall clock.</param>
/// <param name="Exercises">Exercise names in the workout's order.</param>
/// <param name="Duration">Finish minus start.</param>
public sealed record WeekWorkout(int Id, DateTime StartedAt, IReadOnlyList<string> Exercises, TimeSpan Duration);
