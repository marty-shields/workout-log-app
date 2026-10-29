using System.Text.Json.Nodes;
using WorkoutLog.Core.Domain.Exercises;
using WorkoutLog.Core.Import.Upgrades;

namespace WorkoutLog.Core.Tests.Import.Upgrades;

public class ValidationRulesFormatUpgradeTests
{
    private readonly ValidationRulesFormatUpgrade upgrade = new(new ExerciseCatalog());

    [Test]
    public void Version_Is2() =>
        Assert.That(upgrade.Version, Is.EqualTo(2));

    [Test]
    public void Apply_StrengthSetWithoutWeight_Gets0()
    {
        Assert.That(Upgraded("""{"exercise":{"id":264},"sets":[{"reps":20},{"reps":8,"weightKg":10},{"reps":5,"weightKg":null}]}"""),
            Is.EqualTo("""{"exercise":{"id":264},"sets":[{"reps":20,"weightKg":0},{"reps":8,"weightKg":10},{"reps":5,"weightKg":0}]}"""));
    }

    [Test]
    public void Apply_CardioZeros_AreRemovedAndIncline0IsKept()
    {
        Assert.That(Upgraded("""{"exercise":{"id":309},"entry":{"duration":"00:20:00","floors":0,"level":0,"incline":0,"calories":0,"rate":0}}"""),
            Is.EqualTo("""{"exercise":{"id":309},"entry":{"duration":"00:20:00","incline":0}}"""));
    }

    [Test]
    public void Apply_ValuesOver0_AreKept()
    {
        const string exercise = """{"exercise":{"id":305},"entry":{"duration":"00:20:00","distance":4000,"level":5,"calories":200,"rate":24}}""";

        Assert.That(Upgraded(exercise), Is.EqualTo(exercise));
    }

    [TestCase(311)]
    [TestCase(312)]
    public void Apply_PaceOnAnExerciseWithWorkedOutPace_IsRemoved(int id)
    {
        Assert.That(Upgraded($$$"""{"exercise":{"id":{{{id}}}},"entry":{"duration":"00:30:00","distance":5.2,"pace":"00:05:46"}}"""),
            Is.EqualTo($$$"""{"exercise":{"id":{{{id}}}},"entry":{"duration":"00:30:00","distance":5.2}}"""));
    }

    [TestCase(300)]
    [TestCase(313)]
    public void Apply_RateOnAnExerciseWithWorkedOutSpeed_IsRemoved(int id)
    {
        Assert.That(Upgraded($$$"""{"exercise":{"id":{{{id}}}},"entry":{"duration":"00:30:00","distance":5,"rate":10}}"""),
            Is.EqualTo($$$"""{"exercise":{"id":{{{id}}}},"entry":{"duration":"00:30:00","distance":5}}"""));
    }

    [Test]
    public void Apply_PaceOnAnotherExercise_IsKept()
    {
        const string exercise = """{"exercise":{"id":305},"entry":{"duration":"00:30:00","distance":5000,"pace":"00:02:00"}}""";

        Assert.That(Upgraded(exercise), Is.EqualTo(exercise));
    }

    [TestCase("""{"exercise":{"id":999},"entry":{"duration":"00:30:00","pace":"00:05:00","rate":10}}""", TestName = "Unknown exercise")]
    [TestCase("""{"exercise":{"id":"311"},"entry":{"duration":"00:30:00","pace":"00:05:00"}}""", TestName = "Id as text")]
    [TestCase("""{"entry":{"duration":"00:30:00","pace":"00:05:00"}}""", TestName = "No exercise info")]
    [TestCase("""{"exercise":{"id":311},"entry":[],"sets":{}}""", TestName = "Entry and sets of the wrong kind")]
    public void Apply_ExerciseNotInTheExportShape_IsLeftForTheReader(string exercise) =>
        Assert.That(Upgraded(exercise), Is.EqualTo(exercise));

    [TestCase("""{"formatVersion":1}""")]
    [TestCase("""{"formatVersion":1,"workouts":{}}""")]
    [TestCase("""{"formatVersion":1,"workouts":[null,1,{"exercises":{}},{"exercises":[null,2]}]}""")]
    public void Apply_FileNotInTheExportShape_IsUnchanged(string json)
    {
        var file = JsonNode.Parse(json)!.AsObject();

        upgrade.Apply(file);

        Assert.That(file.ToJsonString(), Is.EqualTo(json));
    }

    /// <summary>Applies the step to a file holding one workout with <paramref name="exercise"/>; returns the exercise as JSON.</summary>
    private string Upgraded(string exercise)
    {
        var file = JsonNode.Parse($$"""{"formatVersion":1,"workouts":[{"exercises":[{{exercise}}]}]}""")!.AsObject();

        upgrade.Apply(file);

        return file["workouts"]![0]!["exercises"]![0]!.ToJsonString();
    }
}
