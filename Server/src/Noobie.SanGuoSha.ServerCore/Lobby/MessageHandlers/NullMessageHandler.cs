using Injectio.Attributes;
using Microsoft.Extensions.Logging;
using Noobie.SanGuoSha.Network;

namespace Noobie.SanGuoSha.Lobby.MessageHandlers;

[RegisterTransient<IMessageHandler>(Key = "*"), AutoConstructor]
internal partial class NullMessageHandler : IMessageHandler
{
    private readonly ILogger<NullMessageHandler> _logger;

    public void Handle(SanGuoShaTcpClient connection, LobbyPacket packet)
    {
        _logger.LogWarning("No handler specified for packet {packet}", packet.GetType().Name);
    }
}