namespace WorkoutLog.Core.Domain.Exercises.Progress;

/// <summary>A cardio session with its chart values.</summary>
/// <param name="Session">The session.</param>
/// <param name="Best">Chart values of its entry.</param>
public sealed record CardioSessionProgress(CardioSession Session, CardioBest Best);
