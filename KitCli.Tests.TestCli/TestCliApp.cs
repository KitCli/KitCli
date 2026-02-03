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
        Console.WriteLine("TestCliApp session started.");
        Io.SetTitle("Test Cli App");
    }

    protected override void OnRunCreated(ICliWorkflowRun run)
    {
        Console.WriteLine("TestCliApp run created.");
    }

    protected override void OnRunStarted(ICliWorkflowRun run, string? ask)
    {
        Console.WriteLine("TestCliApp run started.");
    }

    protected override void OnRunComplete(ICliWorkflowRun run, CliCommandOutcome[] outcomes)
    {
        Console.WriteLine("TestCliApp run completed.");
    }

    protected override void OnSessionEnd(List<ICliWorkflowRun> runs)
    {
        Console.WriteLine("TestCliApp session ended.");
    }
}