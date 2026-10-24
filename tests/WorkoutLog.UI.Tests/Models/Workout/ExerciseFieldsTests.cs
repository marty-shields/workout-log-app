using System.Globalization;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Time.Testing;
using WorkoutLog.Core.Domain.Exercises;
using WorkoutLog.Core.Domain.Workouts;
using WorkoutLog.UI.Models.Fields;
using WorkoutLog.UI.Models.Workout;
using WorkoutLog.UI.Stories.Samples;

namespace WorkoutLog.UI.Tests.Models.Workout;

public class ExerciseFieldsTests
{
    private static readonly IExerciseCatalog Catalog = new ExerciseCatalog();

    [Test]
    public void For_RepsAndWeight() =>
        Assert.That(Fields(6), Is.EqualTo(new[]
        {
            new FieldSpec("reps", Metric.Reps, "Reps", FieldKind.Whole, Required: true),
            new FieldSpec("weight", Metric.WeightKg, "Weight", FieldKind.Decimal, Unit: "kg"),
        }));

    [Test]
    public void For_AssistedMachine_UsesTheLabelWithKg() =>
        Assert.That(Fields(218)[1], Is.EqualTo(new FieldSpec("weight", Metric.WeightKg, "Assist", FieldKind.Decimal, Unit: "kg")));

    [Test]
    public void For_Hold_RequiresDuration() =>
        Assert.That(Fields(258)[0], Is.EqualTo(new FieldSpec("duration", Metric.Duration, "Duration", FieldKind.Duration, Required: true)));

    [Test]
    public void For_Carry_RequiresDistanceInMetres() =>
        Assert.That(Fields(187), Is.EqualTo(new[]
        {
            new FieldSpec("distance", Metric.Distance, "Distance", FieldKind.Decimal, Required: true, Unit: "m"),
            new FieldSpec("weight", Metric.WeightKg, "Weight", FieldKind.Decimal, Unit: "kg"),
            new FieldSpec("duration", Metric.Duration, "Duration", FieldKind.Duration),
        }));

    [Test]
    public void For_Treadmill() =>
        Assert.That(Fields(300), Is.EqualTo(new[]
        {
            new FieldSpec("duration", Metric.Duration, "Duration", FieldKind.Duration, Required: true),
            new FieldSpec("distance", Metric.Distance, "Distance", FieldKind.Decimal, Unit: "km"),
            new FieldSpec("incline", Metric.Incline, "Incline", FieldKind.Decimal, Unit: "%"),
            new FieldSpec("calories", Metric.Calories, "Calories", FieldKind.Whole),
            new FieldSpec("rate", Metric.Rate, "Rate", FieldKind.Decimal, Unit: "km/h"),
        }));

    [Test]
    public void For_StairClimber_FloorsAndLevelLabel() =>
        Assert.That(Fields(309), Is.EqualTo(new[]
        {
            new FieldSpec("duration", Metric.Duration, "Duration", FieldKind.Duration, Required: true),
            new FieldSpec("floors", Metric.Floors, "Floors", FieldKind.Whole),
            new FieldSpec("level", Metric.Level, "Speed level", FieldKind.Whole),
            new FieldSpec("calories", Metric.Calories, "Calories", FieldKind.Whole),
            new FieldSpec("rate", Metric.Rate, "Rate", FieldKind.Decimal, Unit: "steps/min"),
        }));

    [Test]
    public void For_OutdoorRun_PaceInMinPerKm() =>
        Assert.That(Fields(311)[^1], Is.EqualTo(new FieldSpec("pace", Metric.Pace, "Pace", FieldKind.Pace, Unit: "min/km")));

    [TestCase(301, "rpm")]
    [TestCase(305, "strokes/min")]
    [TestCase(307, "strides/min")]
    [TestCase(310, "feet/min")]
    public void For_RateUnits(int id, string unit) =>
        Assert.That(Fields(id).Single(f => f.Key == "rate").Unit, Is.EqualTo(unit));

    [TestCase("Treadmill", "Duration, Distance (km), Incline (%), Calories, Rate (km/h)")]
    [TestCase("Exercise Bike", "Duration, Distance (km), Resistance, Calories, Rate (rpm)")]
    [TestCase("Recumbent Bike", "Duration, Distance (km), Resistance, Calories, Rate (rpm)")]
    [TestCase("Spin Bike", "Duration, Distance (km), Resistance, Calories, Rate (rpm)")]
    [TestCase("Air Bike", "Duration, Distance (km), Calories, Rate (rpm)")]
    [TestCase("Rowing Machine", "Duration, Distance (m), Damper, Calories, Rate (strokes/min)")]
    [TestCase("Ski Erg", "Duration, Distance (m), Damper, Calories, Rate (strokes/min)")]
    [TestCase("Cross Trainer", "Duration, Distance (km), Resistance, Incline (%), Calories, Rate (strides/min)")]
    [TestCase("Arc Trainer", "Duration, Distance (km), Resistance, Incline (%), Calories, Rate (strides/min)")]
    [TestCase("Stair Climber", "Duration, Floors, Speed level, Calories, Rate (steps/min)")]
    [TestCase("VersaClimber", "Duration, Distance (ft), Calories, Rate (feet/min)")]
    [TestCase("Outdoor Run", "Duration, Distance (km), Calories, Pace (min/km)")]
    [TestCase("Outdoor Walk", "Duration, Distance (km), Calories, Pace (min/km)")]
    [TestCase("Outdoor Cycle", "Duration, Distance (km), Calories, Rate (km/h)")]
    [TestCase("Swimming", "Duration, Distance (m), Calories")]
    [TestCase("Jump Rope", "Duration, Calories")]
    public void For_EachCardioMachine_LabelsAndUnits(string name, string labels) =>
        Assert.That(string.Join(", ", ExerciseFields.For(Catalog.All.Single(e => e.Name == name)).Select(f => f.Unit is null ? f.Label : $"{f.Label} ({f.Unit})")),
            Is.EqualTo(labels));

    [Test]
    public void For_DistanceInFeet() =>
        Assert.That(Fields(310).Single(f => f.Key == "distance").Unit, Is.EqualTo("ft"));

    [Test]
    public void For_EveryExerciseMaps() =>
        Assert.That(() => Catalog.All.Select(ExerciseFields.For).ToList(), Throws.Nothing);

    [TestCase(132, ExerciseKind.Strength)]
    [TestCase(300, ExerciseKind.Cardio)]
    public void New_HasOneEmptySetOrEntry(int id, ExerciseKind kind)
    {
        var definition = Find(id);

        var exercise = ExerciseFields.New(definition);

        Assert.Multiple(() =>
        {
            Assert.That(exercise.ExerciseId, Is.EqualTo(id));
            Assert.That(exercise.Name, Is.EqualTo(definition.Name));
            Assert.That(exercise.Kind, Is.EqualTo(kind));
            Assert.That(exercise.Fields, Is.EqualTo(ExerciseFields.For(definition)));
            Assert.That(exercise.Entries, Has.Count.EqualTo(1));
            Assert.That(exercise.Entries[0].Values, Is.Empty);
            Assert.That(exercise.Error, Is.Null);
        });
    }

    [Test]
    public void Saved_Strength_OneEntryPerSetInOrder()
    {
        var exercise = ExerciseFields.Saved(Find(6),
            new StrengthExercise(6, [new StrengthSet(8, 60m, null, null), new StrengthSet(6, 62.5m, null, null)]))!;

        Assert.Multiple(() =>
        {
            Assert.That(exercise.ExerciseId, Is.EqualTo(6));
            Assert.That(exercise.Name, Is.EqualTo("Barbell Back Squat"));
            Assert.That(exercise.Kind, Is.EqualTo(ExerciseKind.Strength));
            Assert.That(exercise.Fields, Is.EqualTo(ExerciseFields.For(Find(6))));
            Assert.That(Values(exercise), Is.EqualTo(new[]
            {
                new Dictionary<string, string?> { ["reps"] = "8", ["weight"] = "60" },
                new Dictionary<string, string?> { ["reps"] = "6", ["weight"] = "62.5" },
            }));
            Assert.That(exercise.Entries.All(e => e.Errors.Count == 0 && !e.CheckOnChange), Is.True);
            Assert.That(exercise.Error, Is.Null);
        });
    }

    [Test]
    public void Saved_EmptyValuesAreNull() =>
        Assert.That(Values(ExerciseFields.Saved(Find(6), new StrengthExercise(6, [new StrengthSet(8, null, null, null)]))),
            Is.EqualTo(new[] { new Dictionary<string, string?> { ["reps"] = "8", ["weight"] = null } }));

    [TestCase("60.50", "60.5")]
    [TestCase("60.00", "60")]
    [TestCase("0.05", "0.05")]
    [TestCase("0", "0")]
    [TestCase("1000", "1000")]
    public void Saved_DecimalsWithoutTrailingZeros(string saved, string shown) =>
        Assert.That(Values(ExerciseFields.Saved(Find(6), new StrengthExercise(6, [new StrengthSet(1, decimal.Parse(saved, CultureInfo.InvariantCulture), null, null)])))[0]["weight"],
            Is.EqualTo(shown));

    [Test]
    public void Saved_Carry_DurationAsHoursMinutesSeconds() =>
        Assert.That(Values(ExerciseFields.Saved(Find(187), new StrengthExercise(187,
            [new StrengthSet(null, 32m, TimeSpan.FromSeconds(45), 40m), new StrengthSet(null, null, new TimeSpan(23, 59, 59), 1.5m)]))),
            Is.EqualTo(new[]
            {
                new Dictionary<string, string?> { ["distance"] = "40", ["weight"] = "32", ["duration"] = "0:00:45" },
                new Dictionary<string, string?> { ["distance"] = "1.5", ["weight"] = null, ["duration"] = "23:59:59" },
            }));

    [Test]
    public void Saved_Cardio_TheEntryWithPaceAsMinutesSeconds()
    {
        var exercise = ExerciseFields.Saved(Find(311),
            new CardioExercise(311, new CardioEntry(new TimeSpan(0, 25, 30), 5m, null, null, null, null, null, new TimeSpan(0, 5, 6))))!;

        Assert.Multiple(() =>
        {
            Assert.That(exercise.Kind, Is.EqualTo(ExerciseKind.Cardio));
            Assert.That(Values(exercise), Is.EqualTo(new[]
            {
                new Dictionary<string, string?> { ["duration"] = "0:25:30", ["distance"] = "5", ["calories"] = null, ["pace"] = "5:06" },
            }));
        });
    }

    [Test]
    public void Saved_Cardio_WholeNumbersAndDecimals() =>
        Assert.That(Values(ExerciseFields.Saved(Find(309),
            new CardioExercise(309, new CardioEntry(new TimeSpan(1, 0, 0), null, 60, 8, null, 150, 72.25m, null))))[0],
            Is.EqualTo(new Dictionary<string, string?>
            {
                ["duration"] = "1:00:00", ["floors"] = "60", ["level"] = "8", ["calories"] = "150", ["rate"] = "72.25",
            }));

    [Test]
    public void Saved_Pace_SixtyMinutes() =>
        Assert.That(Values(ExerciseFields.Saved(Find(311),
            new CardioExercise(311, new CardioEntry(TimeSpan.FromMinutes(30), null, null, null, null, null, null, TimeSpan.FromMinutes(60)))))[0]["pace"],
            Is.EqualTo("60:00"));

    [TestCase(6)]
    [TestCase(187)]
    [TestCase(218)]
    [TestCase(258)]
    [TestCase(309)]
    [TestCase(311)]
    public void Saved_ReadBack_GivesTheSameRecords(int id)
    {
        var saved = SampleWorkout(id);
        var checker = new SetChecker(new SampleWorkoutService(new FakeTimeProvider(), Catalog), Catalog, NullLogger<SetChecker>.Instance);

        var read = checker.Read(ExerciseFields.Saved(Find(id), saved)!);

        Assert.That(read, Is.EqualTo(saved).Using<WorkoutExercise>(SameExercise));
    }

    [Test]
    public void Saved_StrengthDefinitionWithCardioRecord_IsNull() =>
        Assert.That(ExerciseFields.Saved(Find(6), new CardioExercise(6, new CardioEntry(TimeSpan.FromMinutes(1), null, null, null, null, null, null, null))),
            Is.Null);

    [Test]
    public void Saved_CardioDefinitionWithStrengthRecord_IsNull() =>
        Assert.That(ExerciseFields.Saved(Find(300), new StrengthExercise(300, [])), Is.Null);

    [Test]
    public void PickerItem_HasIdNameAndKind() =>
        Assert.That(ExerciseFields.PickerItem(Find(305)), Is.EqualTo(new PickerItem(305, "Rowing Machine", ExerciseKind.Cardio)));

    private static List<Dictionary<string, string?>> Values(ExerciseState? exercise) => exercise!.Entries.Select(e => e.Values).ToList();

    /// <summary>A saved exercise with every metric its definition shows filled, and a second set for strength.</summary>
    private static WorkoutExercise SampleWorkout(int id) => id switch
    {
        6 => new StrengthExercise(6, [new StrengthSet(8, 60m, null, null), new StrengthSet(6, 62.5m, null, null)]),
        187 => new StrengthExercise(187, [new StrengthSet(null, 32.25m, TimeSpan.FromSeconds(45), 40m), new StrengthSet(null, null, null, 0.5m)]),
        218 => new StrengthExercise(218, [new StrengthSet(10, 25m, null, null), new StrengthSet(8, 22.5m, null, null)]),
        258 => new StrengthExercise(258, [new StrengthSet(null, null, new TimeSpan(1, 2, 3), null), new StrengthSet(null, 10m, TimeSpan.FromSeconds(30), null)]),
        309 => new CardioExercise(309, new CardioEntry(new TimeSpan(0, 15, 0), null, 60, 8, null, 150, 72.25m, null)),
        311 => new CardioExercise(311, new CardioEntry(new TimeSpan(0, 25, 30), 5.05m, null, null, null, 300, null, new TimeSpan(0, 5, 6))),
        _ => throw new ArgumentOutOfRangeException(nameof(id), id, null),
    };

    /// <summary>Same type, exercise id and sets or entry; the records hold their sets in a list, which compares by reference.</summary>
    private static bool SameExercise(WorkoutExercise x, WorkoutExercise y) => (x, y) switch
    {
        (StrengthExercise a, StrengthExercise b) => a.ExerciseId == b.ExerciseId && a.Sets.SequenceEqual(b.Sets),
        (CardioExercise a, CardioExercise b) => a == b,
        _ => false,
    };

    private static ExerciseDefinition Find(int id) => Catalog.All.Single(e => e.Id == id);

    private static IReadOnlyList<FieldSpec> Fields(int id) => ExerciseFields.For(Find(id));
}
