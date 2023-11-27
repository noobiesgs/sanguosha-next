using Injectio.Attributes;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TouchSocketSlim.Core;
using TouchSocketSlim.Sockets;

namespace Noobie.SanGuoSha.Network;

[RegisterSingleton]
public class SanGuoShaTcpServer : TcpService<SanGuoShaTcpClient>
{
    private readonly ILogger _logger;

    public SanGuoShaTcpServer(
        IOptions<ServerOptions> options,
        ILogger<SanGuoShaTcpServer> logger)
        : base(() => new FixedHeaderPackageAdapter(), $"{options.Value.ServerIp}:{options.Value.ServerPort}")
    {
        Received = OnClientReceived;
        Connected = OnClientConnected;
        Disconnected = OnClientDisconnected;
        _logger = logger;
    }

    private void OnClientDisconnected(SanGuoShaTcpClient client, DisconnectEventArgs e)
    {

    }

    private void OnClientConnected(SanGuoShaTcpClient client, ConnectedEventArgs e)
    {

    }

    private void OnClientReceived(SanGuoShaTcpClient client, byte[] buffer, int offset, int length)
    {

    }

    protected override SanGuoShaTcpClient CreateSocketClient()
    {
        return new SanGuoShaTcpClient();
    }

    public override void Start()
    {
        base.Start();
        _logger.LogInformation("Game server started, host on {endPoint}, state: {state}", Monitors.First().Option.IpHost.EndPoint, ServerState);
    }
}