using WorkoutLog.UI.Components.Lists;

namespace WorkoutLog.UI.Tests.Components.Lists;

public class RecordsCardTests : ComponentTestBase
{
    [Test]
    public void Title_ShowsTheRange()
    {
        var cut = Render([]);

        Assert.That(TestText.Normalise(cut.Find("h2").TextContent), Is.EqualTo("Records · last 3 months"));
    }

    [Test]
    public void Rows_ShowLabelValueAndDateInOrder()
    {
        var cut = Render([new("Heaviest weight", "70kg × 3", "20 Oct"), new("Best e1RM", "82.3kg", "13 Oct")]);

        Assert.That(Rows(cut), Is.EqualTo(new[] { ("Heaviest weight", "70kg × 3", "20 Oct"), ("Best e1RM", "82.3kg", "13 Oct") }));
    }

    [Test]
    public void NoValue_ShowsADashAndNoDate()
    {
        var cut = Render([new("Heaviest weight", null, "20 Oct")]);

        Assert.That(Rows(cut), Is.EqualTo(new[] { ("Heaviest weight", "—", "") }));
    }

    [Test]
    public void TestId_IsOnTheCard()
    {
        var cut = Render([]);

        Assert.That(cut.Find("section").GetAttribute("data-testid"), Is.EqualTo("records"));
    }

    private IRenderedComponent<RecordsCard> Render(IReadOnlyList<RecordRow> rows) =>
        Context.Render<RecordsCard>(p => p.Add(x => x.Range, "last 3 months").Add(x => x.Rows, rows).Add(x => x.TestId, "records"));

    private static IEnumerable<(string, string, string)> Rows(IRenderedComponent<RecordsCard> cut) =>
        cut.FindAll(".row").Select(r => (r.QuerySelector(".label")!.TextContent, r.QuerySelector(".value")!.TextContent, r.QuerySelector(".date")!.TextContent));
}
