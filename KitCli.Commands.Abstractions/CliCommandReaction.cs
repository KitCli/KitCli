using MediatR;

namespace KitCli.Commands.Abstractions;

public abstract record CliCommandReaction : INotification;