using Injectio.Attributes;
using Microsoft.Extensions.Logging;
using Noobie.SanGuoSha.Network;

namespace Noobie.SanGuoSha.Lobby.MessageHandlers;

[RegisterTransient<IMessageHandler>(Key = nameof(RegisterRequestPacket))]
internal class RegisterMessageHandler : MessageHandlerBase<RegisterRequestPacket>
{
    public RegisterMessageHandler(LobbyService lobbyService, ILogger<MessageHandlerBase<RegisterRequestPacket>> logger) : base(lobbyService, logger)
    {
    }

    protected override void Handle(SanGuoShaTcpClient connection, RegisterRequestPacket packet)
    {
        connection.SendAsync(new RegisterResponsePacket(packet.RequestId, LobbyService.Register(packet.AccountName, packet.Nickname, packet.Password)));
    }

    protected override bool ConnectionAuthentication(SanGuoShaTcpClient connection)
    {
        return true;
    }
}