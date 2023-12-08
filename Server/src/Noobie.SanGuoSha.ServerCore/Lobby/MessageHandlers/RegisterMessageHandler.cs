using Injectio.Attributes;
using Microsoft.Extensions.Logging;
using Noobie.SanGuoSha.Network;

namespace Noobie.SanGuoSha.Lobby.MessageHandlers;

[RegisterTransient<IMessageHandler>(Key = nameof(RegisterPacket))]
internal class RegisterMessageHandler : MessageHandlerBase<RegisterPacket>
{
    public RegisterMessageHandler(LobbyService lobbyService, ILogger<MessageHandlerBase<RegisterPacket>> logger) : base(lobbyService, logger)
    {
    }

    protected override void Handle(SanGuoShaTcpClient connection, RegisterPacket packet)
    {
        connection.SendAsync(new RegisterResultPacket(LobbyService.Register(packet.AccountName, packet.Nickname, packet.Password)));
    }

    protected override bool ConnectionAuthentication(SanGuoShaTcpClient connection)
    {
        return true;
    }
}