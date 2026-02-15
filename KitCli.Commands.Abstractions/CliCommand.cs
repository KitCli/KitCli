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

    public string GetInstructionName()
        => GetType().Name
            .ReplaceCommandSuffix()            
            .ToLowerSplitString(InstructionConstants.DefaultCommandNameSeparator);
    
    public static string StripCommandName(string commandName)
        => commandName.ReplaceCommandSuffix();
}
