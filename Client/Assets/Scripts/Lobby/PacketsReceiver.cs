using Microsoft.Extensions.Logging;
using Noobie.SanGuoSha.Infrastructure;
using Noobie.SanGuoSha.LocalEventBus;
using Noobie.SanGuoSha.Network;

namespace Noobie.SanGuoSha.Lobby
{
    public class PacketsReceiver
    {
        private readonly UpdateRunner _updateRunner;
        private readonly ILogger _logger;
        private readonly IPublisher<LobbyPacketReceivedMessage> _lobbyMessagePublisher;
        private readonly LocalLobbyUser _user;

        public PacketsReceiver(
            UpdateRunner updateRunner,
            IPublisher<LobbyPacketReceivedMessage> lobbyMessagePublisher,
            ILogger logger,
            LocalLobbyUser user)
        {
            _updateRunner = updateRunner;
            _lobbyMessagePublisher = lobbyMessagePublisher;
            _logger = logger;
            _user = user;
        }

        public void BeginReceive()
        {
            _updateRunner.Subscribe(OnUpdate, 1f / 40);
        }

        public void EndReceive()
        {
            _updateRunner.Unsubscribe(OnUpdate);
        }

        private void OnUpdate(float _)
        {
            if (_user.ReceivedPackets.IsEmpty)
            {
                return;
            }

            while (_user.ReceivedPackets.TryDequeue(out var packet))
            {
                switch (packet)
                {
                    case ILobbyPacket p:
                        _lobbyMessagePublisher.Publish(new LobbyPacketReceivedMessage(p));
                        break;
                    default:
                        _logger.LogWarning("Unhandled packet, type: {0}", packet.GetType().Name);
                        break;
                }
            }
        }
    }
}
