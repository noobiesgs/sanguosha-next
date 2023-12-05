using Injectio.Attributes;
using Microsoft.Extensions.DependencyInjection;
using Noobie.SanGuoSha.Lobby.MessageHandlers;
using Noobie.SanGuoSha.Network;

namespace Noobie.SanGuoSha.Lobby;

[RegisterTransient, AutoConstructor]
public partial class MessageHandlerFactory
{
    private readonly IServiceProvider _serviceProvider;

    public IMessageHandler GetMessageHandler(LobbyPacket packet)
    {
        var handler = _serviceProvider.GetRequiredKeyedService<IMessageHandler>(packet.GetType().Name);
        return handler;
    }
}