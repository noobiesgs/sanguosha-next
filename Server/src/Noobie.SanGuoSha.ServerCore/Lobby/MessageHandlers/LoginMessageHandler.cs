using Injectio.Attributes;
using Microsoft.Extensions.Logging;
using Noobie.SanGuoSha.Database;
using Noobie.SanGuoSha.Network;

namespace Noobie.SanGuoSha.Lobby.MessageHandlers;

[RegisterTransient<IMessageHandler>(Key = nameof(LoginPacket))]
internal class LoginMessageHandler : MessageHandlerBase<LoginPacket>
{
    public LoginMessageHandler(LobbyService lobbyService, ILogger<MessageHandlerBase<LoginPacket>> logger) : base(lobbyService, logger)
    {
    }

    protected override void Handle(SanGuoShaTcpClient connection, LoginPacket packet)
    {
        var result = LobbyService.Login(connection, packet.ProtocolVersion, packet.AccountName, packet.Password, out var account, out var loginToken);
        connection.SendAsync(result == LoginStatus.Success ? new LoginResultPacket(result, ObjectMapper.Map<AccountPacket>(account!), loginToken) : new LoginResultPacket(result));
    }

    protected override bool ConnectionAuthentication(SanGuoShaTcpClient connection)
    {
        return true;
    }
}