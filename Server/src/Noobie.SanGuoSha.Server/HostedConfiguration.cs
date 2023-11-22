using Injectio.Attributes;

namespace Noobie.SanGuoSha;

internal static class HostedConfiguration
{
    [RegisterServices]
    public static void Configure(IServiceCollection services)
    {
        services.AddHostedService<SanGuoShaServerHostedService>();
    }
}