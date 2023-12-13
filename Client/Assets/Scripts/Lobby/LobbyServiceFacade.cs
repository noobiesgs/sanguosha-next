#nullable enable

using System;
using System.Runtime.CompilerServices;
using System.Threading;
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
        private readonly NetworkEventsTracker _networkEventsTracker;
        private readonly LobbyRequestWrapper _requestWrapper;
        private readonly ILogger _logger;
        private readonly LocalLobbyUser _user;
        private readonly ISubscriber<ClientDisconnectedMessage> _subscriber;

        private IDisposable? _subscriptions;
        private int _requestId;

        public LobbyServiceFacade(
            ILogger logger,
            LocalLobbyUser user,
            LobbyHeartbeat lobbyHeartbeat,
            PacketsSender packetsSender,
            ISubscriber<ClientDisconnectedMessage> subscriber,
            NetworkEventsTracker networkEventsTracker,
            LobbyRequestWrapper requestWrapper)
        {
            _logger = logger;
            _user = user;
            _lobbyHeartbeat = lobbyHeartbeat;
            _packetsSender = packetsSender;
            _subscriber = subscriber;
            _networkEventsTracker = networkEventsTracker;
            _requestWrapper = requestWrapper;
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
            _networkEventsTracker.EndTracking();
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
                _networkEventsTracker.BeginTracking();
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

        public UniTask<LoginResponsePacket> LoginAsync(string accountName, string password, CancellationToken cancellationToken)
        {
            return SendAsync<LoginResponsePacket>(requestId => new LoginRequestPacket(requestId, accountName, password, Misc.ProtocolVersion), cancellationToken);
        }

        public UniTask<RegisterResponsePacket> RegisterAsync(string accountName, string nickname, string password, CancellationToken cancellationToken)
        {
            return SendAsync<RegisterResponsePacket>(requestId => new RegisterRequestPacket(requestId, accountName, nickname, password), cancellationToken);
        }

        private async UniTask<TResponse> SendAsync<TResponse>(Func<int, IGameDataPacket> requestFactory, CancellationToken cancellationToken)
            where TResponse : ILobbyResponsePacket
        {
            if (!_user.IsOnline)
            {
                throw new InvalidOperationException("Client is offline");
            }

            var requestId = GenerateRequestId();
            var (utcs, linkedSource) = CreateTaskCompletionSource(requestId, cancellationToken);
            _user.SendAsync(requestFactory(requestId));

            try
            {
                var result = await EnsureResponse<TResponse>(utcs);
                return result;
            }
            finally
            {
                linkedSource.Dispose();
            }
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int GenerateRequestId()
        {
            return Interlocked.Increment(ref _requestId);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static async UniTask<T> EnsureResponse<T>(UniTaskCompletionSource<ILobbyResponsePacket> utcs) where T : ILobbyResponsePacket
        {
            var (canceled, response) = await utcs.Task.SuppressCancellationThrow();

            if (canceled)
            {
                throw new OperationCanceledException("Request timed out");
            }

            if (response is not T packet)
            {
                throw new Exception("Response type incorrect");
            }

            return packet;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private (UniTaskCompletionSource<ILobbyResponsePacket>, IDisposable) CreateTaskCompletionSource(int requestId, CancellationToken cancellationToken)
        {
            var utcs = new UniTaskCompletionSource<ILobbyResponsePacket>();
            var timeoutTokenSource = new CancellationTokenSource();
            timeoutTokenSource.CancelAfterSlim(GetRequestTimeout(), DelayType.Realtime);
            var linked = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutTokenSource.Token);
            _requestWrapper.Add(requestId, utcs, linked.Token);

            return (utcs, linked);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static TimeSpan GetRequestTimeout()
        {
            return TimeSpan.FromSeconds(5);
        }
    }
}
