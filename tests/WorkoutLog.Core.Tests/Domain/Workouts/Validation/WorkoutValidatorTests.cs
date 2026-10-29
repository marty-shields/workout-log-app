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
    private const int Run = 6;
    private const int Treadmill = 7;

    /// <summary>Ids of more lifts, for the count limits.</summary>
    private static readonly int[] MoreLifts = [.. Enumerable.Range(100, 10)];

    private WorkoutValidator validator = null!;

    [SetUp]
    public void SetUp()
    {
        var catalog = new Mock<IExerciseCatalog>();
        catalog.Setup(c => c.All).Returns(
        [
            Strength(Press, ExerciseCategory.Lift, new MetricDefinition(Metric.Reps, Required: true), Weight()),
            Strength(Carry, ExerciseCategory.Carry, new MetricDefinition(Metric.Distance, Required: true), Weight(),
                new MetricDefinition(Metric.Duration, Required: true)),
            new ExerciseDefinition(Bike, "Bike", ExerciseType.Cardio, ExerciseCategory.Cardio, [], null, null, null,
            [
                new MetricDefinition(Metric.Duration, Required: true),
                new MetricDefinition(Metric.Distance, Required: true),
                new MetricDefinition(Metric.Level, Required: false, Label: "Resistance"),
                new MetricDefinition(Metric.Calories, Required: false),
                new MetricDefinition(Metric.Rate, Required: false),
            ], DistanceUnit.Km, RateUnit.Rpm, null, Loaded: false),
            Strength(Plank, ExerciseCategory.Hold, new MetricDefinition(Metric.Duration, Required: true), Weight()),
            new ExerciseDefinition(Climber, "Climber", ExerciseType.Cardio, ExerciseCategory.Cardio, [], null, null, null,
            [
                new MetricDefinition(Metric.Duration, Required: true),
                new MetricDefinition(Metric.Floors, Required: false),
                new MetricDefinition(Metric.Incline, Required: false),
            ], null, null, null, Loaded: false),
            new ExerciseDefinition(Run, "Run", ExerciseType.Cardio, ExerciseCategory.Cardio, [], null, null, null,
            [
                new MetricDefinition(Metric.Duration, Required: true),
                new MetricDefinition(Metric.Distance, Required: true),
                new MetricDefinition(Metric.Calories, Required: false),
            ], DistanceUnit.Km, null, PaceUnit.PerKm, Loaded: false),
            new ExerciseDefinition(Treadmill, "Treadmill", ExerciseType.Cardio, ExerciseCategory.Cardio, [], null, null, null,
            [
                new MetricDefinition(Metric.Duration, Required: true),
                new MetricDefinition(Metric.Distance, Required: true),
                new MetricDefinition(Metric.Incline, Required: false),
            ], DistanceUnit.Km, RateUnit.KmPerHour, null, Loaded: false),
            .. MoreLifts.Select(id => Strength(id, ExerciseCategory.Lift, new MetricDefinition(Metric.Reps, Required: true), Weight())),
        ]);
        validator = new WorkoutValidator(catalog.Object);
    }

    [Test]
    public void Validate_ValidWorkout_IsValid()
    {
        var errors = validator.Validate(
        [
            new StrengthExercise(Press, [new StrengthSet(10, 22.75m, null, null), new StrengthSet(8, 0m, null, null)]),
            new CardioExercise(Bike, new CardioEntry(TimeSpan.FromMinutes(20), 8.5m, null, 6, null, 210, 85m)),
            new StrengthExercise(Carry, [new StrengthSet(null, 0m, TimeSpan.FromSeconds(40), 20m)]),
        ]);

        Assert.That(errors.IsValid, Is.True);
        Assert.That(errors.Errors, Is.Empty);
        Assert.That(errors.Exercises, Has.Count.EqualTo(3));
    }

    [Test]
    public void Validate_NoExercises_ReturnsNoExercises()
    {
        var errors = validator.Validate([]);

        Assert.That(errors.Errors, Is.EqualTo(new[] { WorkoutError.NoExercises }));
        Assert.That(errors.IsValid, Is.False);
    }

    [Test]
    public void Validate_RepeatedExercise_ReturnsRepeatedExerciseAndChecksEachExercise()
    {
        var errors = validator.Validate([Lift(Press), new StrengthExercise(Press, [])]);

        Assert.That(errors.Errors, Is.EqualTo(new[] { WorkoutError.RepeatedExercise }));
        Assert.That(errors.Exercises.Select(e => e.Error), Is.EqualTo(new ExerciseError?[] { null, ExerciseError.NoSets }));
        Assert.That(errors.IsValid, Is.False);
    }

    [Test]
    public void Validate_TenExercises_IsValid() =>
        Assert.That(validator.Validate([Lift(Press), .. MoreLifts.Take(9).Select(Lift)]).IsValid, Is.True);

    [Test]
    public void Validate_ElevenExercises_ReturnsTooManyExercises()
    {
        var errors = validator.Validate([Lift(Press), .. MoreLifts.Select(Lift)]);

        Assert.That(errors.Errors, Is.EqualTo(new[] { WorkoutError.TooManyExercises }));
        Assert.That(errors.Exercises.All(e => e.IsValid), Is.True);
    }

    [Test]
    public void Validate_TooManyAndRepeated_ReturnsBoth() =>
        Assert.That(validator.Validate([Lift(Press), Lift(Press), .. MoreLifts.Take(9).Select(Lift)]).Errors,
            Is.EqualTo(new[] { WorkoutError.TooManyExercises, WorkoutError.RepeatedExercise }));

    [Test]
    public void Validate_StrengthWithNoSets_ReturnsNoSets()
    {
        var errors = validator.Validate(new StrengthExercise(Press, []));

        Assert.That(errors.Error, Is.EqualTo(ExerciseError.NoSets));
        Assert.That(errors.IsValid, Is.False);
    }

    [Test]
    public void Validate_TenSets_IsValid() =>
        Assert.That(validator.Validate(new StrengthExercise(Press, [.. Enumerable.Repeat(new StrengthSet(5, 60m, null, null), 10)])).IsValid, Is.True);

    [Test]
    public void Validate_ElevenSets_ReturnsTooManySetsAndChecksEachSet()
    {
        var errors = validator.Validate(new StrengthExercise(Press, [.. Enumerable.Repeat(new StrengthSet(5, 60m, null, null), 10), new StrengthSet(5, null, null, null)]));

        Assert.That(errors.Error, Is.EqualTo(ExerciseError.TooManySets));
        Assert.That(errors.Entries, Has.Count.EqualTo(11));
        Assert.That(errors.Entries[^1].Fields, Is.EqualTo(new Dictionary<Metric, FieldError> { [Metric.WeightKg] = FieldError.Missing }));
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
    public void Validate_EmptySets_ReturnMissingForEveryRequiredMetric()
    {
        Assert.Multiple(() =>
        {
            Assert.That(SetErrors(Press, new StrengthSet(null, null, null, null)),
                Is.EqualTo(new Dictionary<Metric, FieldError> { [Metric.Reps] = FieldError.Missing, [Metric.WeightKg] = FieldError.Missing }));
            Assert.That(SetErrors(Plank, new StrengthSet(null, null, null, null)),
                Is.EqualTo(new Dictionary<Metric, FieldError> { [Metric.WeightKg] = FieldError.Missing, [Metric.Duration] = FieldError.Missing }));
            Assert.That(SetErrors(Carry, new StrengthSet(null, null, null, null)), Is.EqualTo(new Dictionary<Metric, FieldError>
            {
                [Metric.WeightKg] = FieldError.Missing,
                [Metric.Duration] = FieldError.Missing,
                [Metric.Distance] = FieldError.Missing,
            }));
        });
    }

    [Test]
    public void Validate_CardioWithNoDurationOrDistance_ReturnsMissing()
    {
        Assert.Multiple(() =>
        {
            Assert.That(EntryErrors(new CardioEntry(null, null, null, null, null, null, null)),
                Is.EqualTo(new Dictionary<Metric, FieldError> { [Metric.Duration] = FieldError.Missing, [Metric.Distance] = FieldError.Missing }));
            Assert.That(EntryErrors(new CardioEntry(TimeSpan.FromMinutes(10), null, null, null, null, null, null), Climber), Is.Empty);
        });
    }

    [TestCase(0)]
    [TestCase(-1)]
    public void Validate_RepsNotMoreThanZero_ReturnsNotPositive(int reps)
    {
        Assert.That(SetErrors(Press, new StrengthSet(reps, 0m, null, null)),
            Is.EqualTo(new Dictionary<Metric, FieldError> { [Metric.Reps] = FieldError.NotPositive }));
    }

    [TestCase(0)]
    [TestCase(-1)]
    public void Validate_DurationNotMoreThanZero_ReturnsNotPositive(int seconds)
    {
        var duration = TimeSpan.FromSeconds(seconds);

        Assert.Multiple(() =>
        {
            Assert.That(SetErrors(Plank, new StrengthSet(null, 0m, duration, null)),
                Is.EqualTo(new Dictionary<Metric, FieldError> { [Metric.Duration] = FieldError.NotPositive }));
            Assert.That(SetErrors(Carry, new StrengthSet(null, 0m, duration, 20m)),
                Is.EqualTo(new Dictionary<Metric, FieldError> { [Metric.Duration] = FieldError.NotPositive }));
            Assert.That(EntryErrors(Entry() with { Duration = duration }),
                Is.EqualTo(new Dictionary<Metric, FieldError> { [Metric.Duration] = FieldError.NotPositive }));
        });
    }

    [TestCase(0)]
    [TestCase(-0.5)]
    public void Validate_DistanceNotMoreThanZero_ReturnsNotPositive(decimal distance)
    {
        Assert.Multiple(() =>
        {
            Assert.That(SetErrors(Carry, new StrengthSet(null, 32m, TimeSpan.FromSeconds(30), distance)),
                Is.EqualTo(new Dictionary<Metric, FieldError> { [Metric.Distance] = FieldError.NotPositive }));
            Assert.That(EntryErrors(Entry() with { Distance = distance }),
                Is.EqualTo(new Dictionary<Metric, FieldError> { [Metric.Distance] = FieldError.NotPositive }));
        });
    }

    [TestCase(0)]
    [TestCase(-1)]
    public void Validate_OptionalCardioMetricsNotMoreThanZero_ReturnNotPositive(int value)
    {
        Assert.Multiple(() =>
        {
            Assert.That(EntryErrors(Entry() with { Level = value, Calories = value, Rate = value }), Is.EqualTo(new Dictionary<Metric, FieldError>
            {
                [Metric.Level] = FieldError.NotPositive,
                [Metric.Calories] = FieldError.NotPositive,
                [Metric.Rate] = FieldError.NotPositive,
            }));
            Assert.That(EntryErrors(new CardioEntry(TimeSpan.FromMinutes(10), null, value, null, null, null, null), Climber),
                Is.EqualTo(new Dictionary<Metric, FieldError> { [Metric.Floors] = FieldError.NotPositive }));
        });
    }

    [Test]
    public void Validate_WeightAndInclineAtZero_AreValid()
    {
        Assert.Multiple(() =>
        {
            Assert.That(SetErrors(Press, new StrengthSet(10, 0m, null, null)), Is.Empty);
            Assert.That(EntryErrors(new CardioEntry(TimeSpan.FromMinutes(10), null, null, null, 0m, null, null), Climber), Is.Empty);
        });
    }

    [Test]
    public void Validate_NegativeWeightAndIncline_ReturnNegative()
    {
        Assert.Multiple(() =>
        {
            Assert.That(SetErrors(Press, new StrengthSet(10, -2.5m, null, null)),
                Is.EqualTo(new Dictionary<Metric, FieldError> { [Metric.WeightKg] = FieldError.Negative }));
            Assert.That(EntryErrors(new CardioEntry(TimeSpan.FromMinutes(10), null, null, null, -0.5m, null, null), Climber),
                Is.EqualTo(new Dictionary<Metric, FieldError> { [Metric.Incline] = FieldError.Negative }));
        });
    }

    [Test]
    public void Validate_ValuesAtTheLimit_AreValid()
    {
        Assert.Multiple(() =>
        {
            Assert.That(SetErrors(Press, new StrengthSet(1_000, 1_000m, null, null)), Is.Empty);
            Assert.That(SetErrors(Carry, new StrengthSet(null, 0m, MetricLimits.MaxDuration, 100_000m)), Is.Empty);
            Assert.That(EntryErrors(new CardioEntry(MetricLimits.MaxDuration, 1_000m, null, 100, null, 10_000, 300m)), Is.Empty);
            Assert.That(EntryErrors(new CardioEntry(TimeSpan.FromMinutes(1), null, 1_000, null, 100m, null, null), Climber), Is.Empty);
        });
    }

    [Test]
    public void Validate_ValuesAboveTheLimit_ReturnTooLarge()
    {
        Assert.Multiple(() =>
        {
            Assert.That(SetErrors(Press, new StrengthSet(1_001, 1_000.01m, null, null)),
                Is.EqualTo(new Dictionary<Metric, FieldError> { [Metric.Reps] = FieldError.TooLarge, [Metric.WeightKg] = FieldError.TooLarge }));
            Assert.That(SetErrors(Carry, new StrengthSet(null, 0m, MetricLimits.MaxDuration + TimeSpan.FromSeconds(1), 100_000.1m)),
                Is.EqualTo(new Dictionary<Metric, FieldError> { [Metric.Duration] = FieldError.TooLarge, [Metric.Distance] = FieldError.TooLarge }));
            Assert.That(EntryErrors(new CardioEntry(TimeSpan.FromDays(2), 1_000.01m, null, 101, null, 10_001, 300.5m)),
                Is.EqualTo(new Dictionary<Metric, FieldError>
                {
                    [Metric.Duration] = FieldError.TooLarge,
                    [Metric.Distance] = FieldError.TooLarge,
                    [Metric.Level] = FieldError.TooLarge,
                    [Metric.Calories] = FieldError.TooLarge,
                    [Metric.Rate] = FieldError.TooLarge,
                }));
            Assert.That(EntryErrors(new CardioEntry(TimeSpan.FromMinutes(1), null, 1_001, null, 100.5m, null, null), Climber),
                Is.EqualTo(new Dictionary<Metric, FieldError> { [Metric.Floors] = FieldError.TooLarge, [Metric.Incline] = FieldError.TooLarge }));
        });
    }

    [Test]
    public void Validate_TwoDecimalPlaces_AreValid()
    {
        Assert.Multiple(() =>
        {
            Assert.That(SetErrors(Press, new StrengthSet(5, 62.25m, null, null)), Is.Empty);
            Assert.That(SetErrors(Carry, new StrengthSet(null, 0.29m, TimeSpan.FromSeconds(30), 20.05m)), Is.Empty);
            Assert.That(EntryErrors(Entry() with { Distance = 5.21m, Rate = 85.55m }), Is.Empty);
            Assert.That(EntryErrors(new CardioEntry(TimeSpan.FromMinutes(1), null, null, null, 2.55m, null, null), Climber), Is.Empty);
        });
    }

    [Test]
    public void Validate_MoreThanTwoDecimalPlaces_ReturnTooManyDecimals()
    {
        Assert.Multiple(() =>
        {
            Assert.That(SetErrors(Press, new StrengthSet(5, 62.255m, null, null)),
                Is.EqualTo(new Dictionary<Metric, FieldError> { [Metric.WeightKg] = FieldError.TooManyDecimals }));
            Assert.That(SetErrors(Carry, new StrengthSet(null, 0m, TimeSpan.FromSeconds(30), 20.001m)),
                Is.EqualTo(new Dictionary<Metric, FieldError> { [Metric.Distance] = FieldError.TooManyDecimals }));
            Assert.That(EntryErrors(Entry() with { Distance = 5.211m, Rate = 85.555m }),
                Is.EqualTo(new Dictionary<Metric, FieldError> { [Metric.Distance] = FieldError.TooManyDecimals, [Metric.Rate] = FieldError.TooManyDecimals }));
            Assert.That(EntryErrors(new CardioEntry(TimeSpan.FromMinutes(1), null, null, null, 2.555m, null, null), Climber),
                Is.EqualTo(new Dictionary<Metric, FieldError> { [Metric.Incline] = FieldError.TooManyDecimals }));
        });
    }

    [Test]
    public void Validate_TooManyDecimals_ComesAfterNegativeAndBeforeTooLarge()
    {
        Assert.Multiple(() =>
        {
            Assert.That(SetErrors(Press, new StrengthSet(5, -0.001m, null, null)),
                Is.EqualTo(new Dictionary<Metric, FieldError> { [Metric.WeightKg] = FieldError.Negative }));
            Assert.That(SetErrors(Press, new StrengthSet(5, 1_000.001m, null, null)),
                Is.EqualTo(new Dictionary<Metric, FieldError> { [Metric.WeightKg] = FieldError.TooManyDecimals }));
        });
    }

    [Test]
    public void Validate_MetricsTheExerciseDoesNotShow_ReturnNotShown()
    {
        Assert.Multiple(() =>
        {
            Assert.That(SetErrors(Press, new StrengthSet(10, 0m, TimeSpan.FromSeconds(30), 5m)),
                Is.EqualTo(new Dictionary<Metric, FieldError> { [Metric.Duration] = FieldError.NotShown, [Metric.Distance] = FieldError.NotShown }));
            Assert.That(EntryErrors(Entry() with { Floors = 0, Incline = 1.5m }),
                Is.EqualTo(new Dictionary<Metric, FieldError> { [Metric.Floors] = FieldError.NotShown, [Metric.Incline] = FieldError.NotShown }));
        });
    }

    [Test]
    public void Validate_RateOnAnExerciseWithWorkedOutSpeed_ReturnsNotShown() =>
        Assert.That(EntryErrors(Entry() with { Rate = 10m }, Treadmill),
            Is.EqualTo(new Dictionary<Metric, FieldError> { [Metric.Rate] = FieldError.NotShown }));

    [Test]
    public void Validate_PartSeconds_ReturnNotWholeSeconds()
    {
        var part = TimeSpan.FromSeconds(30.5);

        Assert.Multiple(() =>
        {
            Assert.That(SetErrors(Plank, new StrengthSet(null, 0m, part, null)),
                Is.EqualTo(new Dictionary<Metric, FieldError> { [Metric.Duration] = FieldError.NotWholeSeconds }));
            Assert.That(SetErrors(Carry, new StrengthSet(null, 0m, part, 20m)),
                Is.EqualTo(new Dictionary<Metric, FieldError> { [Metric.Duration] = FieldError.NotWholeSeconds }));
            Assert.That(EntryErrors(Entry() with { Duration = part }, Run),
                Is.EqualTo(new Dictionary<Metric, FieldError> { [Metric.Duration] = FieldError.NotWholeSeconds }));
        });
    }

    [Test]
    public void Validate_PartSeconds_ComeAfterNotPositive() =>
        Assert.That(SetErrors(Plank, new StrengthSet(null, 0m, TimeSpan.FromSeconds(-0.5), null)),
            Is.EqualTo(new Dictionary<Metric, FieldError> { [Metric.Duration] = FieldError.NotPositive }));

    [Test]
    public void Validate_ErrorsInSomeSets_AreReportedPerSetInOrder()
    {
        var errors = validator.Validate(new StrengthExercise(Press,
        [
            new StrengthSet(10, 0m, null, null),
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
        var errors = validator.Validate([Lift(Press), new StrengthExercise(MoreLifts[0], [])]);

        Assert.That(errors.Errors, Is.Empty);
        Assert.That(errors.IsValid, Is.False);
        Assert.That(errors.Exercises.Select(e => e.Error), Is.EqualTo(new ExerciseError?[] { null, ExerciseError.NoSets }));
    }

    private static MetricDefinition Weight() => new(Metric.WeightKg, Required: true);

    private static ExerciseDefinition Strength(int id, ExerciseCategory category, params MetricDefinition[] metrics) =>
        new(id, $"Exercise {id}", ExerciseType.Strength, category, [], Muscle.Chest, Equipment.Barbell, Mechanic.Compound, metrics,
            metrics.Any(m => m.Metric == Metric.Distance) ? DistanceUnit.Metres : null, null, null, Loaded: false);

    private static StrengthExercise Lift(int id) => new(id, [new StrengthSet(5, 60m, null, null)]);

    private static CardioEntry Entry() => new(TimeSpan.FromMinutes(20), 5m, null, null, null, null, null);

    private IReadOnlyDictionary<Metric, FieldError> SetErrors(int exerciseId, StrengthSet set) =>
        validator.Validate(new StrengthExercise(exerciseId, [set])).Entries.Single().Fields;

    private IReadOnlyDictionary<Metric, FieldError> EntryErrors(CardioEntry entry, int exerciseId = Bike) =>
        validator.Validate(new CardioExercise(exerciseId, entry)).Entries.Single().Fields;
}
