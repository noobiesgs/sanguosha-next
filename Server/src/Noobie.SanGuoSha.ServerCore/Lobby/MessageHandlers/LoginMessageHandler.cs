using Injectio.Attributes;
using Microsoft.Extensions.Logging;
using Noobie.SanGuoSha.Database;
using Noobie.SanGuoSha.Network;

namespace Noobie.SanGuoSha.Lobby.MessageHandlers;

[RegisterTransient<IMessageHandler>(Key = nameof(LoginRequestPacket))]
internal class LoginMessageHandler : MessageHandlerBase<LoginRequestPacket>
{
    public LoginMessageHandler(LobbyService lobbyService, ILogger<MessageHandlerBase<LoginRequestPacket>> logger) : base(lobbyService, logger)
    {
    }

    protected override void Handle(SanGuoShaTcpClient connection, LoginRequestPacket packet)
    {
        var result = LobbyService.Login(connection, packet.ProtocolVersion, packet.AccountName, packet.Password, out var account, out var loginToken);
        connection.SendAsync(result == LoginStatus.Success
            ? new LoginResponsePacket(packet.RequestId, result, ObjectMapper.Map<AccountPacket>(account!), loginToken)
            : new LoginResponsePacket(packet.RequestId, result));
    }

    protected override bool ConnectionAuthentication(SanGuoShaTcpClient connection)
    {
        return true;
    }
}