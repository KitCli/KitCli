using System.Diagnostics;

namespace KitCli.Tooling.Release;

public static class GitCli
{
    public static string Run(string repoRoot, params string[] args)
    {
        var startInfo = new ProcessStartInfo("git")
        {
            WorkingDirectory = repoRoot,
            UseShellExecute = false,
            RedirectStandardOutput = true,
        };
        foreach (var arg in args)
            startInfo.ArgumentList.Add(arg);

        using var process = Process.Start(startInfo) ?? throw new ReleaseException(ReleaseExceptionCode.ProcessFailedToStart, "Failed to start git process.");
        var output = process.StandardOutput.ReadToEnd();
        process.WaitForExit();
        if (process.ExitCode != 0)
            throw new ReleaseException(ReleaseExceptionCode.GitCommandFailed, $"git {string.Join(' ', args)} failed with exit code {process.ExitCode}.");
        return output;
    }
}
