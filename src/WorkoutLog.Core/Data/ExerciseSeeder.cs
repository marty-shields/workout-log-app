using Microsoft.Extensions.Logging;
using SQLite;
using WorkoutLog.Core.Domain.Exercises;
using WorkoutLog.Core.Logging;

namespace WorkoutLog.Core.Data;

/// <summary>Seeds the Exercise table from <see cref="IExerciseCatalog"/> in one transaction.</summary>
internal sealed partial class ExerciseSeeder(SQLiteAsyncConnection connection, IExerciseCatalog catalog, ILogger<ExerciseSeeder> logger) : IExerciseSeeder
{
    /// <inheritdoc />
    public async Task SeedAsync()
    {
        var existing = (await connection.Table<Exercise>().ToListAsync()).ToDictionary(e => e.Id);
        var added = 0;
        var updated = 0;

        await connection.RunInTransactionAsync(db =>
        {
            foreach (var definition in catalog.All)
            {
                if (!existing.TryGetValue(definition.Id, out var row))
                {
                    db.Insert(new Exercise { Id = definition.Id, Name = definition.Name, Type = definition.Type });
                    added++;
                }
                else if (row.Name != definition.Name || row.Type != definition.Type)
                {
                    row.Name = definition.Name;
                    row.Type = definition.Type;
                    db.Update(row);
                    updated++;
                }
            }
        });

        Log.Seeded(logger, added, updated);
    }

    private static partial class Log
    {
        [LoggerMessage((int)CoreEvent.ExercisesSeeded, LogLevel.Information, "Exercise list seeded: {Added} added, {Updated} updated")]
        public static partial void Seeded(ILogger logger, int added, int updated);
    }
}
