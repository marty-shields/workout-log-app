using System.Text.Json.Nodes;
using WorkoutLog.Core.Import.Upgrades;
using static WorkoutLog.Core.Tests.Import.ImportJson;

namespace WorkoutLog.Core.Tests.Import.Upgrades;

public class NotesFormatUpgradeTests
{
    [Test]
    public void Version_Is3() => Assert.That(new NotesFormatUpgrade().Version, Is.EqualTo(3));

    [Test]
    public void Apply_Version2File_ChangesNothing()
    {
        var json = File(2, Workout(0, Bench, Run), Workout(1));
        var file = JsonNode.Parse(json)!.AsObject();

        new NotesFormatUpgrade().Apply(file);

        Assert.That(file.ToJsonString(), Is.EqualTo(JsonNode.Parse(json)!.ToJsonString()));
    }
}
