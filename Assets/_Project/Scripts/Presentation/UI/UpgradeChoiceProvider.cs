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
        private readonly List<UpgradeButtonView> _spawnedButtons = new();

        private void Awake()
        {
            if (panelRoot != null) panelRoot.SetActive(false);
        }

        public void ShowUpgrades(List<UpgradeType> choices, Action<UpgradeType> onSelected)
        {
            _onSelectedCallback = onSelected;
            ClearButtons();

            if (upgradeText != null)
            {
                upgradeText.text = "SELECT YOUR UPGRADE";
            }

            foreach (var type in choices)
            {
                var btnInstance = Instantiate(buttonPrefab, upgradeGroup);
                btnInstance.gameObject.name = "UpgradeButton_" + type;
                var visualData = visualConfig.GetVisualData(type);
                
                btnInstance.Setup(visualData, HandleUpgradeSelected);
                
                _spawnedButtons.Add(btnInstance);
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
            ClearButtons();
        }

        private void ClearButtons()
        {
            foreach (var btn in _spawnedButtons)
            {
                if (btn != null) Destroy(btn.gameObject);
            }
            _spawnedButtons.Clear();
        }
    }
}