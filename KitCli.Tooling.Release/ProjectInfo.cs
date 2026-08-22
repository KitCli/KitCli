namespace KitCli.Tooling.Release;

public class ProjectInfo(string path, string packageId, string version, List<string> projectReferences)
{
    public string Path { get; } = path;
    public string PackageId { get; } = packageId;
    public string Version { get; set; } = version;
    public List<string> ProjectReferences { get; } = projectReferences;
}
