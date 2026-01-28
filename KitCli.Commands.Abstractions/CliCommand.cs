using KitCli.Commands.Abstractions.Extensions;
using KitCli.Commands.Abstractions.Outcomes;
using KitCli.Instructions.Abstractions;
using MediatR;

namespace KitCli.Commands.Abstractions;

/// <summary>
/// A command that can be executed via the CLI.
/// For example, "List all transactions for payee X".
/// </summary>
public record CliCommand : IRequest<CliCommandOutcome[]>
{
    public string GetSpecificCommandName()
        => GetType().Name.ReplaceCommandSuffix();

    public string GetInstructionName()
        => GetType().Name
            .ReplaceCommandSuffix()            
            .ToLowerSplitString(CliInstructionConstants.DefaultCommandNameSeparator);
    
    public static string StripCommandName(string commandName)
        => commandName.ReplaceCommandSuffix();
}