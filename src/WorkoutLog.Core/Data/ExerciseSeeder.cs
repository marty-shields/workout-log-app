using SQLite;
using WorkoutLog.Core.Domain.Exercises;

namespace WorkoutLog.Core.Data;

/// <summary>Seeds the Exercise table from <see cref="IExerciseCatalog"/> in one transaction.</summary>
public sealed class ExerciseSeeder(SQLiteAsyncConnection connection, IExerciseCatalog catalog) : IExerciseSeeder
{
    /// <inheritdoc />
    public async Task SeedAsync()
    {
        await connection.CreateTableAsync<Exercise>();
        var existing = (await connection.Table<Exercise>().ToListAsync()).ToDictionary(e => e.Id);

        await connection.RunInTransactionAsync(db =>
        {
            foreach (var definition in catalog.All)
            {
                if (!existing.TryGetValue(definition.Id, out var row))
                {
                    db.Insert(new Exercise { Id = definition.Id, Name = definition.Name, Type = definition.Type });
                }
                else if (row.Name != definition.Name || row.Type != definition.Type)
                {
                    row.Name = definition.Name;
                    row.Type = definition.Type;
                    db.Update(row);
                }
            }
        });
    }
}
