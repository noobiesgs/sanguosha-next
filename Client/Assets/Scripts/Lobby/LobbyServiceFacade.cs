#nullable enable

using System;
using Cysharp.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Noobie.SanGuoSha.LocalEventBus;
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
        private readonly ISubscriber<ClientDisconnectedMessage> _subscriber;

        private IDisposable? _subscriptions;

        public LobbyServiceFacade(
            ILogger logger,
            LocalLobbyUser user,
            LobbyHeartbeat lobbyHeartbeat,
            PacketsSender packetsSender,
            ISubscriber<ClientDisconnectedMessage> subscriber
            )
        {
            _logger = logger;
            _user = user;
            _lobbyHeartbeat = lobbyHeartbeat;
            _packetsSender = packetsSender;
            _subscriber = subscriber;
        }

        public void Dispose()
        {
            if (_user.IsOnline)
            {
                _user.Connection?.Close("Lobby dispose");
            }

            _subscriptions?.Dispose();
        }

        public void Start()
        {
            _subscriptions = _subscriber.Subscribe(UserOnDisConnected);
        }

        private void UserOnDisConnected(ClientDisconnectedMessage _)
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
                _logger.LogError(ex);
                client.Dispose();
            }
            return false;
        }

        public void Login(string accountName, string password)
        {
            if (!_user.IsOnline)
            {
                _logger.LogWarning("Client is offline.");
                return;
            }

            _user.SendAsync(new LoginPacket(accountName, password, Misc.ProtocolVersion));
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
