namespace WorkoutLog.UI.Models.Workout;

public sealed record SetRef(ExerciseState Exercise, EntryState Entry)
{
    /// <summary>Set number, from 1.</summary>
    public int Number => Exercise.Entries.IndexOf(Entry) + 1;
}
