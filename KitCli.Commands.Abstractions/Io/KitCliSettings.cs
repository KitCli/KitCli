using KitCli.Commands.Abstractions.Outcomes;

namespace KitCli.Commands.Abstractions.Io;

/// <summary>
/// KitCli-owned settings a consumer can configure through <c>CliAppBuilder</c>'s <c>With...</c> methods,
/// without having to opt in via <c>WithSettings{T}()</c>.
/// </summary>
public class KitCliSettings
{
    /// <summary>
    /// The lowest <see cref="MessageLevel"/> a leveled message outcome must have to be written.
    /// Set via <c>CliAppBuilder.WithMinimumMessageLevel</c>; defaults to <see cref="MessageLevel.Information"/>.
    /// </summary>
    public MessageLevel MinimumMessageLevel { get; set; } = MessageLevel.Information;
}
