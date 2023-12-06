using Noobie.SanGuoSha.Settings;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace Noobie.SanGuoSha.GamePlay.UI
{
    public delegate void LoginButtonClickEventHandler(string serverHost, int serverPort, string accountName, string password);

    public class LoginUI : MonoBehaviour
    {
        [SerializeField] private Button _loginButton;
        [SerializeField] private Button _registerButton;
        [SerializeField] private TMP_InputField _serverHost;
        [SerializeField] private TMP_InputField _serverPort;
        [SerializeField] private TMP_InputField _accountName;
        [SerializeField] private TMP_InputField _password;

        [Inject] GameSettingsManager _gameSettingsManager;

        private GameSettings _gameSettings;

        private void Awake()
        {
            _gameSettings = _gameSettingsManager.Settings;
            if (!string.IsNullOrEmpty(_gameSettings.Server.AccountName))
            {
                _accountName.text = _gameSettings.Server.AccountName;
            }
            if (!string.IsNullOrEmpty(_gameSettings.Server.Ip))
            {
                _serverHost.text = _gameSettings.Server.Ip;
            }
            _serverPort.text = _gameSettings.Server.Port.ToString();

            _loginButton.onClick.AddListener(() =>
            {
                LoginButtonClicked?.Invoke(_serverHost.text, int.Parse(_serverPort.text), _accountName.text, _password.text);
            });
        }

        private void OnDestroy()
        {
            _loginButton.onClick.RemoveAllListeners();
        }

        public void SaveSettings()
        {
            _gameSettings.Server.AccountName = _accountName.text;
            _gameSettings.Server.Ip = _serverHost.text;
            _gameSettings.Server.Port = int.Parse(_serverPort.text);
            _gameSettingsManager.Save();
        }

        public event LoginButtonClickEventHandler LoginButtonClicked;
    }
}
