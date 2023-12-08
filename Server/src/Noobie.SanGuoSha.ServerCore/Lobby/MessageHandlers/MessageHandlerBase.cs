using Microsoft.Extensions.Logging;
using Noobie.SanGuoSha.Network;

namespace Noobie.SanGuoSha.Lobby.MessageHandlers;

internal abstract class MessageHandlerBase<TPacket> : IMessageHandler where TPacket : ILobbyPacket
{
    protected LobbyService LobbyService { get; }
    protected ILogger<MessageHandlerBase<TPacket>> Logger { get; }

    protected MessageHandlerBase(LobbyService lobbyService, ILogger<MessageHandlerBase<TPacket>> logger)
    {
        LobbyService = lobbyService;
        Logger = logger;
    }

    public void Handle(SanGuoShaTcpClient connection, ILobbyPacket packet)
    {
        if (!ConnectionAuthentication(connection))
        {
            Logger.LogWarning("Connection not authenticated, connection id: {id}, handler: {type}", connection.Id, packet.GetType().Name);
            connection.Close("Connection not authenticated");
            connection.Dispose();
            return;
        }
        Handle(connection, (TPacket)packet);
    }

    protected abstract void Handle(SanGuoShaTcpClient connection, TPacket packet);

    protected virtual bool ConnectionAuthentication(SanGuoShaTcpClient connection)
    {
        return LobbyService.IsLoggedId(connection.Id);
    }
}