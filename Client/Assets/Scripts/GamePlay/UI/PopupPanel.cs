using UnityEngine;
using UnityEngine.UI;

namespace Noobie.SanGuoSha.GamePlay.UI
{
    public class PopupPanel : MonoBehaviour
    {
        [SerializeField] private Text _mainText;
        [SerializeField] private GameObject _confirmButton;
        [SerializeField] private GameObject _loadingSpinner;
        [SerializeField] private CanvasGroup _canvasGroup;

        private bool _isDisplaying;
        private bool _closableByUser;

        public bool IsDisplaying => _isDisplaying;

        private void Awake()
        {
            Hide();
        }

        public void OnConfirmClick()
        {
            if (_closableByUser)
            {
                Hide();
            }
        }

        public void SetupPopupPanel(string mainText, bool closableByUser = true)
        {
            _mainText.text = mainText;
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
