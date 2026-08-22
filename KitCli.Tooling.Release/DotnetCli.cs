using System.Diagnostics;
using System.Text.RegularExpressions;

namespace KitCli.Tooling.Release;

public static class DotnetCli
{
    public static void Run(string workingDirectory, string arguments)
    {
        Console.WriteLine($"$ dotnet {Redact(arguments)}");
        var startInfo = new ProcessStartInfo("dotnet", arguments)
        {
            WorkingDirectory = workingDirectory,
            UseShellExecute = false,
        };
        using var process = Process.Start(startInfo) ?? throw new ReleaseException(ReleaseExceptionCode.ProcessFailedToStart, "Failed to start dotnet process.");
        process.WaitForExit();
        if (process.ExitCode != 0)
            throw new ReleaseException(ReleaseExceptionCode.DotnetCommandFailed, $"dotnet {Redact(arguments)} failed with exit code {process.ExitCode}.");
    }

    // Never let the raw arguments string reach Console.WriteLine or an exception message
    // unredacted — both end up in terminal scrollback/logs, and --api-key's value is a secret.
    private static string Redact(string arguments) =>
        Regex.Replace(arguments, "--api-key \\S+", "--api-key ***");
}
