namespace WorkoutLog.Core.Data;

/// <summary>Runs the inner initializer, including seeding, through the <see cref="DatabaseGate"/>.</summary>
internal sealed class GatedDatabaseInitializer(IDatabaseInitializer inner, DatabaseGate gate) : IDatabaseInitializer
{
    /// <inheritdoc />
    public Task InitializeAsync() => gate.RunAsync(inner.InitializeAsync);
}
