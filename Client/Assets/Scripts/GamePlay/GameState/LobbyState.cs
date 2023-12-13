using Microsoft.Extensions.Logging;
using Noobie.SanGuoSha.GamePlay.UI;
using Noobie.SanGuoSha.Infrastructure;
using Noobie.SanGuoSha.Lobby;
using Noobie.SanGuoSha.LocalEventBus;
using UnityEngine.SceneManagement;
using VContainer;

namespace Noobie.SanGuoSha.GamePlay.GameState
{
    public class LobbyState : GameStateBehaviour
    {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        [Inject] private LobbyServiceFacade _lobbyService;
        [Inject] private ILogger _logger;
        [Inject] private LocalLobbyUser _user;
        [Inject] private ISubscriber<ClientDisconnectedMessage> _clientDisconnectedSubscriber;
        [Inject] private PopupManager _popupManager;

#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        private readonly DisposableGroup _disposableGroup = new();

        public override GameState ActiveState => GameState.Lobby;

        protected override void Awake()
        {
            base.Awake();
            var subscription = _clientDisconnectedSubscriber.Subscribe(OnClientDisconnected);
            _disposableGroup.Add(subscription);
        }

        protected override void OnDestroy()
        {
            _disposableGroup.Dispose();
            base.OnDestroy();
        }

        private void OnClientDisconnected(ClientDisconnectedMessage obj)
        {
            _popupManager.ShowPopupPanel("网络连接中断");
            SceneManager.LoadScene("MainMenu");
        }
    }
}
