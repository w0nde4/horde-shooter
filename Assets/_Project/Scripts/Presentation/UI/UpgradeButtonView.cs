using System;
using _Project.Scripts.Domain.Gameplay;
using _Project.Scripts.Presentation.UI.Configs;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _Project.Scripts.Presentation.UI
{
    [RequireComponent(typeof(Button))]
    public class UpgradeButtonView : MonoBehaviour
    {
        [SerializeField] private TMP_Text txtName;
        [SerializeField] private Image imgIcon;

        private Button _button;
        private UpgradeType _upgradeType;
        private Action<UpgradeType> _onClickCallback;

        private void Awake()
        {
            _button = GetComponent<Button>();
            _button.onClick.AddListener(OnButtonClicked);
        }

        public void Setup(UpgradeVisualConfig.UpgradeVisualData visualData, Action<UpgradeType> onClick)
        {
            _upgradeType = visualData.upgradeType;
            _onClickCallback = onClick;

            if (txtName != null)
            {
                txtName.text = visualData.displayName;
            }

            if (imgIcon != null)
            {
                imgIcon.sprite = visualData.icon;
                imgIcon.color = visualData.iconColor;
            }
        }

        private void OnButtonClicked()
        {
            _onClickCallback?.Invoke(_upgradeType);
        }
    }
}