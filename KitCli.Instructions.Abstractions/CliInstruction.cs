namespace KitCli.Instructions.Abstractions;

public record CliInstruction(
    string? Prefix,
    string? Name,
    string? SubInstructionName,
    List<CliInstructionArgument> Arguments);