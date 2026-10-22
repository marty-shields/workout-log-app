using WorkoutLog.UI.Components.Workout;
using WorkoutLog.UI.Models.Fields;
using WorkoutLog.UI.Models.Workout;

namespace WorkoutLog.UI.Tests.Components.Workout;

public class EntryErrorsTests : ComponentTestBase
{
    private static readonly FieldSpec[] Fields =
    [
        new("reps", "Reps", FieldKind.Whole, Required: true),
        new("weight", "Weight", FieldKind.Decimal, Unit: "kg"),
    ];

    [Test]
    public void NoErrors_RendersNothing()
    {
        var cut = Context.Render<EntryErrors>(p => p
            .Add(x => x.Fields, Fields)
            .Add(x => x.Entry, new EntryState()));

        Assert.That(cut.Markup.Trim(), Is.Empty);
    }

    [Test]
    public void Errors_ShowOneLineEachInFieldOrder()
    {
        var entry = new EntryState { Errors = { ["weight"] = "Weight error", ["reps"] = "Enter reps" } };
        var cut = Context.Render<EntryErrors>(p => p
            .Add(x => x.Fields, Fields)
            .Add(x => x.Entry, entry)
            .Add(x => x.TestId, "set-1-error"));

        var errors = cut.Find("[data-testid=set-1-error]");

        Assert.Multiple(() =>
        {
            Assert.That(errors.GetAttribute("role"), Is.EqualTo("alert"));
            Assert.That(errors.QuerySelectorAll(".error").Select(e => e.TextContent), Is.EqualTo(new[] { "Enter reps", "Weight error" }));
        });
    }
}
