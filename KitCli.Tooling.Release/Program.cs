using KitCli.Tooling.Release;

var repoRoot = RepoLocator.FindRepoRoot(AppContext.BaseDirectory)
    ?? throw new ReleaseException(ReleaseExceptionCode.SolutionNotFound, "Could not locate KitCli.sln above the tool's build output directory.");

new ReleaseRunner(repoRoot).Run(dryRun: args.Contains("--dry-run"), publish: args.Contains("--publish"));
