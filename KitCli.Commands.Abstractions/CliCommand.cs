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
    /// Derives the dashed instruction name this command responds to, by stripping the <c>Command</c> suffix
    /// off the type name and inserting a separator before every uppercase letter (except the first).
    /// For example, <c>SpareMoneyCommand</c> becomes <c>spare-money</c>.
    /// </summary>
    /// <returns>The instruction name derived from this command's type.</returns>
    public string GetInstructionName()
        => GetType().Name
            .ReplaceCommandSuffix()
            .ToLowerSplitString(InstructionConstants.DefaultCommandNameSeparator);

    /// <summary>
    /// Strips the <c>Command</c> suffix off a command type name.
    /// </summary>
    /// <param name="commandName">The command type name to strip.</param>
    /// <returns><paramref name="commandName"/> with any trailing <c>Command</c> suffix removed.</returns>
    public static string StripCommandName(string commandName)
        => commandName.ReplaceCommandSuffix();
}