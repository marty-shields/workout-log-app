namespace WorkoutLog.Core.Data;

/// <summary>Runs <see cref="DatabaseInitializer"/>, including seeding, through the <see cref="DatabaseGate"/>.</summary>
internal sealed class GatedDatabaseInitializer(DatabaseInitializer inner, DatabaseGate gate) : IDatabaseInitializer
{
    /// <inheritdoc />
    public Task InitializeAsync() => gate.RunAsync(inner.InitializeAsync);
}
