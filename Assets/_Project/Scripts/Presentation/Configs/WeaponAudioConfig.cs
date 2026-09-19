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
            public AudioClip reloadSound;
        }

        [SerializeField] private List<AudioMapping> mappings;
        
        private Dictionary<string, AudioClip> _cachedShotSounds;
        private Dictionary<string, AudioClip> _cachedReloadSounds;
        
        private void EnsureCacheInitialized()
        {
            if (_cachedShotSounds != null) return;

            _cachedShotSounds = new Dictionary<string, AudioClip>();
            _cachedReloadSounds = new Dictionary<string, AudioClip>();

            foreach (var map in mappings)
            {
                if (string.IsNullOrEmpty(map.weaponName)) continue;

                if (map.shotSound != null)
                    _cachedShotSounds[map.weaponName] = map.shotSound;

                if (map.reloadSound != null)
                    _cachedReloadSounds[map.weaponName] = map.reloadSound;
            }
        }
        
        public AudioClip GetShotSound(string weaponName)
        {
            if (string.IsNullOrEmpty(weaponName)) return null;
            
            EnsureCacheInitialized();
            return _cachedShotSounds.GetValueOrDefault(weaponName);
        }
        
        public AudioClip GetReloadSound(string weaponName)
        {
            if (string.IsNullOrEmpty(weaponName)) return null;
            
            EnsureCacheInitialized();
            return _cachedReloadSounds.GetValueOrDefault(weaponName);
        }
    }
}