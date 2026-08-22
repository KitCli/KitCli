using System.Xml.Linq;

namespace KitCli.Tooling.Release;

public static class ProjectDiscovery
{
    public static List<ProjectInfo> DiscoverPackableProjects(string repoRoot)
    {
        var result = new List<ProjectInfo>();
        foreach (var csprojPath in Directory.EnumerateFiles(repoRoot, "*.csproj", SearchOption.AllDirectories))
        {
            if (csprojPath.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}") ||
                csprojPath.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}"))
                continue;

            var doc = XDocument.Load(csprojPath);
            var packageId = doc.Descendants("PackageId").FirstOrDefault()?.Value;
            var versionElement = doc.Descendants("Version").FirstOrDefault();
            if (packageId is null || versionElement is null)
                continue;

            var projectDir = Path.GetDirectoryName(csprojPath)!;
            var references = doc.Descendants("ProjectReference")
                .Select(e => e.Attribute("Include")?.Value)
                .Where(v => v is not null)
                .Select(v => Path.GetFullPath(Path.Combine(projectDir, v!.Replace('\\', Path.DirectorySeparatorChar))))
                .ToList();

            result.Add(new ProjectInfo(csprojPath, packageId, versionElement.Value, references));
        }
        return result;
    }
}
