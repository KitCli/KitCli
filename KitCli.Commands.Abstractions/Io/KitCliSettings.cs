using KitCli.Commands.Abstractions.Outcomes;

namespace KitCli.Commands.Abstractions.Io;

/// <summary>
/// KitCli-owned settings, resolved the same way as <c>InstructionSettings</c>: bound from a
/// <c>KitCli</c> configuration section if one exists (via <c>WithJsonSettings</c>/
/// <c>WithUserSecretSettings</c>), falling back to the defaults below otherwise. No
/// <c>WithSettings{T}()</c> call is needed either way.
/// </summary>
public class KitCliSettings
{
    /// <summary>
    /// The lowest <see cref="MessageLevel"/> a leveled message outcome must have to be written.
    /// Defaults to <see cref="MessageLevel.Information"/>.
    /// </summary>
    public MessageLevel MinimumMessageLevel { get; set; } = MessageLevel.Information;
}
