#nullable enable

using System.Collections.Concurrent;
using System.Collections.Generic;
using Noobie.SanGuoSha.Network;

namespace Noobie.SanGuoSha.Lobby
{
    public enum NetworkEvent
    {
        Disconnected
    }

    public class LocalLobbyUser
    {
        private SanGuoShaTcpClient? _connection;
        internal Queue<IGameDataPacket> SendingPackets = new();
        internal ConcurrentQueue<IGameDataPacket> ReceivedPackets = new(new BlockingCollection<IGameDataPacket>());
        internal ConcurrentQueue<NetworkEvent> NetworkEvents = new(new BlockingCollection<NetworkEvent>());

        public bool IsOnline => Connection?.Online == true;

        public Account Account { get; } = new();

        public LoginToken LoginToken { get; set; } = LoginToken.Empty;

        public SanGuoShaTcpClient? Connection
        {
            get => _connection;
            internal set
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
                else
                {
                    Account.Update(AccountPacket.Empty);
                    LoginToken = LoginToken.Empty;
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
            NetworkEvents.Enqueue(NetworkEvent.Disconnected);
        }

        public void SendAsync(IGameDataPacket packet)
        {
            SendingPackets.Enqueue(packet);
        }
    }
}
