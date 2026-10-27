namespace WorkoutLog.E2E.Tests.Support;

/// <summary>What a scenario has done so far, for later steps to check against. One per scenario.</summary>
public sealed class ScenarioState
{
    /// <summary>Exercises logged through the UI, in the order added, with each set's values as typed.</summary>
    internal List<(string Name, List<Dictionary<string, string>> Entries)> Logged { get; } = [];

    /// <summary>The exercise added last; steps that name no exercise mean this one.</summary>
    internal string? LastAdded { get; set; }

    /// <summary>Device local time just before and just after New Workout opened.</summary>
    internal (DateTime From, DateTime To)? Started { get; set; }

    /// <summary>Seeded workouts by id, with their local start.</summary>
    internal Dictionary<int, DateTime> Seeded { get; } = [];

    /// <summary>Seeded workouts by id, with their local finish.</summary>
    internal Dictionary<int, DateTime> SeededFinished { get; } = [];

    /// <summary>Seeded workouts' exercises by id.</summary>
    internal Dictionary<int, IReadOnlyList<ExerciseValues>> SeededExercises { get; } = [];

    /// <summary>The app's log when the scenario's starting point was reached, for counting new lines.</summary>
    internal string LogAtStart { get; set; } = "";

    /// <summary>Records a set's or entry's values as typed, replacing the fields given.</summary>
    internal void RecordLogged(string name, int set, IReadOnlyDictionary<string, string> values)
    {
        var index = Logged.FindIndex(e => e.Name == name);
        if (index < 0)
        {
            Logged.Add((name, []));
            index = Logged.Count - 1;
        }

        var entries = Logged[index].Entries;
        while (entries.Count < set)
        {
            entries.Add([]);
        }

        foreach (var (key, value) in values)
        {
            entries[set - 1][key] = value;
        }
    }
}
