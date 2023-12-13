#nullable enable

using System;
using Cysharp.Threading.Tasks;
using Noobie.SanGuoSha.GamePlay.UI;
using Noobie.SanGuoSha.Lobby;
using Noobie.SanGuoSha.Network;
using UnityEngine;
using UnityEngine.SceneManagement;
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
        [Inject] private PopupManager _popupManager;

        [SerializeField] private LoginUI _loginUI;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        public override GameState ActiveState => GameState.MainMenu;

        protected override void Awake()
        {
            base.Awake();
            _loginUI.LoginButtonClicked += LoginUIOnLoginButtonClicked;
            _loginUI.Register += LoginUIOnRegister;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            _loginUI.LoginButtonClicked -= LoginUIOnLoginButtonClicked;
            _loginUI.Register -= LoginUIOnRegister;
            _logger.LogInformation("MainMenuState destroyed");
        }

        private void LoginUIOnLoginButtonClicked(string serverHost, int serverPort, string accountName, string password)
        {
            LoginAsync(serverHost, serverPort, accountName, password).Forget();
        }

        private void LoginUIOnRegister(string serverHost, int serverPort, string accountName, string nickname, string password)
        {
            RegisterAsync(serverHost, serverPort, accountName, nickname, password).Forget();
        }

        private async UniTaskVoid RegisterAsync(string serverHost, int serverPort, string accountName, string nickname, string password)
        {
            var success = await EnsureConnectedToServerAsync(serverHost, serverPort);
            if (!success)
            {
                return;
            }

            var response = await _lobbyService.RegisterAsync(accountName, nickname, password, destroyCancellationToken);

            switch (response.Status)
            {
                case RegistrationStatus.Success:
                    _loginUI.BackfillRegisterAccountName();
                    _loginUI.SaveAccountInfoToGameSettings();
                    _popupManager.ShowPopupPanel("RegistrationSuccess");
                    break;
                case RegistrationStatus.Invalid:
                    _popupManager.ShowPopupPanel("InvalidRegistrationInformation");
                    break;
                case RegistrationStatus.AccountAlreadyExists:
                    _popupManager.ShowPopupPanel("AccountAlreadyExists");
                    break;
                case RegistrationStatus.NicknameAlreadyExists:
                    _popupManager.ShowPopupPanel("NicknameAlreadyExists");
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private async UniTaskVoid LoginAsync(string serverHost, int serverPort, string accountName, string password)
        {
            var success = await EnsureConnectedToServerAsync(serverHost, serverPort);
            if (!success)
            {
                return;
            }

            var response = await _lobbyService.LoginAsync(accountName, password, destroyCancellationToken);

            switch (response.Status)
            {
                case LoginStatus.Success:
                    _user.Account.Update(response.Account);
                    _user.LoginToken = response.Token;
                    _loginUI.SaveAccountInfoToGameSettings();
                    SceneManager.LoadScene("Lobby");
                    break;
                case LoginStatus.OutdatedVersion:
                    _popupManager.ShowPopupPanel("ClientVersionOutdated");
                    break;
                case LoginStatus.InvalidUserNameAndPassword:
                    _popupManager.ShowPopupPanel("InvalidUserNameAndPassword");
                    break;
                case LoginStatus.UnknownFailure:
                    _popupManager.ShowPopupPanel("LoginUnknownFailure");
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
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
            _popupManager.ShowPopupPanel("FailedToConnectServer");
            return false;
        }
    }
}
