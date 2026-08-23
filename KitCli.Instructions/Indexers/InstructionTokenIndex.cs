namespace KitCli.Instructions.Indexers;

/// <summary>
/// The position of a single token within a terminal input string.
/// </summary>
public record InstructionTokenIndex
{
    /// <summary>
    /// Gets a value indicating whether the token was found in the terminal input.
    /// </summary>
    public bool Found { get; init; }

    /// <summary>
    /// Gets the index of the first character of the token within the terminal input.
    /// </summary>
    public int StartIndex { get; init; }

    /// <summary>
    /// Gets the index immediately after the last character of the token within the terminal input.
    /// </summary>
    public int EndIndex { get; init; }
}
