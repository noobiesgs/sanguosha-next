using Noobie.SanGuoSha.Database;
using Noobie.SanGuoSha.Games;
using Noobie.SanGuoSha.Lobby;
using Noobie.SanGuoSha.Network;

namespace Noobie.SanGuoSha;

[AutoConstructor]
public partial class SanGuoShaServerHostedService : IHostedService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<SanGuoShaServerHostedService> _logger;
    private readonly LobbyPacketReceivingThread _receivingThread;
    private readonly SanGuoShaTcpServer _server;
    private readonly SendingThread _sendingThread;
    private readonly DatabaseService _database;

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _database.Initialize();
        _server.Start();
        _logger.LogInformation("Web server for ui started, listening on: {endpoint}", _configuration["Kestrel:Endpoints:Http:Url"] ?? "http://localhost:5000");
        _sendingThread.Start();
        _receivingThread.Start();
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}