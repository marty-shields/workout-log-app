using System.Text;

namespace WorkoutLog.Core.Domain.Exercises;

/// <summary>Normalises exercise names, nicknames and queries for search (docs/03).</summary>
internal static class SearchText
{
    private static readonly Dictionary<string, string> Variants = new()
    {
        ["triceps"] = "tricep",
        ["biceps"] = "bicep",
        ["flies"] = "fly",
        ["flyes"] = "fly",
        ["flye"] = "fly",
        ["pullup"] = "pull up",
        ["pullups"] = "pull up",
        ["pushup"] = "push up",
        ["pushups"] = "push up",
        ["chinup"] = "chin up",
        ["chinups"] = "chin up",
        ["situp"] = "sit up",
        ["situps"] = "sit up",
        ["stepup"] = "step up",
        ["stepups"] = "step up",
        ["ups"] = "up",
        ["nordics"] = "nordic",
        ["skullcrusher"] = "skull crusher",
    };

    /// <summary>Two-word variants, by their joined form: a word that starts past the first part is split there, e.g. pullu → pull u.</summary>
    private static readonly (string Joined, string First)[] Joined = Variants.Values
        .Distinct()
        .Where(v => v.Contains(' '))
        .Select(v => (v.Replace(" ", ""), v[..v.IndexOf(' ')]))
        .ToArray();

    /// <summary>
    /// Lower-case words. Apostrophes are removed and any other character that isn't a letter or digit separates words.
    /// Variants are replaced, and a partly typed two-word variant is split as the full one would be.
    /// </summary>
    public static string[] Words(string? text) =>
        Split(text).SelectMany(Normalise).ToArray();

    private static IEnumerable<string> Split(string? text)
    {
        var word = new StringBuilder();
        foreach (var c in text ?? "")
        {
            if (c is '\'' or '’')
            {
                continue;
            }

            if (char.IsLetterOrDigit(c))
            {
                word.Append(char.ToLowerInvariant(c));
            }
            else if (word.Length > 0)
            {
                yield return word.ToString();
                word.Clear();
            }
        }

        if (word.Length > 0)
        {
            yield return word.ToString();
        }
    }

    private static IEnumerable<string> Normalise(string word)
    {
        if (Variants.TryGetValue(word, out var replacement))
        {
            return replacement.Split(' ');
        }

        foreach (var (joined, first) in Joined)
        {
            if (word.Length > first.Length && joined.StartsWith(word, StringComparison.Ordinal))
            {
                return [first, word[first.Length..]];
            }
        }

        return [word];
    }
}
