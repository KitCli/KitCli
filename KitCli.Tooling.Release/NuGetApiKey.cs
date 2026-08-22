namespace KitCli.Tooling.Release;

public static class NuGetApiKey
{
    public static readonly string FilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".kitcli", "nuget-api-key");

    public static string? Read()
    {
        var fromEnv = Environment.GetEnvironmentVariable("NUGET_API_KEY");
        if (!string.IsNullOrWhiteSpace(fromEnv))
            return fromEnv.Trim();

        return File.Exists(FilePath) ? File.ReadAllText(FilePath).Trim() : null;
    }
}
