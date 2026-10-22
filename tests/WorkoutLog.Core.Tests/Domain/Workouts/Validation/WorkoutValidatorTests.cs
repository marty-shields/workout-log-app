using Moq;
using WorkoutLog.Core.Domain.Exercises;
using WorkoutLog.Core.Domain.Workouts;
using WorkoutLog.Core.Domain.Workouts.Validation;

namespace WorkoutLog.Core.Tests.Domain.Workouts.Validation;

public class WorkoutValidatorTests
{
    private const int Press = 1;
    private const int Carry = 2;
    private const int Bike = 3;
    private const int Plank = 4;
    private const int Climber = 5;

    private WorkoutValidator validator = null!;

    [SetUp]
    public void SetUp()
    {
        var catalog = new Mock<IExerciseCatalog>();
        catalog.Setup(c => c.All).Returns(
        [
            Strength(Press, new MetricDefinition(Metric.Reps, Required: true), new MetricDefinition(Metric.WeightKg, Required: false)),
            Strength(Carry, new MetricDefinition(Metric.Distance, Required: true), new MetricDefinition(Metric.WeightKg, Required: false),
                new MetricDefinition(Metric.Duration, Required: false)),
            new ExerciseDefinition(Bike, "Bike", ExerciseType.Cardio, [], null, null, null,
            [
                new MetricDefinition(Metric.Duration, Required: true),
                new MetricDefinition(Metric.Distance, Required: false),
                new MetricDefinition(Metric.Level, Required: false, Label: "Resistance"),
                new MetricDefinition(Metric.Calories, Required: false),
                new MetricDefinition(Metric.Rate, Required: false),
            ], DistanceUnit.Km, RateUnit.Rpm),
            Strength(Plank, new MetricDefinition(Metric.Duration, Required: true), new MetricDefinition(Metric.WeightKg, Required: false)),
            new ExerciseDefinition(Climber, "Climber", ExerciseType.Cardio, [], null, null, null,
            [
                new MetricDefinition(Metric.Duration, Required: true),
                new MetricDefinition(Metric.Floors, Required: false),
                new MetricDefinition(Metric.Incline, Required: false),
            ], null, null),
        ]);
        validator = new WorkoutValidator(catalog.Object);
    }

    [Test]
    public void Validate_ValidWorkout_IsValid()
    {
        var errors = validator.Validate(
        [
            new StrengthExercise(Press, [new StrengthSet(10, 22.75m, null, null), new StrengthSet(8, null, null, null)]),
            new CardioExercise(Bike, new CardioEntry(TimeSpan.FromMinutes(20), 8.5m, null, 6, null, 210, 85m)),
            new StrengthExercise(Press, [new StrengthSet(5, 0m, null, null)]),
        ]);

        Assert.That(errors.IsValid, Is.True);
        Assert.That(errors.Error, Is.Null);
        Assert.That(errors.Exercises, Has.Count.EqualTo(3));
    }

    [Test]
    public void Validate_NoExercises_ReturnsNoExercises()
    {
        var errors = validator.Validate([]);

        Assert.That(errors.Error, Is.EqualTo(WorkoutError.NoExercises));
        Assert.That(errors.IsValid, Is.False);
    }

    [Test]
    public void Validate_StrengthWithNoSets_ReturnsNoSets()
    {
        var errors = validator.Validate(new StrengthExercise(Press, []));

        Assert.That(errors.Error, Is.EqualTo(ExerciseError.NoSets));
        Assert.That(errors.IsValid, Is.False);
    }

    [Test]
    public void Validate_UnknownExercise_ReturnsUnknownExercise()
    {
        var errors = validator.Validate(new StrengthExercise(99, [new StrengthSet(null, null, null, null)]));

        Assert.That(errors, Is.EqualTo(new ExerciseErrors(ExerciseError.UnknownExercise, [])).UsingPropertiesComparer());
    }

    [Test]
    public void Validate_StrengthForACardioExercise_ReturnsWrongType()
    {
        var errors = validator.Validate(new StrengthExercise(Bike, [new StrengthSet(10, null, null, null)]));

        Assert.That(errors, Is.EqualTo(new ExerciseErrors(ExerciseError.WrongType, [])).UsingPropertiesComparer());
    }

    [Test]
    public void Validate_CardioForAStrengthExercise_ReturnsWrongType()
    {
        var errors = validator.Validate(new CardioExercise(Press, Entry()));

        Assert.That(errors, Is.EqualTo(new ExerciseErrors(ExerciseError.WrongType, [])).UsingPropertiesComparer());
    }

    [Test]
    public void Validate_EmptySet_ReturnsMissingForRequiredMetricsOnly()
    {
        Assert.That(SetErrors(Press, new StrengthSet(null, null, null, null)),
            Is.EqualTo(new Dictionary<Metric, FieldError> { [Metric.Reps] = FieldError.Missing }));
    }

    [Test]
    public void Validate_CardioWithNoDuration_ReturnsMissing()
    {
        Assert.That(EntryErrors(Entry() with { Duration = null }),
            Is.EqualTo(new Dictionary<Metric, FieldError> { [Metric.Duration] = FieldError.Missing }));
    }

    [TestCase(0)]
    [TestCase(-1)]
    public void Validate_RepsNotMoreThanZero_ReturnsNotPositive(int reps)
    {
        Assert.That(SetErrors(Press, new StrengthSet(reps, null, null, null)),
            Is.EqualTo(new Dictionary<Metric, FieldError> { [Metric.Reps] = FieldError.NotPositive }));
    }

    [TestCase(0)]
    [TestCase(-1)]
    public void Validate_RequiredDurationNotMoreThanZero_ReturnsNotPositive(int seconds)
    {
        var duration = TimeSpan.FromSeconds(seconds);

        Assert.Multiple(() =>
        {
            Assert.That(SetErrors(Plank, new StrengthSet(null, null, duration, null)),
                Is.EqualTo(new Dictionary<Metric, FieldError> { [Metric.Duration] = FieldError.NotPositive }));
            Assert.That(EntryErrors(Entry() with { Duration = duration }),
                Is.EqualTo(new Dictionary<Metric, FieldError> { [Metric.Duration] = FieldError.NotPositive }));
        });
    }

    [Test]
    public void Validate_OptionalDuration_ZeroIsValidAndNegativeIsNot()
    {
        Assert.Multiple(() =>
        {
            Assert.That(SetErrors(Carry, new StrengthSet(null, null, TimeSpan.Zero, 20m)), Is.Empty);
            Assert.That(SetErrors(Carry, new StrengthSet(null, null, TimeSpan.FromSeconds(-1), 20m)),
                Is.EqualTo(new Dictionary<Metric, FieldError> { [Metric.Duration] = FieldError.Negative }));
        });
    }

    [Test]
    public void Validate_NegativeFloorsAndIncline_ReturnNegative()
    {
        var errors = validator.Validate(new CardioExercise(Climber, new CardioEntry(TimeSpan.FromMinutes(10), null, -1, null, -0.5m, null, null)));

        Assert.That(errors.Entries.Single().Fields, Is.EqualTo(new Dictionary<Metric, FieldError>
        {
            [Metric.Floors] = FieldError.Negative,
            [Metric.Incline] = FieldError.Negative,
        }));
    }

    [Test]
    public void Validate_ValuesAtTheLimit_AreValid()
    {
        Assert.Multiple(() =>
        {
            Assert.That(SetErrors(Press, new StrengthSet(1_000, 1_000m, null, null)), Is.Empty);
            Assert.That(SetErrors(Carry, new StrengthSet(null, null, MetricLimits.MaxDuration, 100_000m)), Is.Empty);
            Assert.That(EntryErrors(new CardioEntry(MetricLimits.MaxDuration, 1_000m, null, 100, null, 10_000, 300m)), Is.Empty);
            Assert.That(validator.Validate(new CardioExercise(Climber, new CardioEntry(TimeSpan.FromMinutes(1), null, 1_000, null, 100m, null, null))).IsValid,
                Is.True);
        });
    }

    [Test]
    public void Validate_ValuesAboveTheLimit_ReturnTooLarge()
    {
        Assert.Multiple(() =>
        {
            Assert.That(SetErrors(Press, new StrengthSet(1_001, 1_000.01m, null, null)),
                Is.EqualTo(new Dictionary<Metric, FieldError> { [Metric.Reps] = FieldError.TooLarge, [Metric.WeightKg] = FieldError.TooLarge }));
            Assert.That(SetErrors(Carry, new StrengthSet(null, null, MetricLimits.MaxDuration + TimeSpan.FromTicks(1), 100_000.1m)),
                Is.EqualTo(new Dictionary<Metric, FieldError> { [Metric.Duration] = FieldError.TooLarge, [Metric.Distance] = FieldError.TooLarge }));
            Assert.That(EntryErrors(new CardioEntry(TimeSpan.FromDays(2), decimal.MaxValue, null, 101, null, 10_001, 300.5m)),
                Is.EqualTo(new Dictionary<Metric, FieldError>
                {
                    [Metric.Duration] = FieldError.TooLarge,
                    [Metric.Distance] = FieldError.TooLarge,
                    [Metric.Level] = FieldError.TooLarge,
                    [Metric.Calories] = FieldError.TooLarge,
                    [Metric.Rate] = FieldError.TooLarge,
                }));
            Assert.That(validator.Validate(new CardioExercise(Climber, new CardioEntry(TimeSpan.FromMinutes(1), null, 1_001, null, 100.5m, null, null)))
                    .Entries.Single().Fields,
                Is.EqualTo(new Dictionary<Metric, FieldError> { [Metric.Floors] = FieldError.TooLarge, [Metric.Incline] = FieldError.TooLarge }));
        });
    }

    [TestCase(0)]
    [TestCase(-0.5)]
    public void Validate_RequiredDistanceNotMoreThanZero_ReturnsNotPositive(decimal distance)
    {
        Assert.That(SetErrors(Carry, new StrengthSet(null, 32m, null, distance)),
            Is.EqualTo(new Dictionary<Metric, FieldError> { [Metric.Distance] = FieldError.NotPositive }));
    }

    [Test]
    public void Validate_OptionalMetricsAtZero_AreValid()
    {
        Assert.Multiple(() =>
        {
            Assert.That(SetErrors(Press, new StrengthSet(10, 0m, null, null)), Is.Empty);
            Assert.That(EntryErrors(new CardioEntry(TimeSpan.FromMinutes(1), 0m, null, 0, null, 0, 0m)), Is.Empty);
        });
    }

    [Test]
    public void Validate_NegativeOptionalMetrics_ReturnNegative()
    {
        Assert.Multiple(() =>
        {
            Assert.That(SetErrors(Press, new StrengthSet(10, -2.5m, null, null)),
                Is.EqualTo(new Dictionary<Metric, FieldError> { [Metric.WeightKg] = FieldError.Negative }));
            Assert.That(EntryErrors(new CardioEntry(TimeSpan.FromMinutes(1), -1m, null, -1, null, -1, -1m)),
                Is.EqualTo(new Dictionary<Metric, FieldError>
                {
                    [Metric.Distance] = FieldError.Negative,
                    [Metric.Level] = FieldError.Negative,
                    [Metric.Calories] = FieldError.Negative,
                    [Metric.Rate] = FieldError.Negative,
                }));
        });
    }

    [Test]
    public void Validate_MetricsTheExerciseDoesNotShow_ReturnNotShown()
    {
        Assert.Multiple(() =>
        {
            Assert.That(SetErrors(Press, new StrengthSet(10, null, TimeSpan.FromSeconds(30), 5m)),
                Is.EqualTo(new Dictionary<Metric, FieldError> { [Metric.Duration] = FieldError.NotShown, [Metric.Distance] = FieldError.NotShown }));
            Assert.That(EntryErrors(Entry() with { Floors = 0, Incline = 1.5m }),
                Is.EqualTo(new Dictionary<Metric, FieldError> { [Metric.Floors] = FieldError.NotShown, [Metric.Incline] = FieldError.NotShown }));
        });
    }

    [Test]
    public void Validate_ErrorsInSomeSets_AreReportedPerSetInOrder()
    {
        var errors = validator.Validate(new StrengthExercise(Press,
        [
            new StrengthSet(10, null, null, null),
            new StrengthSet(null, -1m, null, null),
        ]));

        Assert.That(errors.Error, Is.Null);
        Assert.That(errors.IsValid, Is.False);
        Assert.That(errors.Entries.Select(e => e.Fields), Is.EqualTo(new[]
        {
            new Dictionary<Metric, FieldError>(),
            new Dictionary<Metric, FieldError> { [Metric.Reps] = FieldError.Missing, [Metric.WeightKg] = FieldError.Negative },
        }));
    }

    [Test]
    public void Validate_WorkoutWithAFaultyExercise_IsInvalidWithNoWorkoutError()
    {
        var errors = validator.Validate([new StrengthExercise(Press, [new StrengthSet(10, null, null, null)]), new StrengthExercise(Press, [])]);

        Assert.That(errors.Error, Is.Null);
        Assert.That(errors.IsValid, Is.False);
        Assert.That(errors.Exercises.Select(e => e.Error), Is.EqualTo(new ExerciseError?[] { null, ExerciseError.NoSets }));
    }

    private static ExerciseDefinition Strength(int id, params MetricDefinition[] metrics) =>
        new(id, $"Exercise {id}", ExerciseType.Strength, [], Muscle.Chest, Equipment.Barbell, Mechanic.Compound, metrics,
            metrics.Any(m => m.Metric == Metric.Distance) ? DistanceUnit.Metres : null, null);

    private static CardioEntry Entry() => new(TimeSpan.FromMinutes(20), null, null, null, null, null, null);

    private IReadOnlyDictionary<Metric, FieldError> SetErrors(int exerciseId, StrengthSet set) =>
        validator.Validate(new StrengthExercise(exerciseId, [set])).Entries.Single().Fields;

    private IReadOnlyDictionary<Metric, FieldError> EntryErrors(CardioEntry entry) =>
        validator.Validate(new CardioExercise(Bike, entry)).Entries.Single().Fields;
}
