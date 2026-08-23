namespace KitCli.Instructions.Abstractions;

/// <summary>
/// Represents the parsed structure of a terminal input line: its prefix, command name,
/// optional sub-instruction name, and the arguments supplied with it.
/// </summary>
/// <param name="Prefix">The instruction prefix (e.g. "/"), or <see langword="null"/> if none was present.</param>
/// <param name="Name">The instruction name, or <see langword="null"/> if none was present.</param>
/// <param name="SubInstructionName">The sub-instruction name, or <see langword="null"/> if none was present.</param>
/// <param name="Arguments">The arguments supplied with the instruction.</param>
public record Instruction(
    string? Prefix,
    string? Name,
    string? SubInstructionName,
    List<AnonymousInstructionArgument> Arguments)
{
    /// <summary>
    /// An empty instruction with all string fields set to <see cref="string.Empty"/> and no arguments.
    /// </summary>
    public static Instruction Empty = new(string.Empty, string.Empty, string.Empty, []);
}