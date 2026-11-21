using System.Text.RegularExpressions;

namespace WorkoutLog.UI.Tests.DesignSystem;

/// <summary>The components under the app's projects: every <c>*.razor</c> file except <c>_Imports</c>, and every C# class that is a component.</summary>
internal static partial class AppComponents
{
    private static readonly string[] Projects = ["WorkoutLog.UI", "WorkoutLog.App"];

    /// <summary>A component: its name (a generic's without the type parameters) and the file it is in, relative to the repo root.</summary>
    public readonly record struct Component(string Name, string Path);

    /// <summary>Every component in both projects, in file order. A name can repeat when two folders hold the same file name.</summary>
    public static IReadOnlyList<Component> Found()
    {
        var razor = Projects.SelectMany(project => RepoFiles.Find("*.razor", "src", project))
            .Where(file => Path.GetFileNameWithoutExtension(file) != "_Imports")
            .Select(file => new Component(Path.GetFileNameWithoutExtension(file), AppCss.Name(file)));
        var classes = Projects.SelectMany(project => RepoFiles.Find("*.cs", "src", project))
            .SelectMany(file => ClassesIn(File.ReadAllText(file)).Select(name => new Component(name, AppCss.Name(file))));
        return [.. razor, .. classes];
    }

    /// <summary>Names of the components, which a component with a razor file counts once.</summary>
    public static IReadOnlySet<string> Names() => Found().Select(component => component.Name).ToHashSet();

    /// <summary>Names of the components declared in C# source: classes deriving ComponentBase (qualified, or any <c>*ComponentBase</c>), implementing IComponent, or deriving another class of this source that does.</summary>
    public static IReadOnlyList<string> ClassesIn(string source)
    {
        var declared = Class().Matches(source).ToDictionary(m => m.Groups[1].Value, m => m.Groups[2].Value.Split(',').Select(Simple).ToList());
        var components = new HashSet<string>();
        bool changed;
        do
        {
            changed = false;
            foreach (var (name, bases) in declared)
            {
                if (!components.Contains(name) && bases.Any(b => components.Contains(b) || b == "IComponent" || b.EndsWith("ComponentBase", StringComparison.Ordinal)))
                {
                    components.Add(name);
                    changed = true;
                }
            }
        }
        while (changed);

        return [.. declared.Keys.Where(components.Contains)];
    }

    // The last part of a qualified name, without type arguments.
    private static string Simple(string name) => name.Trim().Split('<')[0].Split('.')[^1].Trim();

    // class Name, an optional <T> and primary constructor, then the base list up to the body or a constraint.
    [GeneratedRegex(@"\bclass\s+(\w+)\s*(?:<[^>{]*>)?\s*(?:\([^)]*\))?\s*:\s*([^{;]+?)\s*(?:where\b|\{|$)", RegexOptions.Multiline)]
    private static partial Regex Class();
}
