using WorkoutLog.Core.Domain.Exercises;
using WorkoutLog.Core.Domain.Workouts;

namespace WorkoutLog.E2E.Tests.Support;

/// <summary>
/// An exercise, the values of its sets or entry and its note, e.g. "Barbell Back Squat: 5 reps 60 kg; 5 reps 62.5 kg" or
/// "Barbell Back Squat: 5 reps 60 kg (note: Belt on from set 3)". A note holds no ";", no " / " and no "|".
/// </summary>
public sealed record ExerciseValues(string Name, IReadOnlyList<EntryValues> Entries, string? Note = null)
{
    private const string NoteStart = " (note: ";

    public ExerciseDefinition Definition => Exercises.Named(Name);

    public static ExerciseValues Parse(string text)
    {
        var colon = text.IndexOf(": ", StringComparison.Ordinal);
        if (colon < 0)
        {
            throw new FormatException($"\"{text}\" needs an exercise name, a colon and its values.");
        }

        var values = text[(colon + 2)..];
        string? note = null;
        if (values.EndsWith(')') && values.IndexOf(NoteStart, StringComparison.Ordinal) is var start and >= 0)
        {
            note = values[(start + NoteStart.Length)..^1];
            values = values[..start];
        }

        return new ExerciseValues(text[..colon].Trim(), EntryValues.ParseSets(values), note);
    }

    /// <summary>Exercises separated by " / ".</summary>
    public static IReadOnlyList<ExerciseValues> ParseWorkout(string text) =>
        text.Split(" / ", StringSplitOptions.TrimEntries).Select(Parse).ToList();

    /// <summary>The exercise as Core saves it.</summary>
    public WorkoutExercise ToWorkoutExercise() => Definition.Type == ExerciseType.Cardio
        ? new CardioExercise(Definition.Id, Entries.Single().ToEntry(), Note)
        : new StrengthExercise(Definition.Id, Entries.Select(e => e.ToSet()).ToList(), Note);
}
