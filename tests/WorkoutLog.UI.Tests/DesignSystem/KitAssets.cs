using System.Text.Json;

namespace WorkoutLog.UI.Tests.DesignSystem;

/// <summary>The kit's static web assets (<c>_content/Shields.UI/...</c>), from the package or from the kit source, whichever the build uses.</summary>
internal static class KitAssets
{
    private static readonly JsonElement Manifest = LoadManifest();

    /// <summary>Text of a kit asset, such as <c>css/tokens.css</c>.</summary>
    public static string Read(string relativePath) => File.ReadAllText(PathOf(relativePath));

    private static string PathOf(string relativePath)
    {
        var node = relativePath.Split('/').Aggregate(Manifest.GetProperty("Root").GetProperty("Children").GetProperty("_content").GetProperty("Children").GetProperty("Shields.UI"),
            (current, part) => current.GetProperty("Children").GetProperty(part));
        var asset = node.GetProperty("Asset");
        var root = Manifest.GetProperty("ContentRoots")[asset.GetProperty("ContentRootIndex").GetInt32()].GetString()!;
        return Path.Combine(root, asset.GetProperty("SubPath").GetString()!);
    }

    // This test project's own manifest lists Shields.UI's assets through its reference to WorkoutLog.UI.
    private static JsonElement LoadManifest() =>
        JsonElement.Parse(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "WorkoutLog.UI.Tests.staticwebassets.runtime.json")));
}
