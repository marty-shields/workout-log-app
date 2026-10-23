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
    public void MissingResource_ThrowsOnFirstUseNotOnCreate()
    {
        var missing = new ExerciseCatalog("WorkoutLog.Core.missing.json");

        Assert.That(() => missing.All, Throws.InvalidOperationException);
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
                new MetricDefinition(Metric.WeightKg, Required: false, Label: "Assist"),
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
            Assert.That(exercise.RateUnit, Is.Null);
            Assert.That(exercise.PaceUnit, Is.EqualTo(PaceUnit.PerKm));
            Assert.That(Find("Rowing Machine").RateUnit, Is.EqualTo(RateUnit.StrokesPerMin));
        });
    }

    [TestCase("Outdoor Run")]
    [TestCase("Outdoor Walk")]
    public void All_PaceExercises_ShowPaceWhereRateWas(string name) =>
        Assert.That(Find(name).Metrics.Select(m => m.Metric), Is.EqualTo(new[] { Metric.Duration, Metric.Distance, Metric.Calories, Metric.Pace }));

    [Test]
    public void All_ReadsMetricLabels() =>
        Assert.That(Find("Stair Climber").Metrics.Single(m => m.Metric == Metric.Level).Label, Is.EqualTo("Speed level"));

    [TestCase(null)]
    [TestCase("")]
    [TestCase("   ")]
    public void Search_NoText_ReturnsStrengthThenCardioAToZ(string? query)
    {
        var results = catalog.Search(query);

        var expected = catalog.All
            .OrderBy(e => e.Type)
            .ThenBy(e => e.Name, StringComparer.OrdinalIgnoreCase);
        Assert.That(results, Is.EqualTo(expected));
    }

    [TestCase("triceps pushdown", "Cable Tricep Pushdown")]
    [TestCase("biceps", "Barbell Curl")]
    [TestCase("flies", "Cable Fly")]
    [TestCase("flyes", "Cable Fly")]
    [TestCase("flye", "Cable Fly")]
    [TestCase("pullup", "Close-Grip Pull-up")]
    [TestCase("pushup", "Decline Push-up")]
    [TestCase("chinup", "Chin-up")]
    [TestCase("situp", "Decline Sit-up")]
    [TestCase("stepup", "Barbell Step-up")]
    [TestCase("skullcrusher", "Barbell Skull Crusher")]
    [TestCase("farmers walk", "Farmer's Walk")]
    [TestCase("farmer’s walk", "Farmer's Walk")]
    [TestCase("chin-up", "Chin-up")]
    [TestCase("CHIN UP", "Chin-up")]
    public void Search_NormalisesWords(string query, string expected) =>
        Assert.That(Names(query), Does.Contain(expected));

    [Test]
    public void Search_NordicsMatchesNordic()
    {
        var nordic = catalog.All.First(e => e.Name.Contains("Nordic", StringComparison.Ordinal));

        Assert.That(catalog.Search("nordics"), Does.Contain(nordic));
    }

    [Test]
    public void Search_WordsInAnyOrder() =>
        Assert.That(Names("press chest machine"), Does.Contain("Machine Chest Press"));

    [Test]
    public void Search_MatchesTheStartOfWordsOnly()
    {
        Assert.Multiple(() =>
        {
            Assert.That(Names("pre"), Does.Contain("Machine Chest Press"));
            Assert.That(Names("ress"), Does.Not.Contain("Machine Chest Press"));
        });
    }

    [Test]
    public void Search_EveryWordMustMatch() =>
        Assert.That(Names("chest zumba"), Is.Empty);

    [Test]
    public void Search_NoMatch_ReturnsEmpty() =>
        Assert.That(catalog.Search("zumba"), Is.Empty);

    [Test]
    public void Search_MatchesNicknames() =>
        Assert.That(Names("gravitron"), Is.EqualTo(new[] { "Machine Assisted Pull-up" }));

    [Test]
    public void Search_NameStartingWithTheQueryComesFirst() =>
        Assert.That(Names("dumbbell curl")[0], Is.EqualTo("Dumbbell Curl"));

    [Test]
    public void Search_OrdersNameStartThenNameThenNicknameAToZ() =>
        Assert.That(Names("pull up"), Is.EqualTo(new[]
        {
            "Pull-up",
            "Close-Grip Pull-up",
            "Machine Assisted Pull-up",
            "Neutral-Grip Pull-up",
            "Wide-Grip Pull-up",
            "Inverted Row",
        }));

    [Test]
    public void Search_EveryWordOfTheNameStartMayBePartial() =>
        Assert.That(Names("dumb curl"), Is.EqualTo(new[]
        {
            "Dumbbell Curl",
            "Dumbbell Concentration Curl",
            "Dumbbell Cross-Body Hammer Curl",
            "Dumbbell Hammer Curl",
            "Dumbbell Hammer Preacher Curl",
            "Dumbbell Incline Curl",
            "Dumbbell Incline Hammer Curl",
            "Dumbbell Leg Curl",
            "Dumbbell Preacher Curl",
            "Dumbbell Prone Incline Curl",
            "Dumbbell Reverse Curl",
            "Dumbbell Reverse Wrist Curl",
            "Dumbbell Wrist Curl",
            "Dumbbell Zottman Curl",
        }));

    [Test]
    public void Search_NicknameOnlyMatches_AToZ() =>
        Assert.That(Names("pec"), Is.EqualTo(new[] { "Machine Chest Fly", "Machine Rear Delt Fly" }));

    [TestCase("pul")]
    [TestCase("pull")]
    [TestCase("pullu")]
    [TestCase("pullup")]
    [TestCase("pullups")]
    [TestCase("pull ups")]
    [TestCase("pull-ups")]
    public void Search_TypingAJoinedWord_KeepsMatching(string query) =>
        Assert.That(Names(query), Does.Contain("Pull-up"));

    [TestCase("pushu", "Decline Push-up")]
    [TestCase("pushups", "Decline Push-up")]
    [TestCase("chinups", "Chin-up")]
    [TestCase("situps", "Decline Sit-up")]
    [TestCase("stepups", "Barbell Step-up")]
    [TestCase("skullc", "Barbell Skull Crusher")]
    [TestCase("skullcr", "Barbell Skull Crusher")]
    public void Search_PartAndPluralJoinedWords(string query, string expected) =>
        Assert.That(Names(query), Does.Contain(expected));

    [Test]
    public void Search_PartJoinedWord_DoesNotMatchOtherWordsStartingTheSame() =>
        Assert.That(Names("pushd"), Does.Not.Contain("Decline Push-up"));

    [TestCase("bench.")]
    [TestCase("bench, ")]
    [TestCase("(bench)")]
    [TestCase("bench/press")]
    [TestCase("bench\npress")]
    public void Search_PunctuationSeparatesWords(string query) =>
        Assert.That(Names(query), Does.Contain("Barbell Bench Press"));

    [Test]
    public void Search_OnlyPunctuation_ReturnsEverything() =>
        Assert.That(catalog.Search(".,;!?"), Has.Count.EqualTo(catalog.All.Count));

    [Test]
    public void Search_QueryAtTheLimit_IsReadInFull()
    {
        var query = "dumbbell " + new string(' ', ExerciseCatalog.MaxQueryLength - "dumbbell ".Length - "zottman".Length) + "zottman";
        Assert.That(query, Has.Length.EqualTo(ExerciseCatalog.MaxQueryLength));

        Assert.That(Names(query), Is.EqualTo(new[] { "Dumbbell Zottman Curl" }));
    }

    [Test]
    public void Search_TextPastTheLimit_IsIgnored()
    {
        var query = "dumbbell" + new string(' ', ExerciseCatalog.MaxQueryLength - "dumbbell".Length) + "zumba";

        Assert.That(Names(query), Is.EqualTo(Names("dumbbell")));
    }

    [Test]
    public void Search_OneOverTheLimit_DropsTheLastCharacter()
    {
        var query = new string(' ', ExerciseCatalog.MaxQueryLength - 3) + "pecx";
        Assert.That(query, Has.Length.EqualTo(ExerciseCatalog.MaxQueryLength + 1));

        Assert.That(Names(query), Is.EqualTo(Names("pec")));
    }

    [Test]
    public void Search_VeryLongPaste_ReturnsQuickly()
    {
        var query = string.Concat(Enumerable.Repeat("b ", 500_000));
        var timer = System.Diagnostics.Stopwatch.StartNew();

        catalog.Search(query);

        Assert.That(timer.Elapsed, Is.LessThan(TimeSpan.FromSeconds(1)));
    }

    private ExerciseDefinition Find(string name) => catalog.All.Single(e => e.Name == name);

    private List<string> Names(string query) => catalog.Search(query).Select(e => e.Name).ToList();
}
