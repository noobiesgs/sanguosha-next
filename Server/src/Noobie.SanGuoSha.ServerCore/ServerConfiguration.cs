using Injectio.Attributes;
using MemoryPack.Formatters;
using MemoryPack;
using Microsoft.Extensions.DependencyInjection;

namespace Noobie.SanGuoSha;

internal class ServerConfiguration
{
    [RegisterServices]
    public static void Configure(IServiceCollection services)
    {
        MemoryPackFormatterProvider.Register(new MemoryPoolFormatter<byte>());
    }
}