using System.Globalization;
using Microsoft.AspNetCore.Components;

namespace WorkoutLog.UI.Components.Charts;

/// <summary>Works out where the ticks, line, points and dates go.</summary>
public partial class Chart
{
    // Drawing area in viewBox units.
    private const double Width = 400;
    private const double Height = 200;
    private const double Left = 48;
    private const double FirstX = 64;
    private const double Right = 376;
    private const double Top = 28;
    private const double Bottom = 168;
    private const double TickX = 40;
    private const double DateY = 194;
    private const double LabelGap = 14;
    private const double LabelBelow = 24;
    private const double Radius = 4.5;
    private const double LatestRadius = 6;
    private const double RingRadius = 9;

    // Label size estimate at --font-size-sm (14 units), bold: measured in Chrome, "79.2kg" is 54 units wide and "5.2km" 49.
    private const double CharWidth = 10;
    private const double LineHeight = 14;

    // Smallest tick step: the 0.1 a weight shows; time steps start at 1 second.
    private const decimal MinStep = 0.1m;

    private static readonly decimal[] NiceSteps = [1m, 2m, 5m, 10m];

    // Tick steps for time, in seconds.
    private static readonly decimal[] TimeSteps = [1, 2, 5, 10, 15, 30, 60, 120, 300, 600, 900, 1800, 3600];

    private IReadOnlyList<(string Y, string Text)> ticks = [];
    private List<Plotted> plotted = [];
    private IReadOnlyList<Label> labels = [];
    private IReadOnlyList<(string X, string Text, string Anchor)> dates = [];
    private string? line;

    /// <summary>Points in date order, oldest first, evenly spaced.</summary>
    [Parameter, EditorRequired]
    public IReadOnlyList<ChartPoint> Points { get; set; } = [];

    /// <summary>Lower is better: the lowest value is at the top and is the best.</summary>
    [Parameter]
    public bool Flipped { get; set; }

    /// <summary>Value text for point labels and the text alternative, e.g. 82.3kg.</summary>
    [Parameter, EditorRequired]
    public Func<decimal, string> Format { get; set; } = null!;

    /// <summary>Values are seconds: ticks step on whole times and read m:ss (h:mm:ss from an hour).</summary>
    [Parameter]
    public bool TimeScale { get; set; }

    /// <summary>Accessible name, e.g. Top set, last 3 months.</summary>
    [Parameter, EditorRequired]
    public string Name { get; set; } = "";

    /// <summary>Value of the data-testid attribute; the text alternative has <c>{TestId}-values</c>.</summary>
    [Parameter]
    public string? TestId { get; set; }

    private IEnumerable<string> Values =>
        Points.Where(p => p.Value is not null).Select(p => $"{p.Date}: {Format(p.Value!.Value)}{(p.Label is { } label ? $" {label}" : "")}");

    /// <summary>Lays out the chart for the current points.</summary>
    protected override void OnParametersSet()
    {
        var valued = Points.Select((point, index) => (Point: point, Index: index)).Where(p => p.Point.Value is not null).ToList();
        if (valued.Count == 0)
        {
            (ticks, plotted, labels, dates, line) = ([], [], [], [], null);
            return;
        }

        var (low, high, step) = Scale(valued.Min(p => p.Point.Value!.Value), valued.Max(p => p.Point.Value!.Value), TimeScale);
        var (best, latest) = LabelledPoints(valued.Select(p => p.Point.Value!.Value).ToList(), Flipped);

        ticks = Enumerable.Range(0, (int)((high - low) / step) + 1)
            .Select(i => low + (i * step))
            .Select(value => (Number(Y(value, low, high)), TickText(value)))
            .ToList();

        plotted = valued
            .Select((p, i) => new Plotted(X(p.Index, Points.Count), Y(p.Point.Value!.Value, low, high), i == best, i == latest))
            .ToList();

        labels = Labels(plotted[best], plotted[latest], LabelText(valued[best].Point), LabelText(valued[latest].Point));

        line = plotted.Count > 1 ? "M" + string.Join(" L", plotted.Select(p => $"{Number(p.X)} {Number(p.Y)}")) : null;

        dates = Points.Count == 1
            ? [(Number(X(0, 1)), Points[0].Date, "middle")]
            : [(Number(FirstX), Points[0].Date, "middle"), (Number(Right), Points[^1].Date, "end")];
    }

    /// <summary>
    /// Lowest tick, highest tick and the step between them: a whole step of 1, 2 or 5 × 10ⁿ (or a whole time) spanning the values.
    /// The step is at least <see cref="MinStep"/> (1 when every value is the same), and ticks don't go below 0 when no value does.
    /// </summary>
    internal static (decimal Low, decimal High, decimal Step) Scale(decimal min, decimal max, bool time)
    {
        var range = max > min ? max - min : Math.Max(Math.Abs(max), 1m) / 5;
        var step = Math.Max(time ? TimeStep(range / 4) : NiceStep(range / 4), max > min ? MinStep : 1m);
        var low = Math.Floor(min / step) * step;
        var high = Math.Ceiling(max / step) * step;
        if (low == high)
        {
            (low, high) = (low - step, high + step);
        }

        return (min >= 0 ? Math.Max(low, 0) : low, high, step);
    }

    /// <summary>
    /// Indexes of the points that get a label: the best (the highest value, or the lowest when flipped; the latest of a tie) and the latest.
    /// They are the same index when the latest point is the best.
    /// </summary>
    internal static (int Best, int Latest) LabelledPoints(IReadOnlyList<decimal> values, bool flipped)
    {
        var best = 0;
        for (var i = 1; i < values.Count; i++)
        {
            if (flipped ? values[i] <= values[best] : values[i] >= values[best])
            {
                best = i;
            }
        }

        return (best, values.Count - 1);
    }

    /// <summary>
    /// Labels over the best and latest points, kept inside the plot: a label near an edge is anchored to start or end at its point so it
    /// clears the y-axis ticks and the right edge. When the latest label would overlap the best label or ring, it goes under its point;
    /// the best is the highest point, so the latest is below it.
    /// </summary>
    private static IReadOnlyList<Label> Labels(Plotted best, Plotted latest, string bestText, string latestText)
    {
        var latestLabel = Place(latest, latestText, latest.Y - LabelGap);
        if (best == latest)
        {
            return [latestLabel];
        }

        var bestLabel = Place(best, bestText, best.Y - LabelGap);
        var ring = new Box(best.X - RingRadius, best.X + RingRadius, best.Y - RingRadius, best.Y + RingRadius);
        if (latestLabel.Box().Overlaps(bestLabel.Box()) || latestLabel.Box().Overlaps(ring))
        {
            latestLabel = Place(latest, latestText, latest.Y + LabelBelow);
        }

        return [bestLabel, latestLabel];
    }

    private static Label Place(Plotted point, string text, double y)
    {
        var half = text.Length * CharWidth / 2;
        return point.X - half < Left ? new(point.X - LatestRadius, y, "start", text)
            : point.X + half > Width ? new(point.X + LatestRadius, y, "end", text)
            : new(point.X, y, "middle", text);
    }

    private string LabelText(ChartPoint point) => point.Label ?? Format(point.Value!.Value);

    private static decimal NiceStep(decimal rough)
    {
        var power = (decimal)Math.Pow(10, Math.Floor(Math.Log10((double)rough)));
        return NiceSteps.First(m => m * power >= rough) * power;
    }

    private static decimal TimeStep(decimal rough) =>
        rough <= TimeSteps[^1] ? TimeSteps.First(s => s >= rough) : Math.Ceiling(rough / 3600) * 3600;

    private string TickText(decimal value) =>
        TimeScale ? DisplayFormat.Clock(TimeSpan.FromSeconds((double)value)) : value.ToString("0.##", CultureInfo.InvariantCulture);

    private static double X(int index, int count) => count == 1 ? (FirstX + Right) / 2 : FirstX + (index * (Right - FirstX) / (count - 1));

    private double Y(decimal value, decimal low, decimal high)
    {
        var fraction = (double)((value - low) / (high - low));
        return Flipped ? Top + (fraction * (Bottom - Top)) : Bottom - (fraction * (Bottom - Top));
    }

    private static string Number(double value) => value.ToString("0.#", CultureInfo.InvariantCulture);

    /// <summary>A point as drawn: position, and whether it is the best and the latest.</summary>
    private sealed record Plotted(double X, double Y, bool Best, bool Latest);

    /// <summary>A point's label as drawn: <see cref="Anchor"/> is the SVG text-anchor of <see cref="X"/>, and <see cref="Y"/> its baseline.</summary>
    private sealed record Label(double X, double Y, string Anchor, string Text)
    {
        /// <summary>Estimated extent, from <see cref="CharWidth"/> and <see cref="LineHeight"/>.</summary>
        public Box Box()
        {
            var width = Text.Length * CharWidth;
            var (start, end) = Anchor switch
            {
                "start" => (X, X + width),
                "end" => (X - width, X),
                _ => (X - (width / 2), X + (width / 2)),
            };
            return new(start, end, Y - LineHeight, Y);
        }
    }

    private sealed record Box(double Left, double Right, double Top, double Bottom)
    {
        public bool Overlaps(Box other) => Left < other.Right && other.Left < Right && Top < other.Bottom && other.Top < Bottom;
    }
}
