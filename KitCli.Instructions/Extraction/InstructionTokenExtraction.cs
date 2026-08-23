namespace KitCli.Instructions.Extraction;

/// <summary>
/// The raw string tokens extracted from a terminal input, before they are parsed into an
/// <see cref="KitCli.Instructions.Abstractions.Instruction"/>.
/// </summary>
/// <param name="PrefixToken">The prefix token (e.g. <c>/</c>), or <see langword="null"/> if not present.</param>
/// <param name="NameToken">The command name token, or <see langword="null"/> if not present.</param>
/// <param name="SubNameToken">The sub-command name token, or <see langword="null"/> if not present.</param>
/// <param name="ArgumentTokens">The extracted argument name/value pairs.</param>
public record InstructionTokenExtraction(
    string? PrefixToken,
    string? NameToken,
    string? SubNameToken,
    Dictionary<string, string?> ArgumentTokens);