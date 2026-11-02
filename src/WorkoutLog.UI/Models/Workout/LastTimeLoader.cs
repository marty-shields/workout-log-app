using WorkoutLog.Core.Domain.Exercises;
using WorkoutLog.Core.Services;
using WorkoutLog.UI.Models.Exercises;

namespace WorkoutLog.UI.Models.Workout;

/// <summary>Loads an exercise's <see cref="LastTime"/> through <see cref="ILastSessionService"/>.</summary>
public sealed class LastTimeLoader(ILastSessionService lastSessions, IExerciseCatalog catalog)
{
    /// <summary>
    /// Sets <see cref="ExerciseState.LastTime"/> from the exercise's last saved workout, or from the last one that started before
    /// <paramref name="before"/>. It is null when there is none, loading failed or the exercise isn't in the exercise list.
    /// </summary>
    public async Task LoadAsync(ExerciseState exercise, DateTimeOffset? before = null)
    {
        if (catalog.All.FirstOrDefault(e => e.Id == exercise.ExerciseId) is not { } definition)
        {
            return;
        }

        exercise.LastTime = LastTimeItems.From(definition, await lastSessions.GetAsync(exercise.ExerciseId, before));
    }
}
