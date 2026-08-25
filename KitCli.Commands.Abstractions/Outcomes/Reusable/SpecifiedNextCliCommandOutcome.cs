using KitCli.Instructions.Abstractions;

namespace KitCli.Commands.Abstractions.Outcomes.Reusable;

/// <summary>
/// The next command, specified by type rather than built. Nothing is constructed when the handler names
/// it: whoever runs it resolves <see cref="SpecifiedCommandType"/> through the same factory path any
/// instruction takes, so the factory sees the run's accumulated artefacts, plus any
/// <see cref="Arguments"/> the handler chose to pass on.
/// </summary>
/// <param name="SpecifiedCommandType">The type of the command to move to.</param>
/// <param name="Arguments">
/// Arguments to put on the instruction the run builds for it, as though the user had typed them. Empty
/// when the handler passes none, in which case the factory has only artefacts to work from.
/// </param>
public record SpecifiedNextCliCommandOutcome(
    Type SpecifiedCommandType,
    List<AnonymousInstructionArgument> Arguments) : NextCliCommandOutcome
{
    /// <summary>Specifies the next command with no arguments.</summary>
    /// <param name="specifiedCommandType">The type of the command to move to.</param>
    public SpecifiedNextCliCommandOutcome(Type specifiedCommandType)
        : this(specifiedCommandType, [])
    {
    }
}
