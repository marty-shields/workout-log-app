using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using Microsoft.Extensions.Time.Testing;
using WorkoutLog.Core.Domain.Exercises;
using WorkoutLog.Core.Domain.Workouts;
using WorkoutLog.Core.Domain.Workouts.Validation;
using WorkoutLog.Core.Services;
using WorkoutLog.UI.Logging;
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
    private const int Rower = 305;
    private const int StairClimber = 309;
    private const int OutdoorRun = 311;

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
    [TestCase("8", "007")]
    [TestCase("8", "0")]
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
    public void Check_Failing_SetsCheckOnChange_AndPassingKeepsIt()
    {
        var exercise = SampleData.Exercise(Catalog, Squat, ["abc"]);
        var entry = exercise.Entries.Single();

        checker.Check(exercise, entry);
        var afterFail = entry.CheckOnChange;
        entry.Values["reps"] = "8";
        checker.Check(exercise, entry);

        Assert.Multiple(() =>
        {
            Assert.That(afterFail, Is.True);
            Assert.That(entry.CheckOnChange, Is.True);
        });
    }

    [Test]
    public void Check_Passing_LeavesCheckOnChangeUnset()
    {
        var exercise = SampleData.Exercise(Catalog, Squat, ["8"]);

        checker.Check(exercise, exercise.Entries[0]);

        Assert.That(exercise.Entries[0].CheckOnChange, Is.False);
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
    public void Check_ValidCardioEntries_PassAndSendTheValuesRead()
    {
        Assert.Multiple(() =>
        {
            Assert.That(Check(OutdoorRun, "0:25:30", "5.25", "310", "5:06").Valid, Is.True);
            Assert.That(Check(StairClimber, "0:15:00", "60", "8", "150", "90.5").Valid, Is.True);
        });
        Assert.That(workouts.Entries, Is.EqualTo(new[]
        {
            (OutdoorRun, new CardioEntry(new TimeSpan(0, 25, 30), 5.25m, null, null, null, 310, null, new TimeSpan(0, 5, 6))),
            (StairClimber, new CardioEntry(TimeSpan.FromMinutes(15), null, 60, 8, null, 150, 90.5m, null)),
        }));
        Assert.That(workouts.Sets, Is.Empty);
    }

    [TestCase(null, "Enter duration")]
    [TestCase("0:00:00", "Duration must be more than 0")]
    [TestCase("0:00:75", "Duration minutes and seconds must be 00–59")]
    [TestCase("24:00:00", "Duration must be 23:59:59 or less")]
    [TestCase("0:1a:00", "Duration must be a time")]
    public void Check_BadCardioDuration_IsWorded(string? duration, string message)
    {
        Assert.That(Check(Treadmill, duration).Errors, Is.EqualTo(new Dictionary<string, string> { ["duration"] = message }));
    }

    [TestCase(Treadmill, "incline", "1.255", "Incline can have up to 2 decimal places")]
    [TestCase(Treadmill, "rate", "9.555", "Rate can have up to 2 decimal places")]
    [TestCase(Treadmill, "distance", "3,2", "Distance must be a number")]
    [TestCase(Rower, "level", "5.5", "Damper must be a whole number")]
    [TestCase(Rower, "calories", "abc", "Calories must be a number")]
    [TestCase(Rower, "calories", "-1", "Calories must be a number")]
    [TestCase(StairClimber, "floors", "8.", "Floors must be a number")]
    [TestCase(StairClimber, "level", ".5", "Speed level must be a whole number")]
    public void Check_UnreadableCardioField_IsWordedWithItsLabel(int exerciseId, string key, string value, string message)
    {
        Assert.That(CheckField(exerciseId, key, value).Errors, Is.EqualTo(new Dictionary<string, string> { [key] = message }));
    }

    [TestCase(Treadmill, "distance", "1000.01", "Distance must be 1,000 or less")]
    [TestCase(Treadmill, "incline", "100.01", "Incline must be 100 or less")]
    [TestCase(Treadmill, "calories", "10001", "Calories must be 10,000 or less")]
    [TestCase(Treadmill, "rate", "100.5", "Rate must be 100 or less")]
    [TestCase(Rower, "distance", "100001", "Distance must be 100,000 or less")]
    [TestCase(Rower, "rate", "201", "Rate must be 200 or less")]
    [TestCase(StairClimber, "floors", "1001", "Floors must be 1,000 or less")]
    [TestCase(StairClimber, "floors", "99999999999", "Floors must be 1,000 or less")]
    [TestCase(StairClimber, "level", "101", "Speed level must be 100 or less")]
    [TestCase(StairClimber, "rate", "301", "Rate must be 300 or less")]
    public void Check_CardioFieldAboveTheLimit_IsTooLarge(int exerciseId, string key, string value, string message)
    {
        Assert.That(CheckField(exerciseId, key, value).Errors, Is.EqualTo(new Dictionary<string, string> { [key] = message }));
    }

    [Test]
    public void Check_CardioAtTheLimitsAndZero_IsValid()
    {
        Assert.Multiple(() =>
        {
            Assert.That(Check(Treadmill, "23:59:59", "1000", "100", "10000", "100").Valid, Is.True);
            Assert.That(Check(StairClimber, "0:00:01", "0", "0", "0", "0").Valid, Is.True);
        });
    }

    [TestCase("5:30", 330)]
    [TestCase(":30", 30)]
    [TestCase("5:", 300)]
    [TestCase("0:00", 0)]
    [TestCase("60:00", 3_600)]
    public void Check_Pace_IsReadAsMinutesAndSeconds(string pace, int seconds)
    {
        var (valid, _) = CheckField(OutdoorRun, "pace", pace);

        Assert.Multiple(() =>
        {
            Assert.That(valid, Is.True);
            Assert.That(workouts.Entries.Single().Entry.Pace, Is.EqualTo(TimeSpan.FromSeconds(seconds)));
        });
    }

    [TestCase(null)]
    [TestCase("")]
    [TestCase(":")]
    public void Check_EmptyPace_IsValidAndEmpty(string? pace)
    {
        var (valid, _) = CheckField(OutdoorRun, "pace", pace);

        Assert.Multiple(() =>
        {
            Assert.That(valid, Is.True);
            Assert.That(workouts.Entries.Single().Entry.Pace, Is.Null);
        });
    }

    [TestCase("5:75", "Pace seconds must be 00–59")]
    [TestCase("5:60", "Pace seconds must be 00–59")]
    [TestCase("60:01", "Pace must be 60:00 or less")]
    [TestCase("61:00", "Pace must be 60:00 or less")]
    [TestCase("99:59", "Pace must be 60:00 or less")]
    [TestCase("99999999:00", "Pace must be 60:00 or less")]
    [TestCase("9999999999:00", "Pace must be a time")]
    [TestCase("5:3a", "Pace must be a time")]
    [TestCase("-5:30", "Pace must be a time")]
    [TestCase("5", "Pace must be a time")]
    [TestCase("0:05:30", "Pace must be a time")]
    public void Check_BadPace_IsWorded(string pace, string message)
    {
        Assert.That(CheckField(OutdoorRun, "pace", pace).Errors, Is.EqualTo(new Dictionary<string, string> { ["pace"] = message }));
    }

    [Test]
    public void Check_NotWholeSeconds_IsWorded()
    {
        var plank = SampleData.Exercise(Catalog, Plank, ["0:01:00"]);
        var run = SampleData.Exercise(Catalog, OutdoorRun, ["0:25:30", null, null, "5:30"]);
        var codes = new ExerciseErrors(null, [new EntryErrors(new Dictionary<Metric, FieldError>
        {
            [Metric.Duration] = FieldError.NotWholeSeconds,
            [Metric.Pace] = FieldError.NotWholeSeconds,
        })]);
        var stub = new SetChecker(new RecordingWorkoutService(workouts, codes), Catalog, logger);

        Assert.Multiple(() =>
        {
            Assert.That(stub.Check(plank, plank.Entries[0]), Is.False);
            Assert.That(plank.Entries[0].Errors, Is.EqualTo(new Dictionary<string, string> { ["duration"] = "Duration must be whole seconds" }));
            Assert.That(stub.Check(run, run.Entries[0]), Is.False);
            Assert.That(run.Entries[0].Errors, Is.EqualTo(new Dictionary<string, string>
            {
                ["duration"] = "Duration must be whole seconds",
                ["pace"] = "Pace must be whole seconds",
            }));
        });
    }

    [Test]
    public void Check_CardioFieldErrorWithNoMessage_FailsAndLogsAWarning()
    {
        var exercise = SampleData.Exercise(Catalog, OutdoorRun, ["0:25:30"]);
        var notShown = new ExerciseErrors(null, [new EntryErrors(new Dictionary<Metric, FieldError> { [Metric.Rate] = FieldError.NotShown })]);
        var stub = new SetChecker(new RecordingWorkoutService(workouts, notShown), Catalog, logger);

        Assert.Multiple(() =>
        {
            Assert.That(stub.Check(exercise, exercise.Entries[0]), Is.False);
            Assert.That(exercise.Entries[0].Errors, Is.Empty);
            Assert.That(LoggedEvents.Of(logger), Is.EqualTo(new[]
            {
                (UiEvent.SetCheckFieldUnworded, LogLevel.Warning, $"Set check for exercise {OutdoorRun} gave NotShown for Rate and no message"),
            }));
        });
    }

    [Test]
    public void Read_Strength_ReturnsEverySetWithUnreadableFieldsEmpty()
    {
        var exercise = SampleData.Exercise(Catalog, FarmersWalk, ["40.5", "32", "0:00:45"], ["abc", "32.555", "0:00:75"], [null, null, null]);

        Assert.That(checker.Read(exercise), Is.EqualTo(new StrengthExercise(FarmersWalk,
        [
            new StrengthSet(null, 32m, TimeSpan.FromSeconds(45), 40.5m),
            new StrengthSet(null, null, null, null),
            new StrengthSet(null, null, null, null),
        ])).UsingPropertiesComparer());
    }

    [Test]
    public void Read_StrengthWithNoSets_ReturnsNoSets()
    {
        Assert.That(checker.Read(SampleData.Exercise(Catalog, Squat)), Is.EqualTo(new StrengthExercise(Squat, [])).UsingPropertiesComparer());
    }

    [Test]
    public void Read_Cardio_ReturnsTheEntry()
    {
        var exercise = SampleData.Exercise(Catalog, OutdoorRun, ["0:25:30", "5", "abc", "5:75"]);

        Assert.That(checker.Read(exercise),
            Is.EqualTo(new CardioExercise(OutdoorRun, new CardioEntry(new TimeSpan(0, 25, 30), 5m, null, null, null, null, null, null))));
    }

    [Test]
    public void Read_ChangesNoErrorsAndLogsNothing()
    {
        var exercise = SampleData.Exercise(Catalog, Squat, ["abc", "60"]);

        checker.Read(exercise);

        Assert.Multiple(() =>
        {
            Assert.That(exercise.Entries[0].Errors, Is.Empty);
            Assert.That(LoggedEvents.Of(logger), Is.Empty);
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
                (UiEvent.SetCheckFieldUnworded, LogLevel.Warning, $"Set check for exercise {Squat} gave NotShown for Distance and no message"),
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
                (UiEvent.SetCheckExerciseError, LogLevel.Warning, $"Set check for exercise {Squat} failed with {error}"),
            }));
        });
    }

    [Test]
    public void CheckOnSave_ExerciseError_FailsWithoutLogging()
    {
        var exercise = SampleData.Exercise(Catalog, Squat, ["8"]);
        var stub = new SetChecker(new RecordingWorkoutService(workouts, new ExerciseErrors(ExerciseError.UnknownExercise, [])), Catalog, logger);

        Assert.Multiple(() =>
        {
            Assert.That(stub.CheckOnSave(exercise, exercise.Entries[0]), Is.False);
            Assert.That(LoggedEvents.Of(logger), Is.Empty);
        });
    }

    [Test]
    public void Read_CardioWithNoEntry_ReturnsAnEmptyEntry()
    {
        Assert.That(checker.Read(SampleData.Exercise(Catalog, OutdoorRun, [])),
            Is.EqualTo(new CardioExercise(OutdoorRun, new CardioEntry(null, null, null, null, null, null, null, null))));
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

    /// <summary>Checks a cardio entry with a valid duration and <paramref name="value"/> in the field <paramref name="key"/>.</summary>
    private (bool Valid, Dictionary<string, string> Errors) CheckField(int exerciseId, string key, string? value)
    {
        var exercise = SampleData.Exercise(Catalog, exerciseId, []);
        exercise.Entries.Add(new EntryState { Values = { ["duration"] = "0:10:00", [key] = value } });
        var entry = exercise.Entries.Single();
        var valid = checker.Check(exercise, entry);
        return (valid, entry.Errors);
    }

    /// <summary>Records each set and entry checked, and passes it to <paramref name="inner"/> or returns <paramref name="result"/>.</summary>
    private sealed class RecordingWorkoutService(IWorkoutService inner, ExerciseErrors? result = null) : IWorkoutService
    {
        public List<(int ExerciseId, StrengthSet Set)> Sets { get; } = [];

        public List<(int ExerciseId, CardioEntry Entry)> Entries { get; } = [];

        public WorkoutDraft StartNew() => inner.StartNew();

        public WorkoutErrors Validate(IReadOnlyList<WorkoutExercise> exercises) => inner.Validate(exercises);

        public ExerciseErrors ValidateSet(int exerciseId, StrengthSet set)
        {
            Sets.Add((exerciseId, set));
            return result ?? inner.ValidateSet(exerciseId, set);
        }

        public ExerciseErrors ValidateEntry(int exerciseId, CardioEntry entry)
        {
            Entries.Add((exerciseId, entry));
            return result ?? inner.ValidateEntry(exerciseId, entry);
        }

        public Task<SaveResult> SaveNewAsync(WorkoutDraft draft, IReadOnlyList<WorkoutExercise> exercises) => inner.SaveNewAsync(draft, exercises);

        public Task<WorkoutResult> GetAsync(int workoutId) => inner.GetAsync(workoutId);

        public Task<SaveResult> SaveAsync(int workoutId, IReadOnlyList<WorkoutExercise> exercises) => inner.SaveAsync(workoutId, exercises);

        public Task<WorkoutDelete> DeleteAsync(int workoutId) => inner.DeleteAsync(workoutId);
    }
}
