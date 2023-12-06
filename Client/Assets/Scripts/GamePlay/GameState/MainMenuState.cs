#nullable enable

using Cysharp.Threading.Tasks;
using Noobie.SanGuoSha.GamePlay.UI;
using Noobie.SanGuoSha.Lobby;
using Noobie.SanGuoSha.Network;
using UnityEngine;
using ILogger = Microsoft.Extensions.Logging.ILogger;
using VContainer;

namespace Noobie.SanGuoSha.GamePlay.GameState
{
    internal class MainMenuState : GameStateBehaviour
    {
        [Inject] private LobbyServiceFacade _lobbyService;
        [Inject] private ILogger _logger;

        [SerializeField] private LoginUI _loginUI;

        public override GameState ActiveState => GameState.MainMenu;

        protected override void Awake()
        {
            base.Awake();
            _loginUI.LoginButtonClicked += LoginUIOnLoginButtonClicked;
            _lobbyService.ClientDisconnected += LobbyServiceOnClientDisconnected;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            _loginUI.LoginButtonClicked -= LoginUIOnLoginButtonClicked;
            _lobbyService.ClientDisconnected -= LobbyServiceOnClientDisconnected;
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


        }

        private async UniTask<bool> EnsureConnectedToServerAsync(string serverHost, int serverPort)
        {
            if (_lobbyService.Connection != null)
            {
                return true;
            }
            var success = await _lobbyService.ConnectAsync(serverHost, serverPort);
            if (success)
            {
                _lobbyService.Connection!.ReceivePacket += ConnectionOnReceivePacket;
                _logger.LogInformation("Connected to server");
                return true;
            }

            _logger.LogInformation("failed to connect to server");
            return false;
        }

        private void ConnectionOnReceivePacket(GameDataPacket packet)
        {

        }

        private void LobbyServiceOnClientDisconnected(SanGuoShaTcpClient connection)
        {
            connection.ReceivePacket -= ConnectionOnReceivePacket;
        }
    }
}
