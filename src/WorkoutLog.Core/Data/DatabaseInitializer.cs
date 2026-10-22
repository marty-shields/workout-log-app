using SQLite;

namespace WorkoutLog.Core.Data;

/// <summary>Upgrades the schema version stored in SQLite <c>user_version</c>, creates missing tables, then seeds the exercise list.</summary>
public sealed class DatabaseInitializer : IDatabaseInitializer
{
    /// <summary>The app's schema version.</summary>
    public const int CurrentVersion = 1;

    private readonly SQLiteAsyncConnection connection;
    private readonly IReadOnlyList<ISchemaUpgrade> upgrades;
    private readonly IExerciseSeeder seeder;
    private readonly int currentVersion;

    /// <summary>Uses <see cref="CurrentVersion"/>.</summary>
    public DatabaseInitializer(SQLiteAsyncConnection connection, IEnumerable<ISchemaUpgrade> upgrades, IExerciseSeeder seeder)
        : this(connection, upgrades, seeder, CurrentVersion)
    {
    }

    internal DatabaseInitializer(SQLiteAsyncConnection connection, IEnumerable<ISchemaUpgrade> upgrades, IExerciseSeeder seeder, int currentVersion)
    {
        this.connection = connection;
        this.upgrades = upgrades.ToList();
        this.seeder = seeder;
        this.currentVersion = currentVersion;
    }

    /// <inheritdoc />
    /// <remarks>
    /// Throws <see cref="InvalidOperationException"/>, before changing anything, if the stored version is newer than the app's
    /// or the steps are not exactly one for each version from 2 to the app's.
    /// A new database (version 0) gets the current schema from the tables and runs no steps.
    /// An existing database runs the steps after its stored version, then gets any missing tables and columns.
    /// </remarks>
    public async Task InitializeAsync()
    {
        var stored = await connection.ExecuteScalarAsync<int>("PRAGMA user_version");
        if (stored > currentVersion)
        {
            throw new InvalidOperationException($"Database schema version {stored} is newer than the app's version {currentVersion}");
        }

        var steps = OrderedSteps();

        if (stored == 0)
        {
            await CreateTablesAsync();
            await connection.ExecuteAsync($"PRAGMA user_version = {currentVersion}");
        }
        else
        {
            foreach (var step in steps.Where(s => s.Version > stored))
            {
                await connection.RunInTransactionAsync(db =>
                {
                    step.Apply(db);
                    db.Execute($"PRAGMA user_version = {step.Version}");
                });
            }

            await CreateTablesAsync();
        }

        await seeder.SeedAsync();
    }

    private Task CreateTablesAsync() =>
        connection.CreateTablesAsync<Exercise, WorkoutRow, WorkoutExerciseRow, StrengthSetRow, CardioEntryRow>();

    private List<ISchemaUpgrade> OrderedSteps()
    {
        var steps = upgrades.OrderBy(u => u.Version).ToList();
        if (!steps.Select(s => s.Version).SequenceEqual(Enumerable.Range(2, currentVersion - 1)))
        {
            throw new InvalidOperationException(
                $"Expected one schema upgrade step for each version from 2 to {currentVersion}, found [{string.Join(", ", steps.Select(s => s.Version))}]");
        }

        return steps;
    }
}
