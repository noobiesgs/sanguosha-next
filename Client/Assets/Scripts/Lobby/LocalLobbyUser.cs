#nullable enable

using System.Collections.Generic;
using Noobie.SanGuoSha.LocalEventBus;
using Noobie.SanGuoSha.Network;

namespace Noobie.SanGuoSha.Lobby
{
    public class LocalLobbyUser
    {
        private SanGuoShaTcpClient? _connection;
        private readonly IPublisher<LobbyPacketReceiveMessage> _lobbyMessagePublisher;
        private readonly IPublisher<ClientDisconnectedMessage> _clientDisconnectedMessagePublisher;

        public bool IsOnline => Connection?.Online == true;

        internal Queue<GameDataPacket> Packets = new();

        public LocalLobbyUser(
            IPublisher<LobbyPacketReceiveMessage> lobbyMessagePublisher,
            IPublisher<ClientDisconnectedMessage> clientDisconnectedMessagePublisher
            )
        {
            _lobbyMessagePublisher = lobbyMessagePublisher;
            _clientDisconnectedMessagePublisher = clientDisconnectedMessagePublisher;
        }

        public SanGuoShaTcpClient? Connection
        {
            get => _connection;
            set
            {
                if (_connection == value) return;
                if (_connection != null)
                {
                    _connection.Disconnected -= ConnectionOnDisconnected;
                    _connection.ReceivePacket -= ConnectionOnReceivePacket;
                }
                _connection = value;
                if (_connection != null)
                {
                    _connection.Disconnected += ConnectionOnDisconnected;
                    _connection.ReceivePacket += ConnectionOnReceivePacket;
                }
            }
        }

        private void ConnectionOnReceivePacket(GameDataPacket packet)
        {
            if (packet is LobbyPacket lobbyPacket)
            {
                _lobbyMessagePublisher.Publish(new LobbyPacketReceiveMessage(lobbyPacket));
            }
        }

        private void ConnectionOnDisconnected()
        {
            Connection = null;
            _clientDisconnectedMessagePublisher.Publish(new ClientDisconnectedMessage());
        }

        public void SendAsync(GameDataPacket packet)
        {
            Packets.Enqueue(packet);
        }
    }
}
