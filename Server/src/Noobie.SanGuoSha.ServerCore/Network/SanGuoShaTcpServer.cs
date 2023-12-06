using Injectio.Attributes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Noobie.SanGuoSha.Lobby;
using TouchSocketSlim.Core;
using TouchSocketSlim.Sockets;

namespace Noobie.SanGuoSha.Network;

[RegisterSingleton]
public class SanGuoShaTcpServer : TcpService<SanGuoShaTcpClient>
{
    private readonly LobbyService _lobbyService;
    private readonly LobbyPacketReceivingThread _lobbyPacketReceivingThread;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger _logger;

    public SanGuoShaTcpServer(
        IOptions<ServerOptions> options,
        LobbyService lobbyService,
        LobbyPacketReceivingThread lobbyPacketReceivingThread,
        IServiceProvider serviceProvider,
        ILogger<SanGuoShaTcpServer> logger)
        : base(() => new FixedHeaderPackageAdapter(), $"{options.Value.ServerIp}:{options.Value.ServerPort}")
    {
        _lobbyPacketReceivingThread = lobbyPacketReceivingThread;
        Received = OnClientReceived;
        Connected = OnClientConnected;
        Disconnected = OnClientDisconnected;
        _lobbyService = lobbyService;
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    private void OnClientDisconnected(SanGuoShaTcpClient client, DisconnectEventArgs e)
    {
        _lobbyService.ClientDisconnected(client);
        _logger.LogInformation("Client disconnected: {id}-{ip}", client.Id, client.Ip);
    }

    private void OnClientConnected(SanGuoShaTcpClient client, ConnectedEventArgs e)
    {
        _logger.LogInformation("Client connected: {id}-{ip}", client.Id, client.Ip);
    }

    private void OnClientReceived(SanGuoShaTcpClient client, byte[] buffer, int offset, int length)
    {
        using var memory = new MemoryStream(buffer, offset, length, false);

        var packets = StreamingSerializer.Deserialize<GameDataPacket>(memory);

        foreach (var packet in packets)
        {
            if (packet is LobbyPacket lobbyPacket)
            {
                _lobbyPacketReceivingThread.Received(client, lobbyPacket);
            }
            else
            {
                //TODO:
                client.Receive(packet);
            }
        }
    }

    protected override SanGuoShaTcpClient CreateSocketClient()
    {
        return _serviceProvider.GetRequiredService<SanGuoShaTcpClient>();
    }

    public override void Start()
    {
        base.Start();
        _logger.LogInformation("Game server started, host on {endPoint}, state: {state}", Monitors.First().Option.IpHost.EndPoint, ServerState);
    }
}