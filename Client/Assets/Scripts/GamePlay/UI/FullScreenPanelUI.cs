using Cysharp.Threading.Tasks;
using System.Threading;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Noobie.SanGuoSha.GamePlay.UI
{
    internal class FullScreenPanelUI : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField]
        private CanvasGroup _rootCanvasGroup;

        [SerializeField]
        private Transform _containerTransform;

        [SerializeField]
        private CanvasGroup _containerCanvasGroup;

        private bool _inAnimation;

        private CancellationToken _cancellationToken;

        private void Awake()
        {
            _cancellationToken = this.GetCancellationTokenOnDestroy();
        }

        public void Show()
        {

            if (_inAnimation)
            {
                return;
            }

            _inAnimation = true;
            _rootCanvasGroup.alpha = 1;
            _rootCanvasGroup.blocksRaycasts = true;
            _containerCanvasGroup.DOFade(0f, 0.3f).From();
            _containerTransform.DOLocalMoveY(-100f, 0.3f).From().OnComplete(() =>
            {
                _inAnimation = false;
            });
        }

        public async void Hide()
        {
            if (_inAnimation)
            {
                return;
            }
            _inAnimation = true;
            _containerCanvasGroup.DOFade(0f, 0.3f).AwaitForComplete(cancellationToken: _cancellationToken).SuppressCancellationThrow().Forget();
            var canceled = await _containerTransform.DOLocalMoveY(-100f, 0.3f).AwaitForComplete(cancellationToken: _cancellationToken).SuppressCancellationThrow();
            if (!canceled)
            {
                _rootCanvasGroup.alpha = 0;
                _rootCanvasGroup.blocksRaycasts = false;
                _containerCanvasGroup.alpha = 1;
                _containerTransform.localPosition = new Vector3(_containerTransform.localPosition.x, 0);
                _inAnimation = false;
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            Hide();
        }
    }
}
