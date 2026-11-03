using SQLite;
using WorkoutLog.Core.Data;

namespace WorkoutLog.Core.Tests.Data;

public class WorkoutWriterFactoryTests
{
    [Test]
    public void Create_ReturnsAWorkoutWriter()
    {
        using var db = new SQLiteConnection(":memory:");

        Assert.That(new WorkoutWriterFactory().Create(db), Is.InstanceOf<WorkoutWriter>());
    }
}
