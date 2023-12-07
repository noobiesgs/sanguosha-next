using System;
using System.Collections.Generic;
using UnityEngine;

namespace Noobie.SanGuoSha.GamePlay.UI
{
    public class PopupManager : MonoBehaviour
    {
        [SerializeField] private GameObject _popupPanelPrefab;
        [SerializeField] private GameObject _canvas;

        private readonly List<PopupPanel> _popupPanels = new();

        private const float Offset = 30;
        private const float MaxOffset = 200;

        private void Awake()
        {
            DontDestroyOnLoad(_canvas);
        }

        public PopupPanel ShowPopupPanel(string mainText, bool closableByUser = true)
        {
            var popup = GetNextAvailablePopupPanel();
            if (popup != null)
            {
                popup.SetupPopupPanel(mainText, closableByUser);
            }

            return popup;
        }

        PopupPanel GetNextAvailablePopupPanel()
        {
            int nextAvailablePopupIndex = 0;
            for (int i = 0; i < _popupPanels.Count; i++)
            {
                if (_popupPanels[i].IsDisplaying)
                {
                    nextAvailablePopupIndex = i + 1;
                }
            }

            if (nextAvailablePopupIndex < _popupPanels.Count)
            {
                return _popupPanels[nextAvailablePopupIndex];
            }

            // None of the current PopupPanels are available, so instantiate a new one

            var popupGameObject = Instantiate(_popupPanelPrefab, gameObject.transform);
            popupGameObject.transform.position += new Vector3(1, -1) * (Offset * _popupPanels.Count % MaxOffset);
            var popupPanel = popupGameObject.GetComponent<PopupPanel>();
            if (popupPanel != null)
            {
                _popupPanels.Add(popupPanel);
            }
            else
            {
                Debug.LogError("PopupPanel prefab does not have a PopupPanel component!");
            }

            return popupPanel;
        }
    }
}
