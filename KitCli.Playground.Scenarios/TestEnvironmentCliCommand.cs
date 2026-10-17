using KitCli.Abstractions.Io;
using KitCli.Commands.Abstractions;
using KitCli.Commands.Abstractions.Artefacts;
using KitCli.Commands.Abstractions.Artefacts.Page;
using KitCli.Commands.Abstractions.Factories;
using KitCli.Commands.Abstractions.Handlers;
using KitCli.Commands.Abstractions.Outcomes;

namespace KitCli.Playground.Scenarios;

/// <summary>
/// The two values this scenario's commands can see: the environment, which lasts the session, and the
/// page size, which lasts one run. Every command prints them as they stand once it has finished, so the
/// environment can be seen surviving each run and the page size disappearing with its run.
/// </summary>
/// <param name="Environment">The chosen environment, or null when none has been chosen yet.</param>
/// <param name="PageSize">This run's page size, or null when this run has not set one.</param>
public record TestVisibleValues(string? Environment, int? PageSize)
{
    public string Describe()
    {
        var environment = Environment ?? "none";
        var pageSize = PageSize?.ToString() ?? "none";

        return $"Environment: {environment}. Page size: {pageSize}.";
    }
}

/// <summary>
/// Chooses an environment for the whole session. Its run ends straight away, yet every later run sees
/// the environment, because <see cref="TestEnvironmentChosenOutcome"/> lasts the session. Recorded in
/// <c>docs/investigations/can-an-artefact-outlive-the-run-that-made-it.md</c>.
/// </summary>
/// <param name="Environment">The environment to use, or null when none was typed.</param>
/// <param name="VisibleValues">The values this command saw when it started.</param>
public record TestEnvironmentCliCommand(string? Environment, TestVisibleValues VisibleValues) : CliCommand;

public class TestEnvironmentCliCommandFactory : BasicCreationCliCommandFactory<TestEnvironmentCliCommand>
{
    public override CliCommand Create()
    {
        var environmentArtefact = GetArtefact<string>(nameof(TestEnvironmentArtefact));
        var pageSizeArtefact = GetArtefact<int>(nameof(PageSizeArtefact.PageSize));

        var visibleValues = new TestVisibleValues(environmentArtefact?.Value, pageSizeArtefact?.Value);

        return new TestEnvironmentCliCommand(Instruction.SubInstructionName, visibleValues);
    }
}

public record TestEnvironmentChosenOutcome(string Environment)
    : Outcome(OutcomeKind.Anonymous, OutcomeLifetime.Session);

public record TestEnvironmentArtefact(string Environment)
    : Artefact<string>(nameof(TestEnvironmentArtefact), Environment);

public class TestEnvironmentArtefactFactory : ArtefactFactory<TestEnvironmentChosenOutcome>
{
    protected override AnonymousArtefact CreateArtefact(TestEnvironmentChosenOutcome outcome)
        => new TestEnvironmentArtefact(outcome.Environment);
}

public class TestEnvironmentCliCommandHandler : CliCommandHandler<TestEnvironmentCliCommand>
{
    public override Task<Outcome[]> HandleCommand(TestEnvironmentCliCommand command, CancellationToken cancellationToken)
    {
        if (command.Environment is null)
        {
            return FinishThisCommand()
                .ByFinallySaying("Name an environment, e.g. /test-environment prod.", MessageLevel.Error)
                .EndAsync();
        }

        var chosenOutcome = new TestEnvironmentChosenOutcome(command.Environment);
        var valuesAfter = command.VisibleValues with { Environment = command.Environment };
        var description = valuesAfter.Describe();

        return FinishThisCommand()
            .ByResultingIn(chosenOutcome)
            .BySaying($"Using {command.Environment}.")
            .ByFinallySaying(description)
            .EndAsync();
    }
}

/// <summary>
/// Sets a page size for this run only. It ends on a reusable outcome, so the run stays open for the next
/// command, and the page size goes once that run ends.
/// </summary>
/// <param name="PageSize">The page size to use, or null when none was typed or it was not a number.</param>
/// <param name="VisibleValues">The values this command saw when it started.</param>
public record TestPageSizeCliCommand(int? PageSize, TestVisibleValues VisibleValues) : CliCommand;

public class TestPageSizeCliCommandFactory : BasicCreationCliCommandFactory<TestPageSizeCliCommand>
{
    public override CliCommand Create()
    {
        var environmentArtefact = GetArtefact<string>(nameof(TestEnvironmentArtefact));
        var pageSizeArtefact = GetArtefact<int>(nameof(PageSizeArtefact.PageSize));

        var visibleValues = new TestVisibleValues(environmentArtefact?.Value, pageSizeArtefact?.Value);

        var typedPageSize = Instruction.SubInstructionName;
        var isNumber = int.TryParse(typedPageSize, out var pageSize);
        int? parsedPageSize = isNumber ? pageSize : null;

        return new TestPageSizeCliCommand(parsedPageSize, visibleValues);
    }
}

public class TestPageSizeCliCommandHandler : CliCommandHandler<TestPageSizeCliCommand>
{
    public override Task<Outcome[]> HandleCommand(TestPageSizeCliCommand command, CancellationToken cancellationToken)
    {
        if (command.PageSize is null)
        {
            return FinishThisCommand()
                .ByFinallySaying("Give a page size as a number, e.g. /test-page-size 20.", MessageLevel.Error)
                .EndAsync();
        }

        var valuesAfter = command.VisibleValues with { PageSize = command.PageSize };
        var description = valuesAfter.Describe();

        return FinishThisCommand()
            .BySaying(description)
            .ByRememberingPageSize(command.PageSize.Value)
            .EndAsync();
    }
}

/// <summary>
/// Changes nothing. It prints the values as it found them, then ends its run.
/// </summary>
/// <param name="VisibleValues">The values this command saw when it started.</param>
public record TestExecuteCliCommand(TestVisibleValues VisibleValues) : CliCommand;

public class TestExecuteCliCommandFactory : BasicCreationCliCommandFactory<TestExecuteCliCommand>
{
    public override CliCommand Create()
    {
        var environmentArtefact = GetArtefact<string>(nameof(TestEnvironmentArtefact));
        var pageSizeArtefact = GetArtefact<int>(nameof(PageSizeArtefact.PageSize));

        var visibleValues = new TestVisibleValues(environmentArtefact?.Value, pageSizeArtefact?.Value);

        return new TestExecuteCliCommand(visibleValues);
    }
}

public class TestExecuteCliCommandHandler : CliCommandHandler<TestExecuteCliCommand>
{
    public override Task<Outcome[]> HandleCommand(TestExecuteCliCommand command, CancellationToken cancellationToken)
    {
        var description = command.VisibleValues.Describe();

        return FinishThisCommand()
            .BySaying("Executed.")
            .ByFinallySaying(description)
            .EndAsync();
    }
}
