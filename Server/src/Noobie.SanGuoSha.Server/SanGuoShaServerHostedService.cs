namespace Noobie.SanGuoSha;

[AutoConstructor]
public partial class SanGuoShaServerHostedService : IHostedService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<SanGuoShaServerHostedService> _logger;

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Web server for ui started, listening on: {endpoint}", _configuration["Kestrel:Endpoints:Http:Url"] ?? "http://localhost:5000");
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}