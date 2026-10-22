using System.Text.Json;
using WorkoutLog.Core.Domain.Exercises;

namespace WorkoutLog.Core.Tests.Domain.Exercises;

public class ExerciseCatalogTests
{
    private readonly ExerciseCatalog catalog = new();

    [Test]
    public void Options_RejectUnknownFields()
    {
        Assert.Multiple(() =>
        {
            Assert.That(JsonSerializer.Deserialize<MetricDefinition>("""{"metric":"Level","required":false,"label":"Damper"}""", ExerciseCatalog.Options),
                Is.EqualTo(new MetricDefinition(Metric.Level, Required: false, Label: "Damper")));
            Assert.That(() => JsonSerializer.Deserialize<MetricDefinition>("""{"metric":"Level","required":false,"lable":"Damper"}""", ExerciseCatalog.Options),
                Throws.TypeOf<JsonException>());
        });
    }

    [Test]
    public void All_ReadsStrengthFields()
    {
        var exercise = Find("Machine Assisted Pull-up");

        Assert.Multiple(() =>
        {
            Assert.That(exercise.Type, Is.EqualTo(ExerciseType.Strength));
            Assert.That(exercise.Aliases, Does.Contain("Gravitron"));
            Assert.That(exercise.PrimaryMuscle, Is.EqualTo(Muscle.Lats));
            Assert.That(exercise.Equipment, Is.EqualTo(Equipment.Machine));
            Assert.That(exercise.Mechanic, Is.EqualTo(Mechanic.Compound));
            Assert.That(exercise.Metrics, Is.EqualTo(new[]
            {
                new MetricDefinition(Metric.Reps, Required: true),
                new MetricDefinition(Metric.WeightKg, Required: false, Label: "Assist (kg)"),
            }));
        });
    }

    [Test]
    public void All_ReadsCardioUnits()
    {
        var exercise = Find("Outdoor Run");

        Assert.Multiple(() =>
        {
            Assert.That(exercise.Type, Is.EqualTo(ExerciseType.Cardio));
            Assert.That(exercise.DistanceUnit, Is.EqualTo(DistanceUnit.Km));
            Assert.That(exercise.RateUnit, Is.EqualTo(RateUnit.MinPerKm));
        });
    }

    [Test]
    public void All_ReadsMetricLabels() =>
        Assert.That(Find("Stair Climber").Metrics.Single(m => m.Metric == Metric.Level).Label, Is.EqualTo("Speed level"));

    private ExerciseDefinition Find(string name) => catalog.All.Single(e => e.Name == name);
}
