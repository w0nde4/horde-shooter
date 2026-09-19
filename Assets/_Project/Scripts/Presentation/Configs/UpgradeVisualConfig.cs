using System;
using System.Collections.Generic;
using _Project.Scripts.Domain.Gameplay;
using UnityEngine;

namespace _Project.Scripts.Presentation.UI.Configs
{
    [CreateAssetMenu(fileName = "UpgradeVisualsConfig", menuName = "Presentation/Upgrade Visuals Config")]
    public class UpgradeVisualConfig : ScriptableObject
    {
        [Serializable]
        public struct UpgradeVisualData
        {
            public UpgradeType upgradeType;
            public string displayName;
            public Sprite icon;
            public Color iconColor;
        }

        [SerializeField] private List<UpgradeVisualData> visualSettings;

        private Dictionary<UpgradeType, UpgradeVisualData> _cachedVisuals;

        private void EnsureCacheInitialized()
        {
            if (_cachedVisuals != null) return;

            _cachedVisuals = new Dictionary<UpgradeType, UpgradeVisualData>();
            foreach (var setting in visualSettings)
            {
                _cachedVisuals[setting.upgradeType] = setting;
            }
        }

        public UpgradeVisualData GetVisualData(UpgradeType type)
        {
            EnsureCacheInitialized();

            if (_cachedVisuals.TryGetValue(type, out var data))
            {
                return data;
            }

            return new UpgradeVisualData
            {
                upgradeType = type,
                displayName = type.ToString(),
                icon = null,
                iconColor = Color.white
            };
        }
    }
}