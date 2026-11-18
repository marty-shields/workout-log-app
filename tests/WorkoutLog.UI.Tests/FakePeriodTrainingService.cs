using WorkoutLog.Core.Domain.Exercises;
using WorkoutLog.Core.Domain.Workouts;
using WorkoutLog.Core.Domain.Workouts.Periods;
using WorkoutLog.Core.Services;

namespace WorkoutLog.UI.Tests;

/// <summary>
/// <see cref="IPeriodTrainingService"/> for the tests: returns what the properties hold, nothing by default. With <see cref="Hold"/> set,
/// each workouts read waits until the test calls <see cref="Complete"/>, and with <see cref="HoldMuscles"/> each muscle group read waits for <see cref="CompleteMuscles()"/>. Records the start of each period asked for.
/// </summary>
public sealed class FakePeriodTrainingService : IPeriodTrainingService
{
    private readonly List<TaskCompletionSource<ReadResult<IReadOnlyList<WorkoutOutline>>>> pending = [];
    private readonly List<TaskCompletionSource<ReadResult<IReadOnlyList<MuscleGroupCount>>>> pendingMuscles = [];

    public ReadResult<IReadOnlyList<WorkoutOutline>> Workouts { get; set; } = new ReadResult<IReadOnlyList<WorkoutOutline>>(Array.Empty<WorkoutOutline>());

    public ReadResult<PeriodRecords> Records { get; set; } = new PeriodRecords([], []);

    public ReadResult<IReadOnlyList<MuscleGroupCount>> Muscles { get; set; } =
        new ReadResult<IReadOnlyList<MuscleGroupCount>>(Enum.GetValues<MuscleGroup>().Select(g => new MuscleGroupCount(g, 0)).ToList());

    /// <summary>When true, workouts reads wait for <see cref="Complete"/>.</summary>
    public bool Hold { get; set; }

    /// <summary>When true, muscle group reads wait for <see cref="CompleteMuscles()"/>.</summary>
    public bool HoldMuscles { get; set; }

    public List<DateOnly> WorkoutsReads { get; } = [];

    public List<DateOnly> RecordReads { get; } = [];

    public List<DateOnly> MuscleReads { get; } = [];

    /// <inheritdoc />
    public Task<ReadResult<IReadOnlyList<WorkoutOutline>>> GetWorkoutsAsync(Period period)
    {
        WorkoutsReads.Add(period.Start);
        if (!Hold)
        {
            return Task.FromResult(Workouts);
        }

        var read = new TaskCompletionSource<ReadResult<IReadOnlyList<WorkoutOutline>>>();
        pending.Add(read);
        return read.Task;
    }

    /// <summary>Completes the latest held workouts read with <see cref="Workouts"/>.</summary>
    public void Complete() => pending[^1].SetResult(Workouts);

    /// <inheritdoc />
    public Task<ReadResult<PeriodRecords>> GetRecordsAsync(Period period)
    {
        RecordReads.Add(period.Start);
        return Task.FromResult(Records);
    }

    /// <inheritdoc />
    public Task<ReadResult<IReadOnlyList<MuscleGroupCount>>> GetMuscleSetsAsync(Period period)
    {
        MuscleReads.Add(period.Start);
        if (!HoldMuscles)
        {
            return Task.FromResult(Muscles);
        }

        var read = new TaskCompletionSource<ReadResult<IReadOnlyList<MuscleGroupCount>>>();
        pendingMuscles.Add(read);
        return read.Task;
    }

    /// <summary>Completes the latest held muscle group read with <see cref="Muscles"/>.</summary>
    public void CompleteMuscles() => CompleteMuscles(pendingMuscles.Count - 1);

    /// <summary>Completes the held muscle group read at <paramref name="index"/> (in the order they were asked for) with <see cref="Muscles"/>.</summary>
    public void CompleteMuscles(int index) => pendingMuscles[index].SetResult(Muscles);

    /// <inheritdoc />
    public Task<ReadResult<IReadOnlyList<TrainedExercise>>> GetMostTrainedAsync(Period period) => throw new NotSupportedException();
}
