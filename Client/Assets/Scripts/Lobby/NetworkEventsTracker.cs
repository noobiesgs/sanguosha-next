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
        private readonly IPublisher<LobbyMessage> _lobbyMessagePublisher;
        private readonly LobbyRequestWrapper _lobbyRequestWrapper;
        private readonly IPublisher<ClientDisconnectedMessage> _clientDisconnectedMessagePublisher;

        private readonly LocalLobbyUser _user;

        public NetworkEventsTracker(
            UpdateRunner updateRunner,
            IPublisher<LobbyMessage> lobbyMessagePublisher,
            ILogger logger,
            LocalLobbyUser user,
            IPublisher<ClientDisconnectedMessage> clientDisconnectedMessagePublisher,
            LobbyRequestWrapper lobbyRequestWrapper)
        {
            _updateRunner = updateRunner;
            _lobbyMessagePublisher = lobbyMessagePublisher;
            _logger = logger;
            _user = user;
            _clientDisconnectedMessagePublisher = clientDisconnectedMessagePublisher;
            _lobbyRequestWrapper = lobbyRequestWrapper;
        }

        public void BeginTracking()
        {
            _updateRunner.Subscribe(OnUpdate, 1f / 40);
        }

        public void EndTracking()
        {
            _updateRunner.Unsubscribe(OnUpdate);
            _lobbyRequestWrapper.Clear();
        }

        private void OnUpdate(float _)
        {
            if (!_user.ReceivedPackets.IsEmpty)
            {
                while (_user.ReceivedPackets.TryDequeue(out var packet))
                {
                    switch (packet)
                    {
                        case ILobbyMessagePacket p:
                            _lobbyMessagePublisher.Publish(new LobbyMessage(p));
                            break;
                        case ILobbyResponsePacket p:
                            _lobbyRequestWrapper.Response(p);
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
