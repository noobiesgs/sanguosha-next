using System;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace Noobie.SanGuoSha.GamePlay.UI
{
    public class RegisterUI : MonoBehaviour
    {
        [SerializeField] private FullScreenPanelUI _parentPanel;
        [SerializeField] private InputField _inputAccountName;
        [SerializeField] private InputField _inputNickname;
        [SerializeField] private InputField _inputPassword;
        [SerializeField] private InputField _inputPasswordConform;

        [Inject] private PopupManager _popupManager;

        private readonly Regex _accountNameRegex = new (Misc.AccountNamePattern, RegexOptions.Compiled);
        private readonly Regex _nicknameRegex = new (Misc.NicknamePattern, RegexOptions.Compiled);
        private readonly Regex _passwordRegex = new (Misc.PasswordPattern, RegexOptions.Compiled);

        public event EventHandler Register;
        public string AccountName => _inputAccountName.text;
        public string Nickname => _inputNickname.text;
        public string Password => _inputPassword.text;

        public void OnRegisterButtonClick()
        {
            var accountName = _inputAccountName.text;
            var nickname = _inputNickname.text;
            var password = _inputPassword.text;
            var passwordConform = _inputPasswordConform.text;

            if (!_accountNameRegex.IsMatch(accountName))
            {
                _popupManager.ShowPopupPanel("用户名不符合规则");
                return;
            }
            if (!_nicknameRegex.IsMatch(nickname))
            {
                _popupManager.ShowPopupPanel("昵称不符合规则");
                return;
            }
            if (!_passwordRegex.IsMatch(password))
            {
                _popupManager.ShowPopupPanel("密码不符合规则");
                return;
            }
            if (password != passwordConform)
            {
                _popupManager.ShowPopupPanel("两次密码不相同");
                return;
            }

            _parentPanel.Hide();
            Register?.Invoke(this, EventArgs.Empty);
        }

        public void Show()
        {
            _inputAccountName.text = _inputNickname.text = _inputPassword.text = _inputPasswordConform.text = string.Empty;
            _parentPanel.Show();
        }
    }
}
