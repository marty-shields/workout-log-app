using WorkoutLog.UI.Models.Dashboard;
using WorkoutLog.UI.Models.Fields;
using WorkoutLog.UI.Models.History;
using WorkoutLog.UI.Models.Workout;

namespace WorkoutLog.UI.Stories.Samples;

/// <summary>An exercise from docs/exercises.json, as the UI shows it.</summary>
/// <param name="Id">Exercise id.</param>
/// <param name="Name">Exercise name.</param>
/// <param name="Kind">Strength or cardio.</param>
/// <param name="Aliases">Nicknames for search.</param>
/// <param name="Fields">Fields per set or entry.</param>
public sealed record SampleExercise(int Id, string Name, ExerciseKind Kind, string[] Aliases, IReadOnlyList<FieldSpec> Fields);

/// <summary>A saved workout in the prototype.</summary>
/// <param name="Id">Workout id.</param>
/// <param name="StartedAt">Start time.</param>
/// <param name="FinishedAt">Finish time.</param>
/// <param name="Exercises">Exercises and their sets or entries.</param>
public sealed record SavedWorkout(int Id, DateTime StartedAt, DateTime FinishedAt, IReadOnlyList<ExerciseState> Exercises);

/// <summary>Sample data for stories, plus simplified stand-ins for Core's search, save rules and counts.</summary>
public static class SampleData
{
    /// <summary>Start time used by the static stories: Tue 20 Oct 2026 18:05.</summary>
    public static readonly DateTime Started = new(2026, 10, 20, 18, 5, 0);

    private static readonly FieldSpec Reps = new("reps", "Reps", FieldKind.Whole, Required: true);
    private static readonly FieldSpec Weight = new("weight", "Weight", FieldKind.Decimal, Unit: "kg");
    private static readonly FieldSpec Assist = new("weight", "Assist", FieldKind.Decimal, Unit: "kg");
    private static readonly FieldSpec Duration = new("duration", "Duration", FieldKind.Duration, Required: true);
    private static readonly FieldSpec OptionalDuration = new("duration", "Duration", FieldKind.Duration);
    private static readonly FieldSpec CarryDistance = new("distance", "Distance", FieldKind.Decimal, Required: true, Unit: "m");
    private static readonly FieldSpec Incline = new("incline", "Incline", FieldKind.Decimal, Unit: "%");
    private static readonly FieldSpec Floors = new("floors", "Floors", FieldKind.Whole);
    private static readonly FieldSpec Calories = new("calories", "Calories", FieldKind.Whole);
    private static readonly FieldSpec Pace = new("rate", "Pace", FieldKind.Pace, Unit: "min/km");

    /// <summary>A selection of exercises covering every field layout.</summary>
    public static readonly IReadOnlyList<SampleExercise> Exercises =
    [
        new(6, "Barbell Back Squat", ExerciseKind.Strength, [], [Reps, Weight]),
        new(92, "Cable Rope Tricep Pushdown", ExerciseKind.Strength, [], [Reps, Weight]),
        new(132, "Dumbbell Curl", ExerciseKind.Strength, ["Dumbbell Biceps Curl"], [Reps, Weight]),
        new(144, "Dumbbell Incline Press", ExerciseKind.Strength, ["Incline Dumbbell Bench Press"], [Reps, Weight]),
        new(187, "Farmer's Walk", ExerciseKind.Strength, ["Farmer's Carry"], [CarryDistance, Weight, OptionalDuration]),
        new(218, "Machine Assisted Pull-up", ExerciseKind.Strength, ["Gravitron"], [Reps, Assist]),
        new(221, "Machine Chest Fly", ExerciseKind.Strength, ["Pec Deck", "Butterfly", "Pec Fly"], [Reps, Weight]),
        new(222, "Machine Chest Press", ExerciseKind.Strength, ["Machine Bench Press", "Hammer Strength"], [Reps, Weight]),
        new(258, "Plank", ExerciseKind.Strength, [], [Duration, Weight]),
        new(300, "Treadmill", ExerciseKind.Cardio, ["Running Machine"], [Duration, Distance("km"), Incline, Calories, Rate("km/h")]),
        new(305, "Rowing Machine", ExerciseKind.Cardio, ["Rower", "Erg", "Concept2"], [Duration, Distance("m"), Level("Damper"), Calories, Rate("strokes/min")]),
        new(309, "Stair Climber", ExerciseKind.Cardio, ["StairMaster", "StepMill", "Stepper"], [Duration, Floors, Level("Speed level"), Calories, Rate("steps/min")]),
        new(311, "Outdoor Run", ExerciseKind.Cardio, ["Running", "Jog", "Jogging"], [Duration, Distance("km"), Calories, Pace]),
    ];

    /// <summary>Picker results. Every query word must be found in the name or a nickname; name prefix matches first. Empty query: Strength then Cardio, A–Z.</summary>
    public static IReadOnlyList<PickerItem> Search(string? query)
    {
        var text = (query ?? "").Trim();
        var words = SearchWords(text);
        var matches = Exercises.Where(e => words.All(SearchText(e).Contains));
        var ordered = words.Length == 0
            ? matches.OrderBy(e => e.Kind).ThenBy(e => e.Name)
            : matches.OrderBy(e => e.Name.StartsWith(text, StringComparison.OrdinalIgnoreCase) ? 0 : 1).ThenBy(e => e.Name);
        return ordered.Select(e => new PickerItem(e.Id, e.Name, e.Kind)).ToList();
    }

    /// <summary>A newly added exercise: one empty set (strength) or its empty entry (cardio).</summary>
    public static ExerciseState NewExercise(int id) => Exercise(id, new string?[Find(id).Fields.Count]);

    /// <summary>An exercise with one set or entry per array; values follow the exercise's field order.</summary>
    public static ExerciseState Exercise(int id, params string?[][] entries)
    {
        var sample = Find(id);
        var exercise = new ExerciseState(sample.Name, sample.Kind, sample.Fields);
        foreach (var values in entries)
        {
            var entry = new EntryState();
            for (var i = 0; i < sample.Fields.Count; i++)
            {
                entry.Values[sample.Fields[i].Key] = i < values.Length ? values[i] : null;
            }

            exercise.Entries.Add(entry);
        }

        return exercise;
    }

    /// <summary>Strength exercises with filled sets, including a hold and an assisted machine.</summary>
    public static List<ExerciseState> StrengthWorkout() =>
    [
        Exercise(6, ["8", "60"], ["8", "60"], ["6", "62.5"]),
        Exercise(218, ["10", "25"], ["8", "25"]),
        Exercise(258, ["0:01:00", null]),
        Exercise(187, ["40", "32", null]),
    ];

    /// <summary>Cardio exercises with filled entries, including pace.</summary>
    public static List<ExerciseState> CardioWorkout() =>
    [
        Exercise(300, ["0:20:00", "3.2", "1.5", "210", "9.6"]),
        Exercise(309, ["0:15:00", "60", "8", "150", null]),
        Exercise(311, ["0:25:30", "5", null, "5:06"]),
    ];

    /// <summary>A workout after a failed save: a set missing reps, an exercise with no sets, a cardio entry missing duration.</summary>
    public static List<ExerciseState> WorkoutWithErrors()
    {
        List<ExerciseState> exercises =
        [
            Exercise(222, ["10", "50"], [null, "50"]),
            Exercise(132),
            Exercise(305, [null, "2000", "5", null, null]),
        ];
        Validate(exercises, out _);
        return exercises;
    }

    /// <summary>Saved workouts relative to <paramref name="now"/>, for the prototype.</summary>
    public static List<SavedWorkout> SavedWorkouts(DateTime now)
    {
        var today = now.Date;
        return
        [
            new(1, today.AddDays(-9).AddHours(17.75), today.AddDays(-9).AddHours(18.9), StrengthWorkout()),
            new(2, today.AddDays(-3).AddHours(7.5), today.AddDays(-3).AddHours(8.3), CardioWorkout()),
            new(3, today.AddDays(-1).AddHours(18.1), today.AddDays(-1).AddHours(19.2), StrengthWorkout()),
        ];
    }

    /// <summary>History rows for the static stories, newest first.</summary>
    public static IReadOnlyList<HistoryRow> HistoryRows { get; } =
    [
        new(4, new DateTime(2026, 10, 20, 18, 5, 0)),
        new(3, new DateTime(2026, 10, 20, 7, 30, 0)),
        new(2, new DateTime(2026, 10, 19, 17, 45, 0)),
        new(1, new DateTime(2026, 10, 16, 18, 10, 0)),
    ];

    /// <summary>Dashboard values: latest start, and counts from Monday 00:00 and from the 1st of the month.</summary>
    public static DashboardSummary Summary(IReadOnlyCollection<SavedWorkout> saved, DateTime now)
    {
        var weekStart = now.Date.AddDays(-(((int)now.DayOfWeek + 6) % 7));
        var monthStart = new DateTime(now.Year, now.Month, 1);
        return new DashboardSummary(
            saved.Count == 0 ? null : saved.Max(w => w.StartedAt),
            saved.Count(w => w.StartedAt >= weekStart),
            saved.Count(w => w.StartedAt >= monthStart));
    }

    /// <summary>FR-09 save rules and the value rules in docs/03. Sets <see cref="ExerciseState.Error"/> and <see cref="EntryState.Errors"/>; returns false if anything fails.</summary>
    public static bool Validate(IReadOnlyList<ExerciseState> exercises, out string? workoutError)
    {
        workoutError = exercises.Count == 0 ? "Add at least one exercise" : null;
        var valid = workoutError is null;
        foreach (var exercise in exercises)
        {
            exercise.Error = exercise.Entries.Count == 0 ? "Add at least one set" : null;
            valid &= exercise.Error is null;
            foreach (var entry in exercise.Entries)
            {
                valid &= ValidateSet(exercise, entry);
            }
        }

        return valid;
    }

    /// <summary>Save rules for one set or entry. Sets <see cref="EntryState.Errors"/>; returns false if any field fails.</summary>
    public static bool ValidateSet(ExerciseState exercise, EntryState entry)
    {
        entry.Errors.Clear();
        foreach (var field in exercise.Fields)
        {
            if (FieldError(field, entry.Values.GetValueOrDefault(field.Key)) is { } message)
            {
                entry.Errors[field.Key] = message;
            }
        }

        return entry.Errors.Count == 0;
    }

    /// <summary>Deep copy, so edits can be discarded.</summary>
    public static List<ExerciseState> Clone(IEnumerable<ExerciseState> exercises) =>
        exercises.Select(e =>
        {
            var copy = new ExerciseState(e.Name, e.Kind, e.Fields);
            foreach (var entry in e.Entries)
            {
                var entryCopy = new EntryState();
                foreach (var (key, value) in entry.Values)
                {
                    entryCopy.Values[key] = value;
                }

                copy.Entries.Add(entryCopy);
            }

            return copy;
        }).ToList();

    private static SampleExercise Find(int id) => Exercises.Single(e => e.Id == id);

    private static FieldSpec Distance(string unit) => new("distance", "Distance", FieldKind.Decimal, Unit: unit);

    private static FieldSpec Level(string label) => new("level", label, FieldKind.Whole);

    private static FieldSpec Rate(string unit) => new("rate", "Rate", FieldKind.Decimal, Unit: unit);

    private static string SearchText(SampleExercise exercise) =>
        string.Join(' ', SearchWords($"{exercise.Name} {string.Join(' ', exercise.Aliases)}"));

    private static string[] SearchWords(string text) =>
        text.ToLowerInvariant().Replace('-', ' ').Split(' ', StringSplitOptions.RemoveEmptyEntries);

    private static bool IsFilled(string? value) => value is not null && value.Any(char.IsDigit);

    private static string? FieldError(FieldSpec field, string? value)
    {
        if (!IsFilled(value))
        {
            return field.Required ? $"Enter {field.Label.ToLowerInvariant()}" : null;
        }

        return field.Kind switch
        {
            FieldKind.Duration or FieldKind.Pace => TimeError(field, value!),
            _ when field.Key == "reps" && decimal.TryParse(value, out var reps) && reps <= 0 => $"{field.Label} must be more than 0",
            _ => null,
        };
    }

    // Duration (h:mm:ss): minutes and seconds 00–59, total more than 0. Pace (m:ss): seconds 00–59.
    private static string? TimeError(FieldSpec field, string value)
    {
        var parts = value.Split(':').Select(p => int.TryParse(p, out var n) ? n : 0).ToArray();
        if (field.Kind == FieldKind.Pace)
        {
            return parts[^1] > 59 ? $"{field.Label} seconds must be 00–59" : null;
        }

        if (parts.Skip(1).Any(p => p > 59))
        {
            return $"{field.Label} minutes and seconds must be 00–59";
        }

        return parts.All(p => p == 0) ? $"{field.Label} must be more than 0" : null;
    }
}
