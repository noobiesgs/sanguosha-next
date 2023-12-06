using Injectio.Attributes;
using Microsoft.Extensions.Logging;
using Noobie.SanGuoSha.Network;

namespace Noobie.SanGuoSha.Lobby.MessageHandlers;

[RegisterTransient<IMessageHandler>(Key = nameof(PingPacket))]
internal class PingMessageHandler : MessageHandlerBase<PingPacket>
{
    public PingMessageHandler(LobbyService lobbyService, ILogger<MessageHandlerBase<PingPacket>> logger) : base(lobbyService, logger)
    {
    }

    protected override void Handle(SanGuoShaTcpClient connection, PingPacket packet)
    {
        Logger.LogDebug("Client ping! Id: {id}", connection.Id);
    }

    protected override bool ConnectionAuthentication(SanGuoShaTcpClient connection)
    {
        return true;
    }
}

