namespace KitCli.Instructions.Indexers;

/// <summary>
/// Identifies the kind of token located within a terminal input string.
/// </summary>
public enum InstructionTokenType
{
    /// <summary>The instruction prefix mark (e.g. <c>/</c>).</summary>
    Prefix,

    /// <summary>The command name.</summary>
    Name,

    /// <summary>The sub-command name.</summary>
    SubName,

    /// <summary>The argument name/value tokens.</summary>
    Arguments
}
