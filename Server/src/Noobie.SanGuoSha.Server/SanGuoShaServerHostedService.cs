using Noobie.SanGuoSha.Games;
using Noobie.SanGuoSha.Network;

namespace Noobie.SanGuoSha;

[AutoConstructor]
public partial class SanGuoShaServerHostedService : IHostedService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<SanGuoShaServerHostedService> _logger;
    private readonly SanGuoShaTcpServer _server;
    private readonly Game _game;

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _server.Start();
        _logger.LogInformation("Web server for ui started, listening on: {endpoint}", _configuration["Kestrel:Endpoints:Http:Url"] ?? "http://localhost:5000");

        _game.InitTriggers();
        await _game.RunAsync();
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}