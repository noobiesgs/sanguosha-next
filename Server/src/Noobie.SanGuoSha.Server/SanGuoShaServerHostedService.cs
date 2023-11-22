using Microsoft.Extensions.Options;
using Noobie.SanGuoSha.Network;
using System.Net;

namespace Noobie.SanGuoSha;

[AutoConstructor]
public partial class SanGuoShaServerHostedService : IHostedService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<SanGuoShaServerHostedService> _logger;
    private readonly SanGuoShaServer _server;

    [AutoConstructorInject("options.Value", "options", typeof(IOptions<ServerOptions>))]
    private readonly ServerOptions _serverOptions;

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _server.Start(new IPEndPoint(IPAddress.Parse(_serverOptions.ServerIp), _serverOptions.ServerPort));
        _logger.LogInformation("Web server for ui started, listening on: {endpoint}", _configuration["Kestrel:Endpoints:Http:Url"] ?? "http://localhost:5000");
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}