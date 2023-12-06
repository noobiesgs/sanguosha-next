#nullable enable

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Microsoft.IO;
using Noobie.SanGuoSha.Network;

namespace Noobie.SanGuoSha.Lobby
{
    public class LocalLobbyUser
    {
        private SanGuoShaTcpClient? _connection;

        public bool IsOnline => Connection?.Online == true;

        public event EventHandler? DisConnected;
        public event ReceivePacketEventHandler? ReceivePacket;

        internal ConcurrentQueue<GameDataPacket> Packets = new(new BlockingCollection<GameDataPacket>());

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
            ReceivePacket?.Invoke(packet);
        }

        private void ConnectionOnDisconnected()
        {
            Connection = null;
            DisConnected?.Invoke(this, EventArgs.Empty);
        }

        public void SendAsync(GameDataPacket packet)
        {
            Packets.Enqueue(packet);
        }
    }
}
