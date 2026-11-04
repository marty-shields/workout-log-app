using System.Globalization;
using AngleSharp.Dom;
using WorkoutLog.UI.Components.Charts;

namespace WorkoutLog.UI.Tests.Components.Charts;

public class ChartTests : ComponentTestBase
{
    private static readonly Func<decimal, string> Kg = value => $"{value.ToString("0.##", CultureInfo.InvariantCulture)}kg";

    [Test]
    public void Points_DrawADotEach()
    {
        var cut = Render([new("1 Sep", 10), new("8 Sep", 12), new("15 Sep", 11)]);

        Assert.That(cut.FindAll(".point"), Has.Count.EqualTo(3));
    }

    [Test]
    public void PointWithNoValue_IsSkippedAndTheLineJoinsAcross()
    {
        var cut = Render([new("1 Sep", 10), new("8 Sep", null), new("15 Sep", 12)]);

        var circles = cut.FindAll(".point");
        Assert.Multiple(() =>
        {
            Assert.That(circles, Has.Count.EqualTo(2));
            Assert.That(cut.Find("path").GetAttribute("d"), Is.EqualTo($"M{circles[0].GetAttribute("cx")} {circles[0].GetAttribute("cy")} L{circles[1].GetAttribute("cx")} {circles[1].GetAttribute("cy")}"));
            Assert.That(cut.FindAll("[data-testid=chart-values] li").Select(li => li.TextContent), Is.EqualTo(new[] { "1 Sep: 10kg", "15 Sep: 12kg" }));
        });
    }

    [Test]
    public void OnePoint_DrawsASingleDotWithOneDateAndNoLine()
    {
        var cut = Render([new("5 Oct", 14, "×10")]);

        Assert.Multiple(() =>
        {
            Assert.That(cut.FindAll(".point"), Has.Count.EqualTo(1));
            Assert.That(cut.FindAll("path"), Is.Empty);
            Assert.That(Labels(cut), Is.EqualTo(new[] { "×10" }));
            Assert.That(cut.FindAll(".date").Select(d => d.TextContent), Is.EqualTo(new[] { "5 Oct" }));
            Assert.That(Ticks(cut), Is.EqualTo(new[] { "13", "14", "15" }));
        });
    }

    [Test]
    public void NoValues_DrawsNothing()
    {
        var cut = Render([new("5 Oct", null)]);

        Assert.That(cut.FindAll("circle, path, text"), Is.Empty);
    }

    [Test]
    public void Best_IsTheHighestValue()
    {
        var cut = Render([new("1 Sep", 10), new("8 Sep", 14), new("15 Sep", 12)]);

        Assert.That(BestIndex(cut), Is.EqualTo(1));
    }

    [Test]
    public void Flipped_BestIsTheLowestValueAndAtTheTop()
    {
        var cut = Render([new("1 Sep", 340), new("8 Sep", 330), new("15 Sep", 352)], flipped: true);

        var ys = cut.FindAll(".point").Select(c => Number(c, "cy")).ToList();
        Assert.Multiple(() =>
        {
            Assert.That(BestIndex(cut), Is.EqualTo(1));
            Assert.That(ys[1], Is.LessThan(ys[0]));
            Assert.That(ys[0], Is.LessThan(ys[2]));
        });
    }

    [Test]
    public void NotFlipped_HigherValuesAreHigherUp()
    {
        var cut = Render([new("1 Sep", 10), new("8 Sep", 14)]);

        var ys = cut.FindAll(".point").Select(c => Number(c, "cy")).ToList();
        Assert.That(ys[1], Is.LessThan(ys[0]));
    }

    [TestCase(new[] { 5 }, false, 0, 0)]
    [TestCase(new[] { 5, 7, 6 }, false, 1, 2)]
    [TestCase(new[] { 5, 6, 7 }, false, 2, 2)]
    [TestCase(new[] { 5, 7, 7, 3 }, false, 2, 3)]
    [TestCase(new[] { 7, 5, 7 }, false, 2, 2)]
    [TestCase(new[] { 340, 330, 352 }, true, 1, 2)]
    [TestCase(new[] { 5, 3, 3, 7 }, true, 2, 3)]
    [TestCase(new[] { 4, 4, 4 }, false, 2, 2)]
    [TestCase(new[] { 4, 4, 4 }, true, 2, 2)]
    public void LabelledPoints_AreTheBestLatestOfATieAndTheLatest(int[] values, bool flipped, int best, int latest)
    {
        Assert.That(Chart.LabelledPoints(values.Select(v => (decimal)v).ToList(), flipped), Is.EqualTo((best, latest)));
    }

    [TestCase(59, 72, false, 55, 75, 5)]
    [TestCase(72.8, 82.3, false, 70, 85, 5)]
    [TestCase(15, 25, false, 15, 25, 5)]
    [TestCase(4.1, 5.2, false, 4, 5.5, 0.5)]
    [TestCase(14, 14, false, 13, 15, 1)]
    [TestCase(0, 0, false, 0, 1, 1)]
    [TestCase(0, 0, true, 0, 1, 1)]
    [TestCase(79.17, 79.2, false, 79.1, 79.2, 0.1)]
    [TestCase(330, 352, true, 330, 360, 10)]
    [TestCase(60, 105, true, 60, 105, 15)]
    [TestCase(1500, 1800, true, 1440, 1800, 120)]
    public void Scale_RoundStepsSpanningTheValues(double min, double max, bool time, double low, double high, double step)
    {
        Assert.That(Chart.Scale((decimal)min, (decimal)max, time), Is.EqualTo(((decimal)low, (decimal)high, (decimal)step)));
    }

    [Test]
    public void Ticks_AreBareNumbers()
    {
        var cut = Render([new("1 Sep", 59), new("8 Sep", 72)]);

        Assert.That(Ticks(cut), Is.EqualTo(new[] { "55", "60", "65", "70", "75" }));
    }

    [Test]
    public void TimeScale_TicksReadMinutesAndSeconds()
    {
        var cut = Render([new("1 Sep", 60), new("8 Sep", 105)], time: true);

        Assert.That(Ticks(cut), Is.EqualTo(new[] { "1:00", "1:15", "1:30", "1:45" }));
    }

    [Test]
    public void TimeScale_FromAnHour_TicksReadHours()
    {
        var cut = Render([new("1 Sep", 3000), new("8 Sep", 4200)], time: true);

        Assert.That(Ticks(cut), Is.EqualTo(new[] { "50:00", "55:00", "1:00:00", "1:05:00", "1:10:00" }));
    }

    [Test]
    public void OnlyTheBestAndLatest_ShowTheirLabelElseTheirFormattedValue()
    {
        var cut = Render([new("1 Sep", 60, "×6"), new("8 Sep", 65, "×5"), new("15 Sep", 60, "×8"), new("22 Sep", 62.5m)]);

        Assert.That(Labels(cut), Is.EqualTo(new[] { "×5", "62.5kg" }));
    }

    [Test]
    public void LatestIsTheBest_HasOneLabel()
    {
        var cut = Render([new("1 Sep", 10, "×6"), new("8 Sep", 12, "×5"), new("15 Sep", 12, "×3")]);

        Assert.Multiple(() =>
        {
            Assert.That(Labels(cut), Is.EqualTo(new[] { "×3" }));
            Assert.That(BestIndex(cut), Is.EqualTo(2));
        });
    }

    [Test]
    public void Latest_IsTheFilledLargerPoint_AndBestHasARing()
    {
        var cut = Render([new("1 Sep", 10), new("8 Sep", 20), new("15 Sep", 15)]);

        var points = cut.FindAll(".point");
        Assert.Multiple(() =>
        {
            Assert.That(points.Select(p => p.ClassList.Contains("latest")), Is.EqualTo(new[] { false, false, true }));
            Assert.That(Number(points[2], "r"), Is.GreaterThan(Number(points[0], "r")));
            Assert.That(BestIndex(cut), Is.EqualTo(1));
        });
    }

    [Test]
    public void FirstPointLabel_StartsAtThePointClearOfTheTicks()
    {
        var cut = Render([new("1 Sep", 2000), new("8 Sep", 1000), new("15 Sep", 1500)]);

        var label = cut.FindAll(".label")[0];
        Assert.Multiple(() =>
        {
            Assert.That(label.GetAttribute("text-anchor"), Is.EqualTo("start"));
            Assert.That(Number(label, "x"), Is.GreaterThan(cut.FindAll(".tick").Max(t => Number(t, "x"))));
        });
    }

    [Test]
    public void LastPointLabel_EndsAtThePoint()
    {
        var cut = Render([new("1 Sep", 20000), new("8 Sep", 10000)]);

        Assert.That(cut.FindAll(".label").Select(l => l.GetAttribute("text-anchor")), Is.EqualTo(new[] { "start", "end" }));
    }

    [Test]
    public void BestNextToTheLatestAndAtTheSameHeight_LatestLabelGoesUnderItsPoint()
    {
        var values = new[] { 10, 20, 30, 40, 50, 60, 70, 80, 100, 99 };
        var cut = Render(values.Select((v, i) => new ChartPoint($"{i + 1} Sep", v)).ToList());

        var labels = cut.FindAll(".label");
        var points = cut.FindAll(".point");
        Assert.Multiple(() =>
        {
            Assert.That(Number(labels[0], "y"), Is.LessThan(Number(points[8], "cy")));
            Assert.That(Number(labels[1], "y"), Is.GreaterThan(Number(points[9], "cy")));
        });
    }

    [Test]
    public void LatestLabelWouldCrossTheBestRing_GoesUnderItsPoint()
    {
        var values = new[] { 72.8m, 76, 76, 79.2m, 79.2m, 79.2m, 82.3m, 79.2m };
        var cut = Render(values.Select((v, i) => new ChartPoint($"{i + 1} Sep", v)).ToList());

        var labels = cut.FindAll(".label");
        Assert.Multiple(() =>
        {
            Assert.That(labels.Select(l => l.TextContent), Is.EqualTo(new[] { "82.3kg", "79.2kg" }));
            Assert.That(Number(labels[1], "y"), Is.GreaterThan(Number(cut.FindAll(".point")[7], "cy")));
        });
    }

    [Test]
    public void BestBesideTheLatestAtTheBottom_StacksBothLabelsOverTheBestClearOfTheDates()
    {
        decimal[] values = [.. Enumerable.Repeat(80m, 14), 80.02m, 80m];
        var cut = Render(values.Select((v, i) => new ChartPoint($"{i + 1} Sep", v)).ToList());

        var labels = cut.FindAll(".label");
        var latest = cut.FindAll(".point")[^1];
        var ring = cut.Find("circle.best");
        Assert.Multiple(() =>
        {
            Assert.That(labels.Select(l => l.TextContent), Is.EqualTo(new[] { "80.02kg", "80kg" }));
            Assert.That(Number(labels[1], "y"), Is.LessThan(Number(ring, "cy") - Number(ring, "r")), "latest label over the best ring");
            Assert.That(Number(labels[0], "y"), Is.LessThanOrEqualTo(Number(labels[1], "y") - 14), "best label a row over the latest");
            Assert.That(Number(labels[1], "y"), Is.LessThan(Number(latest, "cy")), "latest label over its point");
            Assert.That(Number(labels[0], "y") - 14, Is.GreaterThan(0), "inside the top");
        });
    }

    [Test]
    public void BestFarAboveTheLatest_BothLabelsAreOverTheirPoints()
    {
        var cut = Render([new("1 Sep", 10), new("8 Sep", 100), new("15 Sep", 20)]);

        var labels = cut.FindAll(".label");
        var points = cut.FindAll(".point");
        Assert.Multiple(() =>
        {
            Assert.That(Number(labels[0], "y"), Is.LessThan(Number(points[1], "cy")));
            Assert.That(Number(labels[1], "y"), Is.LessThan(Number(points[2], "cy")));
        });
    }

    [Test]
    public void Dates_ShowTheFirstAndLastOnly()
    {
        var cut = Render([new("18 Aug", 60), new("25 Aug", 62), new("20 Oct", 70)]);

        Assert.That(cut.FindAll(".date").Select(d => d.TextContent), Is.EqualTo(new[] { "18 Aug", "20 Oct" }));
    }

    [Test]
    public void Chart_HasAnAccessibleNameAndATextAlternative()
    {
        var cut = Render([new("18 Aug", 60, "×6"), new("20 Oct", 70, "×3")]);

        Assert.Multiple(() =>
        {
            Assert.That(cut.Find("svg").GetAttribute("role"), Is.EqualTo("img"));
            Assert.That(cut.Find("svg").GetAttribute("aria-label"), Is.EqualTo("Top set, last 3 months"));
            Assert.That(cut.Find("[data-testid=chart-values]").ClassList, Does.Contain("visually-hidden"));
            Assert.That(cut.FindAll("[data-testid=chart-values] li").Select(li => li.TextContent), Is.EqualTo(new[] { "18 Aug: 60kg ×6", "20 Oct: 70kg ×3" }));
        });
    }

    private IRenderedComponent<Chart> Render(IReadOnlyList<ChartPoint> points, bool flipped = false, bool time = false) =>
        Context.Render<Chart>(p => p
            .Add(x => x.Points, points)
            .Add(x => x.Format, Kg)
            .Add(x => x.Flipped, flipped)
            .Add(x => x.TimeScale, time)
            .Add(x => x.Name, "Top set, last 3 months")
            .Add(x => x.TestId, "chart"));

    private static IEnumerable<string> Labels(IRenderedComponent<Chart> cut) => cut.FindAll(".label").Select(l => l.TextContent);

    /// <summary>Index of the point the best ring is drawn round.</summary>
    private static int BestIndex(IRenderedComponent<Chart> cut)
    {
        var ring = cut.Find("circle.best");
        return cut.FindAll(".point").Select((c, i) => (c, i)).Single(x => x.c.GetAttribute("cx") == ring.GetAttribute("cx")).i;
    }

    /// <summary>Tick texts from the bottom of the chart up.</summary>
    private static IEnumerable<string> Ticks(IRenderedComponent<Chart> cut) =>
        cut.FindAll(".tick").OrderByDescending(t => Number(t, "y")).Select(t => t.TextContent);

    private static double Number(IElement element, string attribute) => double.Parse(element.GetAttribute(attribute)!, CultureInfo.InvariantCulture);
}
