using WorkoutLog.Core.Services;
using WorkoutLog.UI.Import;

namespace WorkoutLog.UI.Tests.Import;

public class ImportSessionTests
{
    [Test]
    public void TakeDone_ReturnsTheResultOnce()
    {
        var done = new ImportResult(ImportStatus.Imported, 1, 0, 0);
        var session = new ImportSession { Done = done };

        Assert.Multiple(() =>
        {
            Assert.That(session.TakeDone(), Is.SameAs(done));
            Assert.That(session.TakeDone(), Is.Null);
            Assert.That(session.Done, Is.Null);
        });
    }
}
