using MediatR;

namespace KitCli.Commands.Abstractions;

// TODO: Can I add a reaction factory too?
public abstract record CliCommandReaction : INotification;