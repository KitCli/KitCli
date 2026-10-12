using KitCli.Abstractions.Io;

namespace KitCli.Commands.Abstractions.Io;

/// <summary>
/// Writing helpers that build on <see cref="ICliIo"/>.
/// </summary>
public static class CliIoExtensions
{
    extension(ICliIo cliIo)
    {
        /// <summary>
        /// Writes a blank line, then the command's instruction name indented two spaces, with its
        /// description indented four — so a suggestion is visually separated from whatever preceded it.
        /// </summary>
        /// <param name="name">The instruction name to suggest, including its prefix.</param>
        /// <param name="description">What the command does.</param>
        public void SayCommandSuggestion(string name, string description)
        {
            cliIo.Pause();

            cliIo.Say($"{SuggestionOutcomeIoWriter.NameSpacing}{name}");
            cliIo.Say($"{SuggestionOutcomeIoWriter.DescriptionSpacing}{description}");
        }
    }
}
