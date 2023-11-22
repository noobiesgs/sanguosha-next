using Injectio.Attributes;

namespace Noobie.SanGuoSha;

internal static class HostedConfiguration
{
    [RegisterServices]
    public static void Configure(IServiceCollection services)
    {
        services.AddOptions<ServerOptions>().BindConfiguration(ServerOptions.Section);
        services.AddHostedService<SanGuoShaServerHostedService>();
    }
}