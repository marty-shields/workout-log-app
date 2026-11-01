namespace WorkoutLog.UI.Components.Dashboard;

/// <summary>The workout a <see cref="LastWorkoutCard"/> shows.</summary>
/// <param name="StartedAt">Start in the phone's time zone.</param>
/// <param name="DaysAgo">Calendar days from the start to today.</param>
/// <param name="Duration">Finish minus start.</param>
/// <param name="Exercises">Exercise names in the workout's order.</param>
public sealed record LastWorkout(DateTime StartedAt, int DaysAgo, TimeSpan Duration, IReadOnlyList<string> Exercises);
