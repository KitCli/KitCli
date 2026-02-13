using System.Diagnostics;
using KitCli.Commands.Abstractions.Outcomes;
using KitCli.Instructions.Abstractions;
using KitCli.Workflow.Abstractions.Run.State.Change;

namespace KitCli.Workflow.Abstractions;

public interface ICliWorkflowRunState
{
    Stopwatch Stopwatch { get; }

    List<ICliWorkflowRunStateChange> Changes { get; }
    
    bool WasChangedTo(params ClIWorkflowRunStateStatus[] oneOfStatuses);

    List<IOutcomeCliWorkflowRunStateChange> AllOutcomeStateChanges();

    void ChangeTo(ClIWorkflowRunStateStatus statusToChangeTo);

    void ChangeTo(ClIWorkflowRunStateStatus statusToChangeTo, CliInstruction instruction);

    void ChangeTo(ClIWorkflowRunStateStatus statusToChangeTo, Outcome[] outcomes);
}