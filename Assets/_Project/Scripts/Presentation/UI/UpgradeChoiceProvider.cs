using System;
using System.Collections.Generic;
using _Project.Scripts.Domain.Gameplay;
using _Project.Scripts.Domain.Gameplay.Interfaces;
using _Project.Scripts.Presentation.UI.Configs;
using TMPro;
using UnityEngine;

namespace _Project.Scripts.Presentation.UI
{
    public class UpgradeChoiceProvider : MonoBehaviour, IUpgradeChoiceProvider
    {
        [Header("UI References")]
        [SerializeField] private GameObject panelRoot;
        [SerializeField] private Transform upgradeGroup;
        [SerializeField] private TMP_Text upgradeText;

        [Header("Prefabs")]
        [SerializeField] private UpgradeButtonView buttonPrefab;

        [Header("Visual Configuration")] 
        [SerializeField] private UpgradeVisualConfig visualConfig;

        private Action<UpgradeType> _onSelectedCallback;
        private readonly List<UpgradeButtonView> _buttonPool = new();

        private void Awake()
        {
            if (panelRoot != null) panelRoot.SetActive(false);
        }

        public void ShowUpgrades(List<UpgradeType> choices, Action<UpgradeType> onSelected)
        {
            _onSelectedCallback = onSelected;
            DeactivateAllButtons();

            if (upgradeText != null)
            {
                upgradeText.text = "SELECT YOUR UPGRADE";
            }

            for (var i = 0; i < choices.Count; i++)
            {
                var type = choices[i];
                UpgradeButtonView btnInstance;

                if (i < _buttonPool.Count)
                {
                    btnInstance = _buttonPool[i];
                }
                else
                {
                    btnInstance = Instantiate(buttonPrefab, upgradeGroup);
                    _buttonPool.Add(btnInstance);
                }

                btnInstance.gameObject.name = "UpgradeButton_" + type;
                var visualData = visualConfig.GetVisualData(type);
                
                btnInstance.Setup(visualData, HandleUpgradeSelected);
                btnInstance.gameObject.SetActive(true);
            }

            if (panelRoot != null)
            {
                panelRoot.SetActive(true);
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }

        private void HandleUpgradeSelected(UpgradeType type)
        {
            if (panelRoot != null)
            {
                panelRoot.SetActive(false);
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }

            _onSelectedCallback?.Invoke(type);
            DeactivateAllButtons();
        }

        private void DeactivateAllButtons()
        {
            foreach (var button in _buttonPool)
            {
                if (button != null)
                {
                    button.gameObject.SetActive(false);
                }
            }
        }

        private void OnDestroy()
        {
            _buttonPool.Clear();
            _onSelectedCallback = null;
        }
    }
}