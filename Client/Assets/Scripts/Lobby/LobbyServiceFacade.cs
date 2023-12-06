#nullable enable

using System;
using Cysharp.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Noobie.SanGuoSha.Network;
using VContainer.Unity;

namespace Noobie.SanGuoSha.Lobby
{
    public class LobbyServiceFacade : IDisposable, IStartable
    {
        private readonly LobbyHeartbeat _lobbyHeartbeat;
        private readonly PacketsSender _packetsSender;
        private readonly ILogger _logger;
        private readonly LocalLobbyUser _user;

        public LobbyServiceFacade(
            ILogger logger,
            LocalLobbyUser user,
            LobbyHeartbeat lobbyHeartbeat,
            PacketsSender packetsSender)
        {
            _logger = logger;
            _user = user;
            _lobbyHeartbeat = lobbyHeartbeat;
            _packetsSender = packetsSender;
        }

        public void Dispose()
        {
            if (_user.IsOnline)
            {
                _user.Connection?.Close("Lobby dispose");
            }

            _user.DisConnected -= UserOnDisConnected;
        }

        public void Start()
        {
            _user.DisConnected += UserOnDisConnected;
        }

        private void UserOnDisConnected(object sender, EventArgs e)
        {
            _lobbyHeartbeat.EndTracking();
            _packetsSender.EndSend();
        }

        public async UniTask<bool> ConnectAsync(string host, int port)
        {
            if (_user.IsOnline)
            {
                return true;
            }

            var client = new SanGuoShaTcpClient($"{host}:{port}");
            try
            {
                await client.ConnectAsync(15 * 1000);
                _lobbyHeartbeat.BeginTracking();
                _packetsSender.BeginSend();
                _user.Connection = client;
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                client.Dispose();
            }
            return false;
        }

        public void Disconnect()
        {
            if (_user.IsOnline)
            {
                var client = _user.Connection!;
                client.Close("Disconnect by lobby service");
                client.Dispose();
            }
        }
    }
}
