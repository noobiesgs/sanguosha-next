using Noobie.SanGuoSha.Network;

namespace Noobie.SanGuoSha.Lobby.MessageHandlers;

public interface IMessageHandler
{
    void Handle(SanGuoShaTcpClient connection, LobbyPacket packet);
}