using KitCli.Instructions.Abstractions;
using KitCli.Workflow.Abstractions;
using KitCli.Workflow.Abstractions.Run.State.Change;
using NUnit.Framework;

namespace KitCli.Workflow.Tests.Run.State.Change;

[TestFixture]
public class InstructionCliWorkflowStateChangeTests : CliWorkflowRunStateTests
{
    [Test]
    public void GivenInstructionInWrongState_WhenChangeTo_CannotChangeState()
    {
        // Arrange
        var priorStatuses = new[]
        {
            ClIWorkflowRunStateStatus.Running,
            ClIWorkflowRunStateStatus.Exceptional,
            ClIWorkflowRunStateStatus.Finished 
        };

        var state = GetPreparedState(priorStatuses);

        var instruction = new CliInstruction(null, null, null, []);
        
        // Act & Assert
        Assert.Throws<ImpossibleStateChangeException>(() => state.ChangeTo(ClIWorkflowRunStateStatus.Running, instruction));
    }

    [Test]
    public void GivenInstructionInCorrectState_WhenChangeTo_CanChangeState()
    {
        // Arrange
        var state = GetPreparedState([]);

        var instruction = new CliInstruction(null, null, null, []);
        
        // Act
        state.ChangeTo(ClIWorkflowRunStateStatus.Running, instruction);
        
        // Assert
        var lastChange = state.Changes.Last();
        
        Assert.That(lastChange, Is.InstanceOf<IInstructionCliWorkflowRunStateChange>());
        
        var instructionStateChange = (IInstructionCliWorkflowRunStateChange)lastChange;
        
        Assert.That(instructionStateChange.Instruction, Is.EqualTo(instruction));
    }
}