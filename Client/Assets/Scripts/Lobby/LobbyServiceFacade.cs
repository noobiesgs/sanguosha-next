#nullable enable

using System;
using Cysharp.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Noobie.SanGuoSha.Infrastructure;
using Noobie.SanGuoSha.Network;
using VContainer.Unity;

namespace Noobie.SanGuoSha.Lobby
{
    public delegate void ClientDisconnectedEventHandler(SanGuoShaTcpClient connection);

    public class LobbyServiceFacade : IDisposable, IStartable
    {
        private readonly LifetimeScope _parentScope;
        private readonly UpdateRunner _updateRunner;
        private readonly ILogger _logger;
        private SanGuoShaTcpClient? _connection;

        public LobbyServiceFacade(LifetimeScope parentScope, UpdateRunner updateRunner, ILogger logger)
        {
            _parentScope = parentScope;
            _updateRunner = updateRunner;
            _logger = logger;
        }

        private LifetimeScope? _serviceScope;

        public void Dispose()
        {
            _serviceScope?.Dispose();
        }

        public SanGuoShaTcpClient? Connection => _connection;

        public event ClientDisconnectedEventHandler? ClientDisconnected;

        public void Start()
        {
            _serviceScope = _parentScope.CreateChild();
        }

        public async UniTask<bool> ConnectAsync(string host, int port)
        {
            if (_connection != null)
            {
                return true;
            }

            var client = new SanGuoShaTcpClient($"{host}:{port}");
            try
            {
                await client.ConnectAsync(15 * 1000);
                client.Disconnected += ClientOnDisconnected;
                _connection = client;
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
            if (_connection != null)
            {
                var client = _connection;
                client.Close("Disconnect by lobby service");
                client.Dispose();
            }
        }

        private void ClientOnDisconnected()
        {
            if (_connection == null)
            {
                return;
            }
            var client = _connection;
            _connection.Disconnected -= ClientOnDisconnected;
            _connection = null;
            ClientDisconnected?.Invoke(client);
        }
    }
}
