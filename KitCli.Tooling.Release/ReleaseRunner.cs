namespace KitCli.Tooling.Release;

public class ReleaseRunner(string repoRoot)
{
    private const string NuGetSource = "https://api.nuget.org/v3/index.json";
    private const string RootPackageId = "KitCli";

    public void Run(bool dryRun, bool publish)
    {
        Console.WriteLine($"Repo root: {repoRoot}");

        var projects = ProjectDiscovery.DiscoverPackableProjects(repoRoot);
        if (projects.Count == 0)
            throw new ReleaseException(ReleaseExceptionCode.NoPackableProjectsFound, "No packable projects found (expected csproj files with both <PackageId> and <Version>).");

        var order = ProjectGraph.TopologicalOrder(projects);
        ProjectGraph.ValidateReachableFromRoot(projects, RootPackageId);

        Console.WriteLine("Publish order (dependencies first):");
        foreach (var project in order)
            Console.WriteLine($"  - {project.PackageId}");

        // A project needs a new version if it changed itself, or if anything it references
        // (directly or transitively) is getting bumped — otherwise a consumer who only upgrades
        // the root package wouldn't actually receive a lower-level fix/feature. `order` is
        // dependencies-first, so by the time a project is checked, everything it references has
        // already been decided.
        var toBump = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var project in order)
        {
            var changed = ProjectChangeDetector.HasChangedSinceLastRelease(repoRoot, project);
            var dependsOnBumped = project.ProjectReferences.Any(toBump.Contains);
            if (changed || dependsOnBumped)
                toBump.Add(project.Path);
        }

        var bumping = order.Where(project => toBump.Contains(project.Path)).ToList();
        if (bumping.Count == 0)
        {
            Console.WriteLine("No packable project has changed since its last release; nothing to bump or publish.");
            return;
        }

        Console.WriteLine("Bumping (changed, or depends on something that changed):");
        foreach (var project in bumping)
            Console.WriteLine($"  - {project.PackageId}");

        if (dryRun)
        {
            Console.WriteLine("Dry run: no files were modified and no commands were run.");
            return;
        }

        foreach (var project in bumping)
            VersionBumper.BumpPatchVersion(project);

        BuildSolution();

        var nupkgDir = Path.Combine(repoRoot, "nupkgs");
        Directory.CreateDirectory(nupkgDir);

        foreach (var project in bumping)
            PackAndPublish(project, nupkgDir, publish);
    }

    // A single solution-wide build, run after every version bump is written to disk, so that
    // `dotnet pack --no-build` below packs binaries built against the *new* version numbers
    // instead of stale output from whatever build happened to exist before this tool ran.
    private void BuildSolution() =>
        DotnetCli.Run(repoRoot, $"build \"{Path.Combine(repoRoot, "KitCli.sln")}\" --configuration Release");

    private void PackAndPublish(ProjectInfo project, string nupkgDir, bool publish)
    {
        DotnetCli.Run(repoRoot, $"pack \"{project.Path}\" --configuration Release --no-build --output \"{nupkgDir}\"");

        var nupkgPath = Path.Combine(nupkgDir, $"{project.PackageId}.{project.Version}.nupkg");
        if (!File.Exists(nupkgPath))
        {
            Console.WriteLine($"WARNING: expected package not found: {nupkgPath}");
            return;
        }

        if (!publish)
        {
            Console.WriteLine($"Packed {nupkgPath} (pass --publish to push to NuGet).");
            return;
        }

        var apiKey = NuGetApiKey.Read();
        if (apiKey is null)
        {
            Console.WriteLine($"No API key found (checked NUGET_API_KEY and {NuGetApiKey.FilePath}). Skipping push for {nupkgPath}.");
            return;
        }

        try
        {
            DotnetCli.Run(repoRoot, $"nuget push \"{nupkgPath}\" --api-key {apiKey} --source {NuGetSource} --skip-duplicate");
            Console.WriteLine($"Published {nupkgPath} to NuGet.");
        }
        catch (ReleaseException)
        {
            Console.WriteLine($"Failed to push {nupkgPath}.");
        }
    }
}
