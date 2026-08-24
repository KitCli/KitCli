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
        => GetType().Name
            .ReplaceCommandSuffix()
            .ToLowerSplitString(InstructionConstants.DefaultCommandNameSeparator);

    /// <summary>
    /// Removes <c>CliCommand</c> from a command type name.
    /// </summary>
    /// <param name="commandName">The command type name to strip.</param>
    /// <returns><paramref name="commandName"/> with every occurrence of <c>CliCommand</c> removed.</returns>
    public static string StripCommandName(string commandName)
        => commandName.ReplaceCommandSuffix();
}