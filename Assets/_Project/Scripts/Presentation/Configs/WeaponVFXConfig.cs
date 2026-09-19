using System;
using System.Collections.Generic;
using UnityEngine;

namespace _Project.Scripts.Presentation.Configs
{
    [CreateAssetMenu(fileName = "WeaponVfxConfig", menuName = "Presentation/Weapon Vfx Config")]
    public class WeaponVFXConfig : ScriptableObject
    {
        [Serializable]
        public struct VfxMapping
        {
            public string weaponName;
            public GameObject muzzleFlashPrefab;
            public GameObject tracerPrefab;
            public GameObject defaultImpactPrefab;
            public GameObject fleshImpactPrefab;
            public float tracerSpeed;
        }

        [SerializeField] private List<VfxMapping> mappings;

        private Dictionary<string, VfxMapping> _cachedMappings;

        private void EnsureCacheInitialized()
        {
            if (_cachedMappings != null) return;
            _cachedMappings = new Dictionary<string, VfxMapping>();

            foreach (var map in mappings)
            {
                if (!string.IsNullOrEmpty(map.weaponName))
                {
                    _cachedMappings[map.weaponName] = map;
                }
            }
        }

        public bool TryGetMapping(string weaponName, out VfxMapping mapping)
        {
            if (string.IsNullOrEmpty(weaponName))
            {
                mapping = default;
                return false;
            }

            EnsureCacheInitialized();
            return _cachedMappings.TryGetValue(weaponName, out mapping);
        }
    }
}