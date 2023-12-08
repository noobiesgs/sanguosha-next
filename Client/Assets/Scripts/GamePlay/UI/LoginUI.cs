using System;
using Noobie.SanGuoSha.Settings;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace Noobie.SanGuoSha.GamePlay.UI
{
    public delegate void LoginButtonClickEventHandler(string serverHost, int serverPort, string accountName, string password);
    public delegate void RegisterEventHandler(string serverHost, int serverPort, string accountName, string nickname, string password);

    public class LoginUI : MonoBehaviour
    {
        [SerializeField] private Button _loginButton;
        [SerializeField] private Button _registerButton;
        [SerializeField] private TMP_InputField _serverHost;
        [SerializeField] private TMP_InputField _serverPort;
        [SerializeField] private TMP_InputField _accountName;
        [SerializeField] private TMP_InputField _password;
        [SerializeField] private RegisterUI _registerUI;

        [Inject] GameSettingsManager _gameSettingsManager;
        [Inject] private PopupManager _popupManager;

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
                if (string.IsNullOrEmpty(_serverPort.text) || string.IsNullOrEmpty(_serverHost.text))
                {
                    _popupManager.ShowPopupPanel("请输入服务器ip以及端口");
                    return;
                }
                LoginButtonClicked?.Invoke(_serverHost.text, int.Parse(_serverPort.text), _accountName.text, _password.text);
            });

            _registerButton.onClick.AddListener(() =>
            {
                if (string.IsNullOrEmpty(_serverPort.text) || string.IsNullOrEmpty(_serverHost.text))
                {
                    _popupManager.ShowPopupPanel("请输入服务器ip以及端口");
                    return;
                }
                _registerUI.Show();
            });

            _registerUI.Register += RegisterUIOnRegister;
        }

        private void RegisterUIOnRegister(object sender, EventArgs e)
        {
            Register?.Invoke(_serverHost.text, int.Parse(_serverPort.text), _registerUI.AccountName, _registerUI.Nickname, _registerUI.Password);
        }

        private void OnDestroy()
        {
            _loginButton.onClick.RemoveAllListeners();
            _registerButton.onClick.RemoveAllListeners();
            _registerUI.Register -= RegisterUIOnRegister;
        }

        public void BackfillRegisterAccountName()
        {
            _accountName.text = _registerUI.AccountName;
        }

        public void SaveAccountInfoToGameSettings()
        {
            _gameSettings.Server.AccountName = _accountName.text;
            _gameSettings.Server.Ip = _serverHost.text;
            _gameSettings.Server.Port = int.Parse(_serverPort.text);
            _gameSettingsManager.Save();
        }

        public event LoginButtonClickEventHandler LoginButtonClicked;
        public event RegisterEventHandler Register;
    }
}
