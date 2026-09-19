using System;
using System.Collections.Generic;
using UnityEngine;

namespace _Project.Scripts.Presentation.Configs
{
    [CreateAssetMenu(fileName = "WeaponAudioConfig", menuName = "Presentation/Weapon Audio Config")]
    public class WeaponAudioConfig : ScriptableObject
    {
        [Serializable]
        public struct AudioMapping
        {
            public string weaponName;
            public AudioClip shotSound;
        }

        [SerializeField] private List<AudioMapping> mappings;

        private Dictionary<string, AudioClip> _cachedMappings;

        public AudioClip GetShotSound(string weaponName)
        {
            if (_cachedMappings == null)
            {
                _cachedMappings = new Dictionary<string, AudioClip>();
                foreach (var map in mappings)
                {
                    if (!string.IsNullOrEmpty(map.weaponName) && map.shotSound != null)
                    {
                        _cachedMappings[map.weaponName] = map.shotSound;
                    }
                }
            }

            return _cachedMappings.GetValueOrDefault(weaponName);
        }
    }
}