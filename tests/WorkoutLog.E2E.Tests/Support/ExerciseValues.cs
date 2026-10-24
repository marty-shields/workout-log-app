using WorkoutLog.Core.Domain.Exercises;
using WorkoutLog.Core.Domain.Workouts;

namespace WorkoutLog.E2E.Tests.Support;

/// <summary>An exercise and the values of its sets or entry, e.g. "Barbell Back Squat: 5 reps 60 kg; 5 reps 62.5 kg".</summary>
public sealed record ExerciseValues(string Name, IReadOnlyList<EntryValues> Entries)
{
    public ExerciseDefinition Definition => Exercises.Named(Name);

    public static ExerciseValues Parse(string text)
    {
        var colon = text.IndexOf(": ", StringComparison.Ordinal);
        if (colon < 0)
        {
            throw new FormatException($"\"{text}\" needs an exercise name, a colon and its values.");
        }

        return new ExerciseValues(text[..colon].Trim(), EntryValues.ParseSets(text[(colon + 2)..]));
    }

    /// <summary>Exercises separated by " / ".</summary>
    public static IReadOnlyList<ExerciseValues> ParseWorkout(string text) =>
        text.Split(" / ", StringSplitOptions.TrimEntries).Select(Parse).ToList();

    /// <summary>The exercise as Core saves it.</summary>
    public WorkoutExercise ToWorkoutExercise() => Definition.Type == ExerciseType.Cardio
        ? new CardioExercise(Definition.Id, Entries.Single().ToEntry())
        : new StrengthExercise(Definition.Id, Entries.Select(e => e.ToSet()).ToList());
}
