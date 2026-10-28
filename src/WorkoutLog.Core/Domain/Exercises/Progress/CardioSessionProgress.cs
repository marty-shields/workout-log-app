namespace WorkoutLog.Core.Domain.Exercises.Progress;

/// <summary>A cardio session with its chart values.</summary>
/// <param name="Session">The session.</param>
/// <param name="Best">Best value of each chart metric across its entries.</param>
public sealed record CardioSessionProgress(CardioSession Session, CardioBest Best);
