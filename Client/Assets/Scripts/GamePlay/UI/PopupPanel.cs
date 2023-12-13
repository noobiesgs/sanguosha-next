using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Tables;
using UnityEngine.UI;

namespace Noobie.SanGuoSha.GamePlay.UI
{
    public class PopupPanel : MonoBehaviour
    {
        [SerializeField] private Text _mainText;
        [SerializeField] private GameObject _confirmButton;
        [SerializeField] private GameObject _loadingSpinner;
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private LocalizedStringTable _localizedStringTable;

        private bool _isDisplaying;
        private bool _closableByUser;
        private StringTable _locale;

        public bool IsDisplaying => _isDisplaying;

        private void Awake()
        {
            _localizedStringTable.TableChanged += LocalizedStringTableOnTableChanged;
            Hide();
        }

        private void OnDestroy()
        {
            _localizedStringTable.TableChanged -= LocalizedStringTableOnTableChanged;
        }

        private void LocalizedStringTableOnTableChanged(StringTable value)
        {
            _locale = value;
        }

        public void OnConfirmClick()
        {
            if (_closableByUser)
            {
                Hide();
            }
        }

        public void SetupPopupPanel(string mainTextKey, bool closableByUser = true)
        {
            _mainText.text = _locale.L(mainTextKey);
            _closableByUser = closableByUser;
            _confirmButton.SetActive(_closableByUser);
            _loadingSpinner.SetActive(!_closableByUser);
            Show();
        }

        private void Show()
        {
            _canvasGroup.alpha = 1f;
            _canvasGroup.blocksRaycasts = true;
            _isDisplaying = true;
        }

        private void Hide()
        {
            _canvasGroup.alpha = 0f;
            _canvasGroup.blocksRaycasts = false;
            _isDisplaying = false;
        }
    }
}
