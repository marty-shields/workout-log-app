using WorkoutLog.Core.Services;

namespace WorkoutLog.Core.Import;

/// <summary>What the reader got from a file.</summary>
/// <param name="Error">Why the file can't be imported, or <see cref="ImportError.None"/>.</param>
/// <param name="Workouts">The workouts in file order. Empty unless <see cref="ImportError.None"/>.</param>
/// <param name="Exception">The exception that made the file unreadable, if any.</param>
internal sealed record ImportFileRead(ImportError Error, IReadOnlyList<ImportWorkout> Workouts, Exception? Exception = null);
