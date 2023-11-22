using Injectio.Attributes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Noobie.SanGuoSha.Network;

[AutoConstructor, RegisterSingleton]
public partial class SanGuoShaServer : TcpServer<ClientSession>
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<SanGuoShaServer> _logger;

    protected override ClientSession CreateSession()
    {
        var session = _serviceProvider.GetRequiredService<ClientSession>();
        return session;
    }

    protected override void OnStarted()
    {
        _logger.LogInformation("SanGuoSha Game server started, listening on: {endpoint}", Endpoint);
    }
}