namespace KitCli.Tooling.Release;

public static class ProjectGraph
{
    public static List<ProjectInfo> TopologicalOrder(List<ProjectInfo> projects)
    {
        var byPath = projects.ToDictionary(p => p.Path, StringComparer.OrdinalIgnoreCase);
        var visited = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase); // 0 = visiting, 1 = done
        var ordered = new List<ProjectInfo>();

        void Visit(ProjectInfo project, List<string> stack)
        {
            if (visited.TryGetValue(project.Path, out var state))
            {
                if (state == 0)
                    throw new ReleaseException(ReleaseExceptionCode.CircularProjectReference, $"Circular ProjectReference detected: {string.Join(" -> ", stack.Append(project.PackageId))}");
                return;
            }

            visited[project.Path] = 0;
            stack.Add(project.PackageId);
            foreach (var reference in project.ProjectReferences)
            {
                if (byPath.TryGetValue(reference, out var dependency))
                    Visit(dependency, stack);
            }
            stack.RemoveAt(stack.Count - 1);
            visited[project.Path] = 1;
            ordered.Add(project);
        }

        foreach (var project in projects)
            Visit(project, new List<string>());

        return ordered;
    }

    public static void ValidateReachableFromRoot(List<ProjectInfo> projects, string rootPackageId)
    {
        var root = projects.FirstOrDefault(p => p.PackageId == rootPackageId);
        if (root is null)
        {
            Console.WriteLine($"WARNING: root package '{rootPackageId}' not found among packable projects; skipping reachability check.");
            return;
        }

        var byPath = projects.ToDictionary(p => p.Path, StringComparer.OrdinalIgnoreCase);
        var reachable = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        void Walk(ProjectInfo project)
        {
            if (!reachable.Add(project.Path))
                return;
            foreach (var reference in project.ProjectReferences)
            {
                if (byPath.TryGetValue(reference, out var dependency))
                    Walk(dependency);
            }
        }

        Walk(root);

        var orphans = projects.Where(p => !reachable.Contains(p.Path)).ToList();
        if (orphans.Count > 0)
        {
            Console.WriteLine($"WARNING: the following packages are published but are not reachable transitively from '{rootPackageId}' " +
                               "— a consumer who only installs the root package will not pull them in:");
            foreach (var orphan in orphans)
                Console.WriteLine($"  - {orphan.PackageId}");
        }
    }
}
