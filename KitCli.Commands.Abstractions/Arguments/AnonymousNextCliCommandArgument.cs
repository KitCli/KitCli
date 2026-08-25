using KitCli.Instructions.Abstractions;

namespace KitCli.Commands.Abstractions.Arguments;

/// <summary>
/// An argument a handler passes to the command it chains to, before it has been put on that command's
/// instruction. Nothing here was typed by the user; a handler decided it.
/// </summary>
/// <param name="Name">The name the next command's factory will look it up by.</param>
public abstract record AnonymousNextCliCommandArgument(string Name)
{
    /// <summary>
    /// Puts this argument in the box a factory reads from. The value is already typed, so this is a
    /// change of container rather than a conversion — there is nothing to parse and nothing to decide,
    /// which is why it is a method here rather than a registered factory.
    /// </summary>
    /// <returns>The same name and value as an instruction argument.</returns>
    public abstract AnonymousInstructionArgument ToInstructionArgument();
}
