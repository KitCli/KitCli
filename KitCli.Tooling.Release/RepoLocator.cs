namespace KitCli.Tooling.Release;

public static class RepoLocator
{
    public static string? FindRepoRoot(string startDirectory)
    {
        var dir = new DirectoryInfo(startDirectory);
        while (dir is not null)
        {
            if (dir.EnumerateFiles("KitCli.sln").Any())
                return dir.FullName;
            dir = dir.Parent;
        }
        return null;
    }
}
