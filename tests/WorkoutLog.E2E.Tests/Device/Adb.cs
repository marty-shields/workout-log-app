using System.ComponentModel;
using System.Diagnostics;

namespace WorkoutLog.E2E.Tests.Device;

/// <summary>Runs adb from <c>ANDROID_HOME</c> against one device, or the only one when <paramref name="udid"/> is null.</summary>
internal sealed class Adb(string? udid)
{
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
