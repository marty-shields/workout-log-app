using System.ComponentModel;
using System.Diagnostics;

namespace WorkoutLog.E2E.Tests.Device;

/// <summary>Runs adb from <c>ANDROID_HOME</c> against one device, or the only one when <paramref name="udid"/> is null.</summary>
internal sealed class Adb(string? udid)
{
    private const string Downloads = "/sdcard/Download";

    private static readonly string Executable = Path.Combine(Environment.GetEnvironmentVariable("ANDROID_HOME") ?? "", "platform-tools", "adb");

    /// <summary>Runs adb and returns its standard output; throws if it exits with an error.</summary>
    public string Run(params string[] args)
    {
        var (exitCode, output, error) = Start(args);
        if (exitCode != 0)
        {
            throw new InvalidOperationException($"adb {string.Join(" ", args)} failed ({exitCode}): {(output + error).Trim()}");
        }

        return output;
    }

    /// <summary>Runs <c>adb shell</c> and returns its trimmed standard output; throws if it exits with an error.</summary>
    public string Shell(params string[] args) => Run(["shell", .. args]).Trim();

    /// <summary>The <c>versionName</c> installed for <paramref name="package"/>, the display version the app shows.</summary>
    public string VersionName(string package)
    {
        var line = Shell("dumpsys", "package", package)
            .Split('\n', StringSplitOptions.TrimEntries)
            .FirstOrDefault(l => l.StartsWith("versionName=", StringComparison.Ordinal));
        return line?["versionName=".Length..] is { Length: > 0 } name
            ? name
            : throw new InvalidOperationException($"dumpsys package {package} has no versionName: is it installed?");
    }

    /// <summary>
    /// Runs <c>adb shell</c> with access to <paramref name="app"/>'s private folder: as the app with run-as when it is the Debug app,
    /// otherwise as root, which an emulator allows. Give paths under <see cref="TestedApp.DataFolder"/>.
    /// </summary>
    public string AppShell(TestedApp app, params string[] args) =>
        Shell([.. app.IsDebug ? (string[])["run-as", app.Package] : ["su", "0"], .. args]);

    /// <summary>Writes <paramref name="content"/> to the device's Download folder as <paramref name="name"/>, through a temp file.</summary>
    public void WriteToDownloads(string name, string content)
    {
        var path = Path.Combine(Path.GetTempPath(), $"workoutlog-{Guid.NewGuid():N}");
        try
        {
            File.WriteAllText(path, content);
            PushToDownloads(path, name);
        }
        finally
        {
            File.Delete(path);
        }
    }

    /// <summary>Copies the file at <paramref name="localPath"/> to the device's Download folder as <paramref name="name"/>, then rescans shared storage so the file picker lists it.</summary>
    private void PushToDownloads(string localPath, string name)
    {
        Run("push", localPath, $"{Downloads}/{name}");
        ScanSharedStorage();
    }

    /// <summary>Deletes <paramref name="name"/> from the device's Download folder, then rescans shared storage.</summary>
    public void RemoveFromDownloads(string name)
    {
        Shell("rm", "-f", $"{Downloads}/{name}");
        ScanSharedStorage();
    }

    private void ScanSharedStorage() => Shell("content", "call", "--uri", "content://media", "--method", "scan_volume", "--arg", "external_primary");

    /// <summary>Runs adb; returns false with the output when it fails or can't start.</summary>
    public bool TryRun(out string output, params string[] args)
    {
        try
        {
            var (exitCode, stdout, error) = Start(args);
            output = (stdout + error).Trim();
            return exitCode == 0;
        }
        catch (Win32Exception e)
        {
            output = e.Message;
            return false;
        }
    }

    private (int ExitCode, string Output, string Error) Start(string[] args)
    {
        var start = new ProcessStartInfo(Executable) { RedirectStandardOutput = true, RedirectStandardError = true };
        if (udid is not null)
        {
            start.ArgumentList.Add("-s");
            start.ArgumentList.Add(udid);
        }

        foreach (var arg in args)
        {
            start.ArgumentList.Add(arg);
        }

        using var process = Process.Start(start)!;
        var error = process.StandardError.ReadToEndAsync();
        var output = process.StandardOutput.ReadToEnd();
        process.WaitForExit();
        return (process.ExitCode, output, error.Result);
    }
}
