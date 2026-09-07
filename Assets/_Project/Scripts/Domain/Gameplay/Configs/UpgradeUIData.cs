using UnityEngine;

namespace _Project.Scripts.Domain.Gameplay.Configs
{
    [CreateAssetMenu(fileName = "UpgradeUIData_", menuName = "Upgrades/UI Data")]
    public class UpgradeUIData : ScriptableObject
    {
        public UpgradeType type;
        public string displayName;
        public string description;
        public Sprite icon;
        public Color color = Color.white;
    }
}