using Injectio.Attributes;
using MemoryPack.Formatters;
using MemoryPack;
using Microsoft.Extensions.DependencyInjection;
using Noobie.SanGuoSha.Actions;
using Noobie.SanGuoSha.Games;
using Microsoft.Extensions.Logging;

namespace Noobie.SanGuoSha;

internal class ServerConfiguration
{
    [RegisterServices]
    public static void Configure(IServiceCollection services)
    {
        MemoryPackFormatterProvider.Register(new MemoryPoolFormatter<byte>());

        services.AddTransient<GameActionScheduler>();
        services.AddTransient<Game>();
        services.AddTransient(p => p.GetRequiredService<ILoggerFactory>().CreateLogger("SanGuoSha"));
    }
}