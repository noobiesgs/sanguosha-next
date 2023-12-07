#nullable enable

using System;
using Cysharp.Threading.Tasks;
using Noobie.SanGuoSha.GamePlay.UI;
using Noobie.SanGuoSha.Lobby;
using Noobie.SanGuoSha.LocalEventBus;
using Noobie.SanGuoSha.Network;
using UnityEngine;
using ILogger = Microsoft.Extensions.Logging.ILogger;
using VContainer;

namespace Noobie.SanGuoSha.GamePlay.GameState
{
    internal class MainMenuState : GameStateBehaviour
    {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        [Inject] private LobbyServiceFacade _lobbyService;
        [Inject] private ILogger _logger;
        [Inject] private LocalLobbyUser _user;
        [Inject] private ISubscriber<LobbyPacketReceivedMessage> _subscriber;
        [Inject] private PopupManager _popupManager;

        [SerializeField] private LoginUI _loginUI;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        private IDisposable? _subscription;

        public override GameState ActiveState => GameState.MainMenu;

        protected override void Awake()
        {
            base.Awake();
            _loginUI.LoginButtonClicked += LoginUIOnLoginButtonClicked;
            _subscription = _subscriber.Subscribe(OnReceivedLobbyPacket);
        }

        private void OnReceivedLobbyPacket(LobbyPacketReceivedMessage lobbyPacketMessage)
        {
            switch (lobbyPacketMessage.Packet)
            {
                case LoginResultPacket p:
                    HandleLoginResult(p);
                    break;
                default:
                    _logger.LogWarning("Unhandled packet: {0}", lobbyPacketMessage.Packet.GetType().Name);
                    break;
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            _loginUI.LoginButtonClicked -= LoginUIOnLoginButtonClicked;
            _subscription?.Dispose();
        }

        private void LoginUIOnLoginButtonClicked(string serverHost, int serverPort, string accountName, string password)
        {
            LoginAsync(serverHost, serverPort, accountName, password).Forget();
        }

        private async UniTaskVoid LoginAsync(string serverHost, int serverPort, string accountName, string password)
        {
            var success = await EnsureConnectedToServerAsync(serverHost, serverPort);
            if (!success)
            {
                return;
            }

            _lobbyService.Login(accountName, password);
        }

        private async UniTask<bool> EnsureConnectedToServerAsync(string serverHost, int serverPort)
        {
            if (_user.IsOnline)
            {
                return true;
            }
            var success = await _lobbyService.ConnectAsync(serverHost, serverPort);
            if (success)
            {
                _logger.LogInformation("Connected to server");
                return true;
            }

            _logger.LogInformation("failed to connect to server");
            return false;
        }

        private void HandleLoginResult(LoginResultPacket result)
        {
            switch (result.Status)
            {
                case LoginStatus.Success:
                    break;
                case LoginStatus.OutdatedVersion:
                    _popupManager.ShowPopupPanel("客户端版本与服务器不匹配，请更新");
                    break;
                case LoginStatus.InvalidUserNameAndPassword:
                    _popupManager.ShowPopupPanel("无效用户名或密码");
                    break;
                case LoginStatus.UnknownFailure:
                    _popupManager.ShowPopupPanel("登录失败，未知错误");
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
