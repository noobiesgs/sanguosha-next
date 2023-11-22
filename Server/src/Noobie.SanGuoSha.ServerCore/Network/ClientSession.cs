using Injectio.Attributes;
using Microsoft.Extensions.Logging;

namespace Noobie.SanGuoSha.Network;

[AutoConstructor, RegisterTransient]
public partial class ClientSession : TcpSession<ClientSession>
{
    private readonly ILogger<ClientSession> _logger;

    protected override void OnConnected()
    {
        _logger.LogDebug("Client connected, connection id: {connectionId}, endpoint: {endpoint}", Id, Socket.RemoteEndPoint);
    }
}