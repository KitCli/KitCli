using KitCli.Commands.Abstractions;
using KitCli.Commands.Abstractions.Factories;
using KitCli.Commands.Abstractions.Handlers;
using KitCli.Commands.Abstractions.Outcomes;

namespace KitCli.Tooling.Release;

public record ReleaseCliCommand(bool DryRun, bool Publish) : CliCommand;

public class ReleaseCliCommandFactory : CliCommandFactory<ReleaseCliCommand>
{
    public override bool CanCreateWhen() => true;

    public override CliCommand Create()
    {
        var dryRun = GetArgument<bool>("dry-run")?.Value ?? false;
        var publish = GetArgument<bool>("publish")?.Value ?? false;

        return new ReleaseCliCommand(dryRun, publish);
    }
}

public class ReleaseCliCommandHandler : CliCommandHandler<ReleaseCliCommand>
{
    public override Task<Outcome[]> HandleCommand(ReleaseCliCommand command, CancellationToken cancellationToken)
    {
        var repoRoot = RepoLocator.FindRepoRoot(AppContext.BaseDirectory)
            ?? throw new ReleaseException(ReleaseExceptionCode.SolutionNotFound, "Could not locate KitCli.sln above the tool's build output directory.");

        new ReleaseRunner(repoRoot).Run(command.DryRun, command.Publish);

        return FinishThisCommand()
            .ByFinallySaying("Release run complete.")
            .EndAsync();
    }
}
