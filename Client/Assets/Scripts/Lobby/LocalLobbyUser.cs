#nullable enable

using System.Collections.Concurrent;
using System.Collections.Generic;
using Noobie.SanGuoSha.LocalEventBus;
using Noobie.SanGuoSha.Network;

namespace Noobie.SanGuoSha.Lobby
{
    public class LocalLobbyUser
    {
        private SanGuoShaTcpClient? _connection;
        private readonly IPublisher<ClientDisconnectedMessage> _clientDisconnectedMessagePublisher;

        public bool IsOnline => Connection?.Online == true;

        internal Queue<IGameDataPacket> SendingPackets = new();
        internal ConcurrentQueue<IGameDataPacket> ReceivedPackets = new(new BlockingCollection<IGameDataPacket>());

        public LocalLobbyUser(IPublisher<ClientDisconnectedMessage> clientDisconnectedMessagePublisher)
        {
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

        private void ConnectionOnReceivePacket(IGameDataPacket packet)
        {
            ReceivedPackets.Enqueue(packet);
        }

        private void ConnectionOnDisconnected()
        {
            Connection = null;
            _clientDisconnectedMessagePublisher.Publish(new ClientDisconnectedMessage());
        }

        public void SendAsync(IGameDataPacket packet)
        {
            SendingPackets.Enqueue(packet);
        }
    }
}
