using KitCli;
using KitCli.Abstractions.Io;
using KitCli.Commands.Abstractions.Outcomes;
using KitCli.Workflow.Abstractions;

public class TestCliApp : CliApp
{
    public TestCliApp(ICliWorkflow workflow, ICliIo io) : base(workflow, io)
    {
    }

    protected override void OnSessionStart()
    {
        Io.Say("TestCliApp session started.");
        Io.SetTitle("Test Cli App");
    }

    protected override void OnRunCreated(ICliWorkflowRun run)
    {
        Io.Say("TestCliApp run created.");
    }

    protected override void OnRunStarted(ICliWorkflowRun run, string? ask)
    {
        Io.Say("TestCliApp run started.");
    }

    protected override void OnRunComplete(ICliWorkflowRun run, Outcome[] outcomes)
    {
        Io.Say("TestCliApp run completed.");

        var statuses = run.State.Changes.Select(c => c.To);
        Io.Say($"Run state changes: {string.Join(", ", statuses)}");

        var outcomeNames = outcomes.Select(outcome => outcome.GetType().Name);
        Io.Say($"Run outcomes achieved: {string.Join(", ", outcomeNames)}");
        
        Io.Say($"Run lasted {run.State.Stopwatch.ElapsedMilliseconds}ms");
    }

    protected override void OnSessionEnd(List<ICliWorkflowRun> runs)
    {
        Io.Say("TestCliApp session ended.");
    }
}