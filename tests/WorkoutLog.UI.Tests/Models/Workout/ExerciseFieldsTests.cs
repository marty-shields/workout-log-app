using WorkoutLog.Core.Domain.Exercises;
using WorkoutLog.UI.Models.Fields;
using WorkoutLog.UI.Models.Workout;

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
    public void PickerItem_HasIdNameAndKind() =>
        Assert.That(ExerciseFields.PickerItem(Find(305)), Is.EqualTo(new PickerItem(305, "Rowing Machine", ExerciseKind.Cardio)));

    private static ExerciseDefinition Find(int id) => Catalog.All.Single(e => e.Id == id);

    private static IReadOnlyList<FieldSpec> Fields(int id) => ExerciseFields.For(Find(id));
}
