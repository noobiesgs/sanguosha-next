using System;
using Microsoft.Extensions.Logging;
using Noobie.SanGuoSha.Infrastructure;
using Noobie.SanGuoSha.LocalEventBus;
using Noobie.SanGuoSha.Network;

namespace Noobie.SanGuoSha.Lobby
{
    public class NetworkEventsTracker
    {
        private readonly UpdateRunner _updateRunner;
        private readonly ILogger _logger;
        private readonly IPublisher<LobbyPacketReceivedMessage> _lobbyMessagePublisher;
        private readonly IPublisher<ClientDisconnectedMessage> _clientDisconnectedMessagePublisher;

        private readonly LocalLobbyUser _user;

        public NetworkEventsTracker(
            UpdateRunner updateRunner,
            IPublisher<LobbyPacketReceivedMessage> lobbyMessagePublisher,
            ILogger logger,
            LocalLobbyUser user,
            IPublisher<ClientDisconnectedMessage> clientDisconnectedMessagePublisher)
        {
            _updateRunner = updateRunner;
            _lobbyMessagePublisher = lobbyMessagePublisher;
            _logger = logger;
            _user = user;
            _clientDisconnectedMessagePublisher = clientDisconnectedMessagePublisher;
        }

        public void BeginTracking()
        {
            _updateRunner.Subscribe(OnUpdate, 1f / 40);
        }

        public void EndTracking()
        {
            _updateRunner.Unsubscribe(OnUpdate);
        }

        private void OnUpdate(float _)
        {
            if (!_user.ReceivedPackets.IsEmpty)
            {
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

            if (!_user.NetworkEvents.IsEmpty)
            {
                while (_user.NetworkEvents.TryDequeue(out var @event))
                {
                    switch (@event)
                    {
                        case NetworkEvent.Disconnected:
                            _clientDisconnectedMessagePublisher.Publish(new ClientDisconnectedMessage());
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
            }
        }
    }
}
