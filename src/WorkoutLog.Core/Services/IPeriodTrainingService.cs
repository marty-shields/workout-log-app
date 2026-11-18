using WorkoutLog.Core.Domain.Workouts;
using WorkoutLog.Core.Domain.Workouts.Periods;

namespace WorkoutLog.Core.Services;

/// <summary>Reads the records, muscle groups and exercises of any week, month or year, for This week and Trends. Days are local to the phone's time zone and a workout only counts up to now.</summary>
public interface IPeriodTrainingService
{
    /// <summary>Returns the workouts started inside the period, oldest first, with their exercise ids in the workout's order. Ids not in the exercise list are left out.</summary>
    Task<ReadResult<IReadOnlyList<WorkoutOutline>>> GetWorkoutsAsync(Period period);

    /// <summary>Returns the records set inside the period, by the same rules as the exercise screens.</summary>
    Task<ReadResult<PeriodRecords>> GetRecordsAsync(Period period);

    /// <summary>Returns all six muscle groups with their strength sets in the period, by primary muscle; 0 when not trained.</summary>
    Task<ReadResult<IReadOnlyList<MuscleGroupCount>>> GetMuscleSetsAsync(Period period);

    /// <summary>Returns up to five exercises with the most sessions in the period, most first, ties by name.</summary>
    Task<ReadResult<IReadOnlyList<TrainedExercise>>> GetMostTrainedAsync(Period period);
}
