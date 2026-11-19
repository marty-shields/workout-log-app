using System.Globalization;
using Reqnroll;
using WorkoutLog.E2E.Tests.Device;
using WorkoutLog.E2E.Tests.Screens;
using WorkoutLog.E2E.Tests.Seeding;
using WorkoutLog.E2E.Tests.Support;

namespace WorkoutLog.E2E.Tests.Steps;

/// <summary>The last sessions on the add-exercise list.</summary>
[Binding]
public sealed class AddExerciseLastSessionSteps(WebView web, AddExerciseScreen addExercise, DeviceClock clock)
{
    [Then(@"^the add-exercise row for ""(.*)"" reads Last (.+)$")]
    public void ThenTheAddExerciseRowReads(string name, string last) =>
        web.Eventually(() => addExercise.Last(Exercises.Named(name).Id), Is.EqualTo(Last(last, clock.LocalNow())));

    /// <summary>"Last" and the day, with the year only when it isn't this year, then the best if given.</summary>
    private static string Last(string text, DateTime now)
    {
        var parts = text.Split(" · ", 2);
        var day = SeedTime.ResolveDay(parts[0], now);
        var date = day.ToString(day.Year == now.Year ? "ddd d MMM" : "ddd d MMM yyyy", CultureInfo.InvariantCulture);
        return parts.Length == 2 ? $"Last {date} · {parts[1]}" : $"Last {date}";
    }
}
