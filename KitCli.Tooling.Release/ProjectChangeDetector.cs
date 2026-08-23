namespace KitCli.Tooling.Release;

public static class ProjectChangeDetector
{
    // VersionBumper always writes the exact string "<Version>{version}</Version>", so a pickaxe
    // search for that string finds the commit that last set this project's currently-committed
    // version — i.e. its last release — without needing tags or a separate log of release commits.
    // If no such commit exists, this project has never been released from the visible history, so
    // every commit that's touched it counts as "since last release".
    public static bool HasChangedSinceLastRelease(string repoRoot, ProjectInfo project)
    {
        var relativeCsprojPath = ToGitPath(repoRoot, project.Path);
        var versionMarker = $"<Version>{project.Version}</Version>";

        var lastReleaseCommit = GitCli.Run(repoRoot, "log", "-1", "--format=%H", $"-S{versionMarker}", "--", relativeCsprojPath).Trim();

        var relativeProjectDir = ToGitPath(repoRoot, Path.GetDirectoryName(project.Path)!);
        var range = lastReleaseCommit.Length == 0 ? "HEAD" : $"{lastReleaseCommit}..HEAD";

        var commitsSinceLastRelease = GitCli.Run(repoRoot, "log", "--format=%H", range, "--", relativeProjectDir).Trim();
        return commitsSinceLastRelease.Length > 0;
    }

    private static string ToGitPath(string repoRoot, string path) =>
        Path.GetRelativePath(repoRoot, path).Replace(Path.DirectorySeparatorChar, '/');
}
