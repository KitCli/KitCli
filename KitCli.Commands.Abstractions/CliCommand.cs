using KitCli.Commands.Abstractions.Extensions;
using KitCli.Commands.Abstractions.Outcomes;
using KitCli.Instructions.Abstractions;
using MediatR;

[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("KitCli.Commands.Abstractions.Tests")]
namespace KitCli.Commands.Abstractions;

/// <summary>
/// A command that can be executed via the CLI.
/// For example, "List all transactions for payee X".
/// </summary>
public record CliCommand : IRequest<Outcome[]>
{
    internal string GetSpecificCommandName()
        => GetType().Name.ReplaceCommandSuffix();

    /// <summary>
    /// Derives the dashed instruction name this command responds to, by removing <c>CliCommand</c>
    /// from the type name and inserting a separator before every uppercase letter (except the first).
    /// For example, <c>SpareMoneyCliCommand</c> becomes <c>spare-money</c>. A type named
    /// <c>SpareMoneyCommand</c> keeps its suffix and becomes <c>spare-money-command</c>.
    /// </summary>
    /// <returns>The instruction name derived from this command's type.</returns>
    public string GetInstructionName()
        => GetInstructionName(GetType());

    /// <summary>
    /// Derives the dashed instruction name a command type responds to, without needing an instance of it.
    /// This is the key its factory is registered under, and the name the workflow run puts in the
    /// instruction it builds when a handler chains to that type.
    /// </summary>
    /// <param name="commandType">The command type to derive a name from.</param>
    /// <returns>The instruction name derived from <paramref name="commandType"/>.</returns>
    public static string GetInstructionName(Type commandType)
        => commandType.Name
            .ReplaceCommandSuffix()
            .ToLowerSplitString(InstructionConstants.DefaultCommandNameSeparator);

    /// <summary>
    /// Removes <c>CliCommand</c> from a command type name.
    /// </summary>
    /// <param name="commandName">The command type name to strip.</param>
    /// <returns><paramref name="commandName"/> with every occurrence of <c>CliCommand</c> removed.</returns>
    public static string StripCommandName(string commandName)
        => commandName.ReplaceCommandSuffix();

    /// <summary>
    /// Derives the sub-command word a command type answers to, by removing <paramref name="primaryCommandType"/>'s
    /// own instruction name from the start of <paramref name="subCommandType"/>'s. For example,
    /// <c>HelloMateCliCommand</c> under <c>HelloCliCommand</c> gives <c>"mate"</c>. Passing the same type for
    /// both gives an empty string, meaning no sub-command word at all — the primary command itself.
    /// </summary>
    /// <param name="primaryCommandType">The command type whose instruction name the two are registered under.</param>
    /// <param name="subCommandType">The command type to derive a sub-command word for.</param>
    /// <returns>The sub-command word, or an empty string when <paramref name="subCommandType"/> is the primary command itself.</returns>
    public static string GetPrimaryInstructionName(Type primaryCommandType, Type subCommandType)
    {
        var keyName = GetInstructionName(primaryCommandType); // e.g. "hello" from HelloCliCommand
        var subName = GetInstructionName(subCommandType); // e.g. "hello-mate" from HelloMateCliCommand

        return subName[keyName.Length..].TrimStart('-');
    }
}