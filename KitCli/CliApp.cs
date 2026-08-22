using KitCli.Abstractions.Io;
using KitCli.Commands.Abstractions.Io;
using KitCli.Commands.Abstractions.Outcomes;
using KitCli.Workflow.Abstractions;

namespace KitCli;

public abstract class CliApp
{
    protected readonly ICliWorkflow Workflow;
    protected readonly ICliIo Io;

    protected CliApp(ICliWorkflow workflow, ICliIo io)
    {
        Workflow = workflow;
        Io = io;
    }
    
    protected void SetUpEventHandlers()
    {
        Io.OnCancel(() =>
        {
            Workflow.Stop();

            OnSessionEnd(Workflow.Runs);

            Environment.Exit(exitCode: 0);
        });
    }

    protected void WriteOutcomes(Outcome[] outcomes, List<IOutcomeIoWriter> outcomeIoWriters)
    {
        foreach (var outcome in outcomes)
        {
            var writer = outcomeIoWriters
                .FirstOrDefault(w => w.CanWriteFor(outcome));

            writer?.Write(outcome);
        }
    }

    protected virtual void OnSessionStart()
    {
    }

    protected virtual void OnRunCreated(ICliWorkflowRun run)
    {
    }

    protected virtual void OnRunStarted(ICliWorkflowRun run, string? ask)
    {
    }

    protected virtual void OnMovingPastAsk(ICliWorkflowRun run)
    {
    }

    protected virtual void OnRunComplete(ICliWorkflowRun run, Outcome[] outcomes)
    {
    }

    protected virtual void OnSessionEnd(List<ICliWorkflowRun> runs)
    {
    }
}