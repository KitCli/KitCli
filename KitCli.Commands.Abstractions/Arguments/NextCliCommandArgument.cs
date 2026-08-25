using KitCli.Instructions.Abstractions;
using KitCli.Instructions.Arguments;

namespace KitCli.Commands.Abstractions.Arguments;

/// <summary>
/// A typed argument a handler passes to the command it chains to, read back by that command's factory
/// with <c>GetArgument</c> or <c>GetRequiredArgument</c>.
/// </summary>
/// <typeparam name="TValue">The type of the argument's value.</typeparam>
/// <param name="Name">The name the next command's factory will look it up by.</param>
/// <param name="Value">The value the handler decided.</param>
public record NextCliCommandArgument<TValue>(string Name, TValue Value)
    : AnonymousNextCliCommandArgument(Name) where TValue : notnull
{
    /// <inheritdoc/>
    public override AnonymousInstructionArgument ToInstructionArgument()
        => new InstructionArgument<TValue>(Name, Value);
}
