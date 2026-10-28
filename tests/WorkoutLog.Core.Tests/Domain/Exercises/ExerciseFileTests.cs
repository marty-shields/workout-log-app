using WorkoutLog.Core.Domain.Exercises;

namespace WorkoutLog.Core.Tests.Domain.Exercises;

/// <summary>Checks the embedded exercises.json against docs/02 and docs/03.</summary>
public class ExerciseFileTests
{
    private static readonly IReadOnlyList<ExerciseDefinition> Exercises = new ExerciseCatalog().All;

    private static readonly Metric[] StrengthMetrics = [Metric.Reps, Metric.WeightKg, Metric.Duration, Metric.Distance];

    private static readonly Metric[] CardioMetrics =
        [Metric.Duration, Metric.Distance, Metric.Floors, Metric.Level, Metric.Incline, Metric.Calories, Metric.Rate, Metric.Pace];

    private static IEnumerable<TestCaseData> Each() => Cases(Exercises);

    private static IEnumerable<TestCaseData> Cardio() => Cases(Exercises.Where(e => e.Type == ExerciseType.Cardio));

    private static IEnumerable<TestCaseData> Cases(IEnumerable<ExerciseDefinition> exercises) =>
        exercises.Select(e => new TestCaseData(e).SetArgDisplayNames($"{e.Id} {e.Name}"));

    [Test]
    public void File_HasTheDocumentedCounts()
    {
        Assert.Multiple(() =>
        {
            Assert.That(Exercises, Has.Count.EqualTo(315));
            Assert.That(Exercises.Count(e => e.Type == ExerciseType.Strength), Is.EqualTo(299));
            Assert.That(Exercises.Count(e => e.Type == ExerciseType.Cardio), Is.EqualTo(16));
            Assert.That(Exercises.GroupBy(e => e.Category).ToDictionary(g => g.Key, g => g.Count()), Is.EqualTo(new Dictionary<ExerciseCategory, int>
            {
                [ExerciseCategory.Lift] = 228,
                [ExerciseCategory.Bodyweight] = 56,
                [ExerciseCategory.Assisted] = 3,
                [ExerciseCategory.Hold] = 6,
                [ExerciseCategory.Carry] = 6,
                [ExerciseCategory.Cardio] = 16,
            }));
        });
    }

    [Test]
    public void Ids_RunFromOneWithoutGaps() =>
        Assert.That(Exercises.Select(e => e.Id), Is.EqualTo(Enumerable.Range(1, Exercises.Count)));

    [Test]
    public void Names_AreUnique() =>
        Assert.That(Exercises.Select(e => e.Name), Is.Unique.IgnoreCase);

    [Test]
    public void Aliases_AreUniqueAndNotAName() =>
        Assert.That(Exercises.SelectMany(e => e.Aliases).Concat(Exercises.Select(e => e.Name)), Is.Unique.IgnoreCase);

    [TestCaseSource(nameof(Each))]
    public void EnumValues_AreDefined(ExerciseDefinition exercise)
    {
        Assert.Multiple(() =>
        {
            Assert.That(Enum.IsDefined(exercise.Type), Is.True);
            Assert.That(Enum.IsDefined(exercise.Category), Is.True);
            Assert.That(exercise.PrimaryMuscle is not { } muscle || Enum.IsDefined(muscle), Is.True);
            Assert.That(exercise.Equipment is not { } equipment || Enum.IsDefined(equipment), Is.True);
            Assert.That(exercise.Mechanic is not { } mechanic || Enum.IsDefined(mechanic), Is.True);
            Assert.That(exercise.Metrics.All(m => Enum.IsDefined(m.Metric)), Is.True);
            Assert.That(exercise.DistanceUnit is not { } distance || Enum.IsDefined(distance), Is.True);
            Assert.That(exercise.RateUnit is not { } rate || Enum.IsDefined(rate), Is.True);
            Assert.That(exercise.PaceUnit is not { } pace || Enum.IsDefined(pace), Is.True);
        });
    }

    [TestCaseSource(nameof(Each))]
    public void Details_MatchTheType(ExerciseDefinition exercise)
    {
        var strength = exercise.Type == ExerciseType.Strength;
        Assert.Multiple(() =>
        {
            Assert.That(exercise.PrimaryMuscle.HasValue, Is.EqualTo(strength));
            Assert.That(exercise.Equipment.HasValue, Is.EqualTo(strength));
            Assert.That(exercise.Mechanic.HasValue, Is.EqualTo(strength));
        });
    }

    [TestCaseSource(nameof(Each))]
    public void Category_FitsTheTypeAndMetrics(ExerciseDefinition exercise)
    {
        bool Requires(Metric metric) => exercise.Metrics.Any(m => m.Metric == metric && m.Required);
        var assistLabel = exercise.Metrics.Any(m => m is { Metric: Metric.WeightKg, Label: "Assist" });
        Assert.Multiple(() =>
        {
            Assert.That(exercise.Category == ExerciseCategory.Cardio, Is.EqualTo(exercise.Type == ExerciseType.Cardio), "cardio");
            Assert.That(exercise.Category == ExerciseCategory.Assisted, Is.EqualTo(assistLabel), "assisted");
            Assert.That(exercise.Category == ExerciseCategory.Carry, Is.EqualTo(exercise.Type == ExerciseType.Strength && Requires(Metric.Distance)), "carry");
            Assert.That(exercise.Category == ExerciseCategory.Hold, Is.EqualTo(exercise.Type == ExerciseType.Strength && Requires(Metric.Duration)), "hold");
            if (exercise.Category is ExerciseCategory.Lift or ExerciseCategory.Bodyweight)
            {
                Assert.That(Requires(Metric.Reps), Is.True, "reps");
            }
        });
    }

    [TestCaseSource(nameof(Each))]
    public void Metrics_FitTheTypeAndAreNotRepeated(ExerciseDefinition exercise)
    {
        var allowed = exercise.Type == ExerciseType.Strength ? StrengthMetrics : CardioMetrics;
        var metrics = exercise.Metrics.Select(m => m.Metric).ToList();
        Assert.Multiple(() =>
        {
            Assert.That(metrics, Is.Not.Empty);
            Assert.That(metrics, Is.Unique);
            Assert.That(metrics, Is.SubsetOf(allowed));
        });
    }

    [TestCaseSource(nameof(Each))]
    public void Aliases_AreNotBlank(ExerciseDefinition exercise) =>
        Assert.That(exercise.Aliases, Has.None.Matches<string?>(string.IsNullOrWhiteSpace));

    [TestCaseSource(nameof(Each))]
    public void Level_HasALabel(ExerciseDefinition exercise) =>
        Assert.That(exercise.Metrics.Where(m => m.Metric == Metric.Level).Select(m => m.Label), Has.None.Matches<string?>(string.IsNullOrWhiteSpace));

    [TestCaseSource(nameof(Cardio))]
    public void Cardio_RequiresDuration(ExerciseDefinition exercise) =>
        Assert.That(exercise.Metrics, Has.One.Matches<MetricDefinition>(m => m is { Metric: Metric.Duration, Required: true }));

    [TestCaseSource(nameof(Each))]
    public void Units_MatchTheMetrics(ExerciseDefinition exercise)
    {
        Assert.Multiple(() =>
        {
            Assert.That(exercise.DistanceUnit.HasValue, Is.EqualTo(exercise.Metrics.Any(m => m.Metric == Metric.Distance)));
            Assert.That(exercise.RateUnit.HasValue, Is.EqualTo(exercise.Metrics.Any(m => m.Metric == Metric.Rate)));
            Assert.That(exercise.PaceUnit.HasValue, Is.EqualTo(exercise.Metrics.Any(m => m.Metric == Metric.Pace)));
        });
    }
}
