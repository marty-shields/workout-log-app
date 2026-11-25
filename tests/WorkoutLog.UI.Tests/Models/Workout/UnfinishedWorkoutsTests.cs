using WorkoutLog.Core.Domain.Exercises;
using WorkoutLog.Core.Domain.Workouts;
using WorkoutLog.UI.Models.Workout;

namespace WorkoutLog.UI.Tests.Models.Workout;

public class UnfinishedWorkoutsTests
{
    private static readonly IExerciseCatalog Catalog = new ExerciseCatalog();
    private static readonly DateTimeOffset Start = new(2026, 10, 28, 12, 0, 0, TimeSpan.Zero);

    private const int Squat = 6;
    private const int Treadmill = 300;

    [Test]
    public void From_ReadsTheExercisesOldestFirstWithTheirNotesDoneStatesAndTheTextAsTyped()
    {
        var squat = ExerciseFields.New(Catalog.All.Single(e => e.Id == Squat));
        squat.Note = "  tempo ";
        squat.IsDone = true;
        squat.Entries[0].Values["reps"] = "5";
        squat.Entries[0].Values["weight"] = "60";
        var second = new EntryState();
        second.Values["reps"] = "abc";
        second.Values["weight"] = null;
        squat.Entries.Add(second);
        var treadmill = ExerciseFields.New(Catalog.All.Single(e => e.Id == Treadmill));
        treadmill.Entries[0].Values["duration"] = "0:20";

        var workout = UnfinishedWorkouts.From(Start, "note", [treadmill, squat]);

        Assert.Multiple(() =>
        {
            Assert.That(workout.StartedAt, Is.EqualTo(Start));
            Assert.That(workout.Note, Is.EqualTo("note"));
            Assert.That(workout.Exercises.Select(e => (e.ExerciseId, e.Note, e.IsDone)), Is.EqualTo(new (int, string?, bool)[] { (Squat, "  tempo ", true), (Treadmill, null, false) }));
            Assert.That(workout.Exercises[0].Entries.Select(e => e.Values), Is.EqualTo(new[]
            {
                new Dictionary<string, string?> { ["reps"] = "5", ["weight"] = "60" },
                new Dictionary<string, string?> { ["reps"] = "abc", ["weight"] = null },
            }));
            Assert.That(workout.Exercises[1].Entries.Single().Values, Is.EqualTo(new Dictionary<string, string?> { ["duration"] = "0:20" }));
        });
    }

    [Test]
    public void From_LaterChangesToTheScreenDoNotChangeTheSnapshot()
    {
        var squat = ExerciseFields.New(Catalog.All.Single(e => e.Id == Squat));
        squat.Entries[0].Values["reps"] = "5";

        var workout = UnfinishedWorkouts.From(Start, null, [squat]);
        squat.Entries[0].Values["reps"] = "6";
        squat.Entries.Add(new EntryState());

        Assert.That(workout.Exercises[0].Entries.Select(e => e.Values["reps"]), Is.EqualTo(new[] { "5" }));
    }

    [Test]
    public void ToExercises_ReturnsWhatFromReadAsTyped_IncludingInvalidAndOpenValues()
    {
        var squat = ExerciseFields.New(Catalog.All.Single(e => e.Id == Squat));
        squat.Note = "tempo";
        squat.IsDone = true;
        squat.Entries[0].Values["reps"] = "-3";
        squat.Entries[0].Values["weight"] = "6x";
        squat.Entries.Add(new EntryState());
        var treadmill = ExerciseFields.New(Catalog.All.Single(e => e.Id == Treadmill));
        treadmill.Entries[0].Values["duration"] = "0:2";

        var restored = UnfinishedWorkouts.ToExercises(UnfinishedWorkouts.From(Start, null, [treadmill, squat]), Catalog);

        Assert.Multiple(() =>
        {
            Assert.That(restored.Select(e => (e.ExerciseId, e.Kind, e.Note, e.IsDone)), Is.EqualTo(new (int, ExerciseKind, string?, bool)[]
            {
                (Squat, ExerciseKind.Strength, "tempo", true),
                (Treadmill, ExerciseKind.Cardio, null, false),
            }));
            Assert.That(restored[0].Entries.Select(e => e.Values), Is.EqualTo(new[]
            {
                new Dictionary<string, string?> { ["reps"] = "-3", ["weight"] = "6x" },
                new Dictionary<string, string?>(),
            }));
            Assert.That(restored[0].Entries.SelectMany(e => e.Errors), Is.Empty, "restored without being checked");
            Assert.That(restored[0].Fields.Select(f => f.Key), Is.EqualTo(squat.Fields.Select(f => f.Key)));
            Assert.That(restored[1].Entries.Single().Values["duration"], Is.EqualTo("0:2"));
        });
    }

    [Test]
    public void ToExercises_CardioWithNoEntry_GetsItsEmptyEntry()
    {
        var workout = new UnfinishedWorkout(Start, null, [new UnfinishedExercise(Treadmill, null, false, [])]);

        Assert.That(UnfinishedWorkouts.ToExercises(workout, Catalog).Single().Entries, Has.Count.EqualTo(1));
    }

    [Test]
    public void ToExercises_StrengthWithNoSets_KeepsNone()
    {
        var workout = new UnfinishedWorkout(Start, null, [new UnfinishedExercise(Squat, null, false, [])]);

        Assert.That(UnfinishedWorkouts.ToExercises(workout, Catalog).Single().Entries, Is.Empty);
    }

    [Test]
    public void ToExercises_ExerciseNotInTheList_IsLeftOut()
    {
        var workout = new UnfinishedWorkout(Start, null, [new UnfinishedExercise(99999, null, false, []), new UnfinishedExercise(Squat, null, false, [])]);

        Assert.That(UnfinishedWorkouts.ToExercises(workout, Catalog).Select(e => e.ExerciseId), Is.EqualTo(new[] { Squat }));
    }
}
