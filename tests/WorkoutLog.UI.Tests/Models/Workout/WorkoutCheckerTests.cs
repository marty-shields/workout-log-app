using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using Microsoft.Extensions.Time.Testing;
using WorkoutLog.Core.Domain.Exercises;
using WorkoutLog.Core.Domain.Workouts.Validation;
using WorkoutLog.UI.Logging;
using WorkoutLog.UI.Models.Fields;
using WorkoutLog.UI.Models.Workout;
using WorkoutLog.UI.Stories.Samples;

namespace WorkoutLog.UI.Tests.Models.Workout;

public class WorkoutCheckerTests
{
    private static readonly IExerciseCatalog Catalog = new ExerciseCatalog();

    private const int Squat = 6;
    private const int Treadmill = 300;
    private const int OutdoorRun = 311;

    private FakeLogger<SetChecker> setLogger = null!;
    private FakeLogger<WorkoutChecker> logger = null!;
    private WorkoutChecker checker = null!;

    [SetUp]
    public void CreateChecker()
    {
        setLogger = new FakeLogger<SetChecker>();
        logger = new FakeLogger<WorkoutChecker>();
        var workouts = new SampleWorkoutService(new FakeTimeProvider(), Catalog);
        checker = new WorkoutChecker(new SetChecker(workouts, Catalog, setLogger), workouts, logger);
    }

    [Test]
    public void Check_NoExercises_AddAtLeastOneExercise()
    {
        Assert.That(checker.Check([]), Is.EqualTo(new WorkoutCheck(false, "Add at least one exercise")));
    }

    [Test]
    public void SaveFailed_CouldntSaveTheWorkout()
    {
        Assert.That(checker.SaveFailed, Is.EqualTo("Couldn't save the workout. Try again."));
    }

    [Test]
    public void Check_ValidWorkout_IsValidAndClearsOldErrors()
    {
        var squat = SampleData.Exercise(Catalog, Squat, ["8", "60"]);
        var run = SampleData.Exercise(Catalog, OutdoorRun, ["0:25:30", "5", null, "5:30"]);
        squat.Error = "Add at least one set";
        run.Entries[0].Errors["duration"] = "Enter duration";

        var result = checker.Check([squat, run]);

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.EqualTo(new WorkoutCheck(true, null)));
            Assert.That(squat.Error, Is.Null);
            Assert.That(run.Entries[0].Errors, Is.Empty);
        });
    }

    [Test]
    public void Check_Valid_ClearsCheckOnChange_AndInvalidKeepsIt()
    {
        var squat = SampleData.Exercise(Catalog, Squat, ["8", "60"]);
        squat.Entries[0].CheckOnChange = true;
        var other = SampleData.Exercise(Catalog, Squat, ["8", "60"]);
        other.Entries[0].CheckOnChange = true;

        checker.Check([squat]);
        checker.Check([other, SampleData.Exercise(Catalog, Treadmill, [null, "3.2"])]);

        Assert.Multiple(() =>
        {
            Assert.That(squat.Entries[0].CheckOnChange, Is.False);
            Assert.That(other.Entries[0].CheckOnChange, Is.True);
        });
    }

    [Test]
    public void Check_StrengthWithNoSets_AddAtLeastOneSet()
    {
        var squat = SampleData.Exercise(Catalog, Squat);

        Assert.Multiple(() =>
        {
            Assert.That(checker.Check([squat]), Is.EqualTo(new WorkoutCheck(false, null)));
            Assert.That(squat.Error, Is.EqualTo("Add at least one set"));
        });
    }

    [Test]
    public void Check_ChecksEverySetAndEntry()
    {
        var squat = SampleData.Exercise(Catalog, Squat, ["8", "60"], [null, "60"], ["8", "60.255"]);
        var treadmill = SampleData.Exercise(Catalog, Treadmill, [null, "3.2", "1.255"]);

        var result = checker.Check([squat, treadmill]);

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.EqualTo(new WorkoutCheck(false, null)));
            Assert.That(squat.Error, Is.Null);
            Assert.That(squat.Entries.Select(e => e.Errors), Is.EqualTo(new[]
            {
                new Dictionary<string, string>(),
                new Dictionary<string, string> { ["reps"] = "Enter reps" },
                new Dictionary<string, string> { ["weight"] = "Weight can have up to 2 decimal places" },
            }));
            Assert.That(treadmill.Entries[0].Errors, Is.EqualTo(new Dictionary<string, string>
            {
                ["duration"] = "Enter duration",
                ["incline"] = "Incline can have up to 2 decimal places",
            }));
        });
    }

    [Test]
    public void Check_UnknownExercise_ExerciseNotFoundAndLoggedOnce()
    {
        var unknown = new ExerciseState(9_999, "Gone", ExerciseKind.Strength, [new FieldSpec("reps", Metric.Reps, "Reps", FieldKind.Whole, Required: true)]);
        unknown.Entries.Add(new EntryState { Values = { ["reps"] = "8" } });
        unknown.Entries.Add(new EntryState { Values = { ["reps"] = "6" } });

        Assert.Multiple(() =>
        {
            Assert.That(checker.Check([unknown]), Is.EqualTo(new WorkoutCheck(false, null)));
            Assert.That(unknown.Error, Is.EqualTo("Exercise not found"));
            Assert.That(unknown.ErrorCode, Is.EqualTo(ExerciseError.UnknownExercise));
            Assert.That(LoggedEvents.Of(logger), Is.EqualTo(new[]
            {
                (UiEvent.SaveCheckExerciseError, LogLevel.Warning, "Save check for exercise 9999 failed with UnknownExercise"),
            }));
            Assert.That(LoggedEvents.Of(setLogger), Is.Empty);
        });
    }

    [Test]
    public void Check_NoSets_SetsTheCodeAndLogsNothing()
    {
        var squat = SampleData.Exercise(Catalog, Squat);

        checker.Check([squat]);

        Assert.Multiple(() =>
        {
            Assert.That(squat.ErrorCode, Is.EqualTo(ExerciseError.NoSets));
            Assert.That(LoggedEvents.Of(logger), Is.Empty);
        });
    }

    [Test]
    public void Check_CardioWithNoEntry_EnterDuration()
    {
        var treadmill = SampleData.Exercise(Catalog, Treadmill, []);

        Assert.Multiple(() =>
        {
            Assert.That(checker.Check([treadmill]).IsValid, Is.False);
            Assert.That(treadmill.Error, Is.Null);
        });
    }

    [Test]
    public void Check_WrongType_ExerciseTypeDoesNotMatch()
    {
        var squatAsCardio = new ExerciseState(Squat, "Barbell Back Squat", ExerciseKind.Cardio, [new FieldSpec("duration", Metric.Duration, "Duration", FieldKind.Duration, Required: true)]);
        squatAsCardio.Entries.Add(new EntryState { Values = { ["duration"] = "0:10:00" } });

        Assert.Multiple(() =>
        {
            Assert.That(checker.Check([squatAsCardio]), Is.EqualTo(new WorkoutCheck(false, null)));
            Assert.That(squatAsCardio.Error, Is.EqualTo("Exercise type doesn't match"));
            Assert.That(squatAsCardio.ErrorCode, Is.EqualTo(ExerciseError.WrongType));
            Assert.That(LoggedEvents.Of(logger), Is.EqualTo(new[]
            {
                (UiEvent.SaveCheckExerciseError, LogLevel.Warning, $"Save check for exercise {Squat} failed with WrongType"),
            }));
        });
    }
}
