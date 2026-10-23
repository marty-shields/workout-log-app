using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using Microsoft.Extensions.Time.Testing;
using WorkoutLog.Core.Domain.Exercises;
using WorkoutLog.Core.Domain.Workouts;
using WorkoutLog.Core.Domain.Workouts.Validation;
using WorkoutLog.Core.Services;
using WorkoutLog.UI.Models.Workout;
using WorkoutLog.UI.Stories.Samples;

namespace WorkoutLog.UI.Tests.Models.Workout;

public class SetCheckerTests
{
    private static readonly IExerciseCatalog Catalog = new ExerciseCatalog();

    private const int Squat = 6;
    private const int FarmersWalk = 187;
    private const int AssistedPullUp = 218;
    private const int Plank = 258;
    private const int Treadmill = 300;

    private RecordingWorkoutService workouts = null!;
    private SetChecker checker = null!;
    private FakeLogger<SetChecker> logger = null!;

    [SetUp]
    public void CreateChecker()
    {
        workouts = new RecordingWorkoutService(new SampleWorkoutService(new FakeTimeProvider(), Catalog));
        logger = new FakeLogger<SetChecker>();
        checker = new SetChecker(workouts, Catalog, logger);
    }

    [TestCase("8", "22.75")]
    [TestCase("8", ".5")]
    [TestCase("08", "007.5")]
    [TestCase("8", null)]
    [TestCase("8", "")]
    public void Check_ValidSet_PassesWithNoErrors(string reps, string? weight)
    {
        var (valid, errors) = Check(Squat, reps, weight);

        Assert.Multiple(() =>
        {
            Assert.That(valid, Is.True);
            Assert.That(errors, Is.Empty);
        });
    }

    [Test]
    public void Check_SendsTheValuesRead()
    {
        Check(FarmersWalk, "40.5", ".25", "1::");

        Assert.That(workouts.Sets, Is.EqualTo(new[] { (FarmersWalk, new StrengthSet(null, 0.25m, TimeSpan.FromHours(1), 40.5m)) }));
    }

    [TestCase("5.")]
    [TestCase(" 20")]
    [TestCase("20 ")]
    [TestCase("  ")]
    [TestCase("+5")]
    [TestCase("-5")]
    [TestCase("1,000")]
    [TestCase("22,75")]
    [TestCase("1e3")]
    [TestCase(".")]
    [TestCase("abc")]
    [TestCase("٣")]
    public void Check_UnreadableNumber_MustBeANumber(string weight)
    {
        var (valid, errors) = Check(Squat, "8", weight);

        Assert.Multiple(() =>
        {
            Assert.That(valid, Is.False);
            Assert.That(errors, Is.EqualTo(new Dictionary<string, string> { ["weight"] = "Weight must be a number" }));
        });
    }

    [TestCase("22.755")]
    [TestCase("22.500")]
    public void Check_MoreThanTwoDecimalPlaces_IsAnError(string weight)
    {
        Assert.That(Check(Squat, "8", weight).Errors,
            Is.EqualTo(new Dictionary<string, string> { ["weight"] = "Weight can have up to 2 decimal places" }));
    }

    [Test]
    public void Check_DistanceWithMoreThanTwoDecimalPlaces_IsAnError()
    {
        Assert.That(Check(FarmersWalk, "40.125").Errors,
            Is.EqualTo(new Dictionary<string, string> { ["distance"] = "Distance can have up to 2 decimal places" }));
    }

    [TestCase("8.0")]
    [TestCase("8.5")]
    [TestCase(".5")]
    public void Check_PointInAWholeNumber_MustBeAWholeNumber(string reps)
    {
        Assert.That(Check(Squat, reps).Errors,
            Is.EqualTo(new Dictionary<string, string> { ["reps"] = "Reps must be a whole number" }));
    }

    [Test]
    public void Check_WholeNumberEndingInAPoint_MustBeANumber()
    {
        Assert.That(Check(Squat, "8.").Errors,
            Is.EqualTo(new Dictionary<string, string> { ["reps"] = "Reps must be a number" }));
    }

    [TestCase(null, "Enter reps")]
    [TestCase("0", "Reps must be more than 0")]
    [TestCase("1001", "Reps must be 1,000 or less")]
    [TestCase("99999999999", "Reps must be 1,000 or less")]
    public void Check_RepsFailingCore_IsWorded(string? reps, string message)
    {
        Assert.That(Check(Squat, reps).Errors, Is.EqualTo(new Dictionary<string, string> { ["reps"] = message }));
    }

    [TestCase("1000.01")]
    [TestCase("9999999999999999999999999999999999999999")]
    public void Check_WeightAboveTheLimit_IsTooLarge(string weight)
    {
        Assert.That(Check(Squat, "8", weight).Errors,
            Is.EqualTo(new Dictionary<string, string> { ["weight"] = "Weight must be 1,000 or less" }));
    }

    [Test]
    public void Check_AtTheLimits_IsValid()
    {
        Assert.Multiple(() =>
        {
            Assert.That(Check(Squat, "1000", "1000").Valid, Is.True);
            Assert.That(Check(FarmersWalk, "100000", null, "23:59:59").Valid, Is.True);
        });
    }

    [Test]
    public void Check_UsesTheExercisesLabel()
    {
        Assert.Multiple(() =>
        {
            Assert.That(Check(AssistedPullUp, "8", "abc").Errors,
                Is.EqualTo(new Dictionary<string, string> { ["weight"] = "Assist must be a number" }));
            Assert.That(Check(AssistedPullUp, "8", "1001").Errors,
                Is.EqualTo(new Dictionary<string, string> { ["weight"] = "Assist must be 1,000 or less" }));
        });
    }

    [Test]
    public void Check_FormatErrorOnARequiredField_IsTheOnlyMessage()
    {
        Assert.That(Check(Squat, "abc").Errors,
            Is.EqualTo(new Dictionary<string, string> { ["reps"] = "Reps must be a number" }));
    }

    [TestCase(null)]
    [TestCase("")]
    [TestCase("::")]
    public void Check_EmptyRequiredDuration_EnterDuration(string? duration)
    {
        Assert.That(Check(Plank, duration).Errors,
            Is.EqualTo(new Dictionary<string, string> { ["duration"] = "Enter duration" }));
    }

    [TestCase("1::")]
    [TestCase("::30")]
    [TestCase(":5:")]
    [TestCase("0:01:00")]
    public void Check_PartlyFilledDuration_EmptyBoxesAreZero(string duration)
    {
        Assert.That(Check(Plank, duration).Valid, Is.True);
    }

    [TestCase("0:00:00", "Duration must be more than 0")]
    [TestCase("0:60:00", "Duration minutes and seconds must be 00–59")]
    [TestCase("0:00:75", "Duration minutes and seconds must be 00–59")]
    [TestCase("24:00:00", "Duration must be 23:59:59 or less")]
    [TestCase("99:59:59", "Duration must be 23:59:59 or less")]
    [TestCase("0:-1:00", "Duration must be a time")]
    [TestCase("0:1.:00", "Duration must be a time")]
    [TestCase("0:00: 5", "Duration must be a time")]
    [TestCase("0:00:0:0", "Duration must be a time")]
    [TestCase("0:00", "Duration must be a time")]
    public void Check_BadDuration_IsWorded(string duration, string message)
    {
        Assert.That(Check(Plank, duration).Errors, Is.EqualTo(new Dictionary<string, string> { ["duration"] = message }));
    }

    [Test]
    public void Check_Carry_OneMessagePerFieldAtFault()
    {
        Assert.That(Check(FarmersWalk, null, "32", "0:00:75").Errors, Is.EqualTo(new Dictionary<string, string>
        {
            ["distance"] = "Enter distance",
            ["duration"] = "Duration minutes and seconds must be 00–59",
        }));
    }

    [TestCase("0", "Distance must be more than 0")]
    [TestCase("100001", "Distance must be 100,000 or less")]
    public void Check_CarryDistanceFailingCore_IsWorded(string distance, string message)
    {
        Assert.That(Check(FarmersWalk, distance).Errors, Is.EqualTo(new Dictionary<string, string> { ["distance"] = message }));
    }

    [Test]
    public void Check_CarryOptionalDurationOfZero_IsValid()
    {
        Assert.That(Check(FarmersWalk, "40", null, "0:00:00").Valid, Is.True);
    }

    [Test]
    public void Check_ReplacesOldErrors()
    {
        var exercise = SampleData.Exercise(Catalog, Squat, ["8", "60"]);
        var entry = exercise.Entries.Single();
        entry.Errors["reps"] = "Enter reps";

        Assert.Multiple(() =>
        {
            Assert.That(checker.Check(exercise, entry), Is.True);
            Assert.That(entry.Errors, Is.Empty);
        });
    }

    [Test]
    public void Check_Cardio_PassesWithNoChecks()
    {
        var exercise = SampleData.Exercise(Catalog, Treadmill, [null, "abc"]);
        var entry = exercise.Entries.Single();
        entry.Errors["duration"] = "Enter duration";

        Assert.Multiple(() =>
        {
            Assert.That(checker.Check(exercise, entry), Is.True);
            Assert.That(entry.Errors, Is.Empty);
            Assert.That(workouts.Sets, Is.Empty);
        });
    }

    [Test]
    public void Check_FieldErrorWithNoMessage_FailsAndLogsAWarning()
    {
        var exercise = SampleData.Exercise(Catalog, Squat, ["8"]);
        var notShown = new ExerciseErrors(null, [new EntryErrors(new Dictionary<Metric, FieldError> { [Metric.Distance] = FieldError.NotShown })]);
        var stub = new SetChecker(new RecordingWorkoutService(workouts, notShown), Catalog, logger);

        Assert.Multiple(() =>
        {
            Assert.That(stub.Check(exercise, exercise.Entries[0]), Is.False);
            Assert.That(exercise.Entries[0].Errors, Is.Empty);
            Assert.That(LoggedEvents.Of(logger), Is.EqualTo(new[]
            {
                (2502, LogLevel.Warning, $"Set check for exercise {Squat} gave NotShown for Distance and no message"),
            }));
        });
    }

    [TestCase(ExerciseError.UnknownExercise)]
    [TestCase(ExerciseError.WrongType)]
    public void Check_ExerciseError_FailsAndLogsAWarning(ExerciseError error)
    {
        var exercise = SampleData.Exercise(Catalog, Squat, ["8"]);
        var stub = new SetChecker(new RecordingWorkoutService(workouts, new ExerciseErrors(error, [])), Catalog, logger);

        Assert.Multiple(() =>
        {
            Assert.That(stub.Check(exercise, exercise.Entries[0]), Is.False);
            Assert.That(exercise.Entries[0].Errors, Is.Empty);
            Assert.That(LoggedEvents.Of(logger), Is.EqualTo(new[]
            {
                (2501, LogLevel.Warning, $"Set check for exercise {Squat} failed with {error} and no message"),
            }));
        });
    }

    [Test]
    public void Check_WordedAndFormatErrors_LogNothing()
    {
        Check(Squat, null, "abc");
        Check(Squat, "abc");
        Check(FarmersWalk, null, "32", "0:00:75");

        Assert.That(LoggedEvents.Of(logger), Is.Empty);
    }

    private (bool Valid, Dictionary<string, string> Errors) Check(int exerciseId, params string?[] values)
    {
        var exercise = SampleData.Exercise(Catalog, exerciseId, values);
        var entry = exercise.Entries.Single();
        var valid = checker.Check(exercise, entry);
        return (valid, entry.Errors);
    }

    /// <summary>Records each set checked, and passes it to <paramref name="inner"/> or returns <paramref name="result"/>.</summary>
    private sealed class RecordingWorkoutService(IWorkoutService inner, ExerciseErrors? result = null) : IWorkoutService
    {
        public List<(int ExerciseId, StrengthSet Set)> Sets { get; } = [];

        public WorkoutDraft StartNew() => inner.StartNew();

        public WorkoutErrors Validate(IReadOnlyList<WorkoutExercise> exercises) => inner.Validate(exercises);

        public ExerciseErrors ValidateSet(int exerciseId, StrengthSet set)
        {
            Sets.Add((exerciseId, set));
            return result ?? inner.ValidateSet(exerciseId, set);
        }

        public ExerciseErrors ValidateEntry(int exerciseId, CardioEntry entry) => inner.ValidateEntry(exerciseId, entry);

        public Task<SaveResult> SaveNewAsync(WorkoutDraft draft, IReadOnlyList<WorkoutExercise> exercises) => inner.SaveNewAsync(draft, exercises);

        public Task<SaveResult> SaveAsync(int workoutId, IReadOnlyList<WorkoutExercise> exercises) => inner.SaveAsync(workoutId, exercises);
    }
}
