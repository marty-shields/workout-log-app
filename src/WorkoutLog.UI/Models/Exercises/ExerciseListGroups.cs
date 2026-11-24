using System.Collections;

namespace WorkoutLog.UI.Models.Exercises;

/// <summary>
/// The add-exercise rows in two groups: the exercises with a saved workout, then the rest. As a list it is both groups in that order.
/// </summary>
public sealed class ExerciseListGroups : IReadOnlyList<ExerciseListItem>
{
    /// <summary>No rows, not grouped.</summary>
    public static readonly ExerciseListGroups None = new([], [], false);

    private readonly IReadOnlyList<ExerciseListItem> all;

    /// <summary>The groups. <paramref name="grouped"/> is whether the list shows headers.</summary>
    public ExerciseListGroups(IReadOnlyList<ExerciseListItem> recent, IReadOnlyList<ExerciseListItem> other, bool grouped)
    {
        Recent = recent;
        Other = other;
        Grouped = grouped;
        all = [.. recent, .. other];
    }

    /// <summary>Exercises with a saved workout, as they are listed.</summary>
    public IReadOnlyList<ExerciseListItem> Recent { get; }

    /// <summary>The rest, as they are listed.</summary>
    public IReadOnlyList<ExerciseListItem> Other { get; }

    /// <summary>
    /// True when the list shows a header over each group that has rows: some exercise has a saved workout, even if the search or chip hides it.
    /// False for the one flat list, with no headers.
    /// </summary>
    public bool Grouped { get; }

    /// <inheritdoc />
    public int Count => all.Count;

    /// <inheritdoc />
    public ExerciseListItem this[int index] => all[index];

    /// <summary>One flat list, with no headers, in the order given.</summary>
    public static ExerciseListGroups Flat(IReadOnlyList<ExerciseListItem> items) => new([], items, false);

    /// <inheritdoc />
    public IEnumerator<ExerciseListItem> GetEnumerator() => all.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
