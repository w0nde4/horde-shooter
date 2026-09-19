using System;
using _Project.Scripts.Domain.Gameplay;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _Project.Scripts.Presentation.UI
{
    [RequireComponent(typeof(Button))]
    public class UpgradeButtonView : MonoBehaviour
    {
        [SerializeField] private TMP_Text txtName;
        [SerializeField] private Image imgIcon; //for custom icon

        private Button _button;
        private UpgradeType _upgradeType;
        private Action<UpgradeType> _onClickCallback;

        private void Awake()
        {
            _button = GetComponent<Button>();
            _button.onClick.AddListener(OnButtonClicked);
        }

        public void Setup(UpgradeType type, Action<UpgradeType> onClick)
        {
            _upgradeType = type;
            _onClickCallback = onClick;

            if (txtName != null)
            {
                txtName.text = type.ToString();
            }

            if (imgIcon != null)
            {
                imgIcon.color = type switch
                {
                    UpgradeType.Damage => Color.red,
                    UpgradeType.GrenadeCooldown => Color.yellow,
                    UpgradeType.MaxHealth => Color.green,
                    UpgradeType.MoveSpeed => Color.blue,
                    _ => imgIcon.color
                };
            }
        }

        private void OnButtonClicked()
        {
            _onClickCallback?.Invoke(_upgradeType);
        }
    }
}