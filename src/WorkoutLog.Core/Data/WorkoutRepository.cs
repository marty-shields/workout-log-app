using SQLite;
using WorkoutLog.Core.Domain.Workouts;

namespace WorkoutLog.Core.Data;

/// <summary>Reads workouts from SQLite and runs each write in its own transaction through an <see cref="IWorkoutWriter"/>.</summary>
internal sealed class WorkoutRepository(SQLiteAsyncConnection connection, IWorkoutWriterFactory writers) : IWorkoutRepository
{
    /// <inheritdoc />
    public async Task<int> AddAsync(NewWorkout workout)
    {
        var id = 0;
        await connection.RunInTransactionAsync(db => id = writers.Create(db).Add(workout));
        return id;
    }

    /// <inheritdoc />
    public Task ReplaceExercisesAsync(int workoutId, IReadOnlyList<WorkoutExercise> exercises) =>
        connection.RunInTransactionAsync(db => writers.Create(db).ReplaceExercises(workoutId, exercises));

    /// <inheritdoc />
    public async Task<Workout?> GetAsync(int id)
    {
        var row = await connection.FindAsync<WorkoutRow>(id);
        if (row is null)
        {
            return null;
        }

        var exerciseRows = await connection.Table<WorkoutExerciseRow>()
            .Where(e => e.WorkoutId == id).OrderBy(e => e.Position).ToListAsync();
        var exerciseIds = exerciseRows.Select(e => e.Id).ToList();
        var sets = (await connection.Table<StrengthSetRow>()
                .Where(s => exerciseIds.Contains(s.WorkoutExerciseId)).OrderBy(s => s.Position).ToListAsync())
            .ToLookup(s => s.WorkoutExerciseId);
        var entries = (await connection.Table<CardioEntryRow>()
                .Where(c => exerciseIds.Contains(c.WorkoutExerciseId)).ToListAsync())
            .ToDictionary(c => c.WorkoutExerciseId);

        return new Workout(row.Id, row.StartedAt, row.FinishedAt, ToExercises(exerciseRows, sets, entries));
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<Workout>> GetAllAsync()
    {
        var rows = await connection.Table<WorkoutRow>().OrderBy(w => w.StartedAt).ToListAsync();
        var exerciseRows = (await connection.Table<WorkoutExerciseRow>().OrderBy(e => e.Position).ToListAsync())
            .ToLookup(e => e.WorkoutId);
        var sets = (await connection.Table<StrengthSetRow>().OrderBy(s => s.Position).ToListAsync())
            .ToLookup(s => s.WorkoutExerciseId);
        var entries = (await connection.Table<CardioEntryRow>().ToListAsync())
            .ToDictionary(c => c.WorkoutExerciseId);

        return rows.Select(w => new Workout(w.Id, w.StartedAt, w.FinishedAt, ToExercises(exerciseRows[w.Id], sets, entries)))
            .ToList();
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<WorkoutSummary>> ListAsync(DateTimeOffset? from, DateTimeOffset? to, int? limit)
    {
        var query = connection.Table<WorkoutRow>();
        if (from is { } start)
        {
            query = query.Where(w => w.StartedAt >= start);
        }

        if (to is { } end)
        {
            query = query.Where(w => w.StartedAt <= end);
        }

        query = query.OrderByDescending(w => w.StartedAt);
        if (limit is { } count)
        {
            query = query.Take(count);
        }

        return (await query.ToListAsync()).Select(w => new WorkoutSummary(w.Id, w.StartedAt)).ToList();
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<WorkoutWeights>> GetWeightsAsync(int exerciseId)
    {
        const string sql = """
            SELECT w.Id AS WorkoutId, s.WeightKg AS WeightKg
            FROM Workout w
            JOIN WorkoutExercise e ON e.WorkoutId = w.Id
            LEFT JOIN StrengthSet s ON s.WorkoutExerciseId = e.Id
            WHERE e.ExerciseId = ?
            ORDER BY w.StartedAt DESC, e.Position, s.Position
            """;
        var rows = await connection.QueryAsync<WeightRow>(sql, exerciseId);
        return rows.GroupBy(r => r.WorkoutId)
            .Select(g => new WorkoutWeights(g.Key, g.Where(r => r.WeightKg is not null).Select(r => r.WeightKg!.Value).ToList()))
            .ToList();
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<LastSession>> GetLastSessionsAsync()
    {
        const string sql = """
            SELECT e.Id, e.WorkoutId, e.ExerciseId, e.Position, w.StartedAt
            FROM WorkoutExercise e
            JOIN Workout w ON w.Id = e.WorkoutId
            JOIN (
                SELECT e2.ExerciseId, MAX(w2.StartedAt) AS LastStartedAt
                FROM WorkoutExercise e2
                JOIN Workout w2 ON w2.Id = e2.WorkoutId
                GROUP BY e2.ExerciseId) latest ON latest.ExerciseId = e.ExerciseId AND latest.LastStartedAt = w.StartedAt
            ORDER BY e.ExerciseId, e.Position
            """;
        var rows = await connection.QueryAsync<ExerciseStartRow>(sql);
        var exerciseIds = rows.Select(r => r.Id).ToList();
        var sets = (await connection.Table<StrengthSetRow>()
                .Where(s => exerciseIds.Contains(s.WorkoutExerciseId)).OrderBy(s => s.Position).ToListAsync())
            .ToLookup(s => s.WorkoutExerciseId);
        var entries = (await connection.Table<CardioEntryRow>()
                .Where(c => exerciseIds.Contains(c.WorkoutExerciseId)).ToListAsync())
            .ToDictionary(c => c.WorkoutExerciseId);

        return rows.GroupBy(r => r.ExerciseId)
            .Select(g => new LastSession(g.Key, new DateTimeOffset(g.First().StartedAt, TimeSpan.Zero),
                ToExercises(g.Select(r => new WorkoutExerciseRow { Id = r.Id, WorkoutId = r.WorkoutId, ExerciseId = r.ExerciseId, Position = r.Position }), sets, entries)))
            .ToList();
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<WorkoutSession>> GetSessionsAsync(int exerciseId)
    {
        const string exercisesSql = """
            SELECT e.Id, e.WorkoutId, e.ExerciseId, e.Position, w.StartedAt
            FROM WorkoutExercise e
            JOIN Workout w ON w.Id = e.WorkoutId
            WHERE e.ExerciseId = ?
            ORDER BY w.StartedAt, e.Position
            """;
        const string setsSql = """
            SELECT s.*
            FROM StrengthSet s
            JOIN WorkoutExercise e ON e.Id = s.WorkoutExerciseId
            WHERE e.ExerciseId = ?
            ORDER BY s.Position
            """;
        const string entriesSql = """
            SELECT c.*
            FROM CardioEntry c
            JOIN WorkoutExercise e ON e.Id = c.WorkoutExerciseId
            WHERE e.ExerciseId = ?
            """;
        var rows = await connection.QueryAsync<ExerciseStartRow>(exercisesSql, exerciseId);
        var sets = (await connection.QueryAsync<StrengthSetRow>(setsSql, exerciseId)).ToLookup(s => s.WorkoutExerciseId);
        var entries = (await connection.QueryAsync<CardioEntryRow>(entriesSql, exerciseId)).ToDictionary(c => c.WorkoutExerciseId);

        return rows.GroupBy(r => r.WorkoutId)
            .Select(g => new WorkoutSession(g.Key, new DateTimeOffset(g.First().StartedAt, TimeSpan.Zero),
                ToExercises(g.Select(r => new WorkoutExerciseRow { Id = r.Id, WorkoutId = r.WorkoutId, ExerciseId = r.ExerciseId, Position = r.Position }), sets, entries)))
            .ToList();
    }

    /// <inheritdoc />
    public Task DeleteAsync(int id) =>
        connection.RunInTransactionAsync(db => writers.Create(db).Delete(id));

    /// <summary>Builds each exercise: cardio when it has an entry, otherwise strength with its sets.</summary>
    private static List<WorkoutExercise> ToExercises(
        IEnumerable<WorkoutExerciseRow> exerciseRows, ILookup<int, StrengthSetRow> sets, Dictionary<int, CardioEntryRow> entries) =>
        exerciseRows.Select(e => entries.TryGetValue(e.Id, out var entry)
                ? (WorkoutExercise)new CardioExercise(e.ExerciseId, ToEntry(entry))
                : new StrengthExercise(e.ExerciseId, sets[e.Id].Select(ToSet).ToList()))
            .ToList();

    private static StrengthSet ToSet(StrengthSetRow row) => new(row.Reps, row.WeightKg, row.Duration, row.Distance);

    private static CardioEntry ToEntry(CardioEntryRow row) =>
        new(row.Duration, row.Distance, row.Floors, row.Level, row.Incline, row.Calories, row.Rate, row.Pace);

    /// <summary>One set's weight with its workout id; null weight for a set without one or an exercise without sets.</summary>
    private sealed class WeightRow
    {
        public int WorkoutId { get; set; }

        public decimal? WeightKg { get; set; }
    }

    /// <summary>A workout exercise row with its workout's start, in UTC ticks.</summary>
    private sealed class ExerciseStartRow
    {
        public int Id { get; set; }

        public int WorkoutId { get; set; }

        public int ExerciseId { get; set; }

        public int Position { get; set; }

        public long StartedAt { get; set; }
    }
}
