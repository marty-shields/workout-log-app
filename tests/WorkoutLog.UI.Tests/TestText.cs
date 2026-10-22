namespace WorkoutLog.UI.Tests;

/// <summary>Text helpers for assertions on rendered markup.</summary>
public static class TestText
{
    /// <summary>Collapses runs of whitespace to single spaces and trims the ends.</summary>
    public static string Normalise(string text) =>
        string.Join(' ', text.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));
}
