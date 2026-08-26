using KitCli.Commands.Abstractions.Factories;

namespace KitCli.Commands.Abstractions.Tests.TestHelpers;

/// <summary>A creation factory whose <see cref="Create"/> reads the attached instruction's sub-instruction name.</summary>
public class TestCreationCliCommandFactory : BasicCreationCliCommandFactory<TestParameterisedNextCliCommand>
{
    public override CliCommand Create() => new TestParameterisedNextCliCommand(Instruction.SubInstructionName!);
}
