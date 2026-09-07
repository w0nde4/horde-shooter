using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace _Project.Scripts.Services
{
    public class AddressableService
    {
        private readonly Dictionary<string, AsyncOperationHandle<GameObject>> _loadedPrefabs = new();
        public async UniTask<T> LoadAssetAsync<T>(string key)
        {
            var handle = Addressables.LoadAssetAsync<T>(key);
            await handle.ToUniTask();

            if (handle.Status != AsyncOperationStatus.Succeeded)
            {
                Debug.LogError($"AddressableService: failed to load asset '{key}'.");
                Addressables.Release(handle);
                return default;
            }
            return handle.Result;
        }

        public async UniTask<GameObject> LoadPrefabAsync(string key)
        {
            if (_loadedPrefabs.TryGetValue(key, out var cached))
            {
                if(cached.IsValid()) return cached.Result;
                _loadedPrefabs.Remove(key);
            }

            var handle = Addressables.LoadAssetAsync<GameObject>(key);
            await handle.ToUniTask();

            if (handle.Status != AsyncOperationStatus.Succeeded)
            {
                Debug.LogError($"AddressableService: failed to load prefab '{key}'.");
                Addressables.Release(handle);
                return null;
            }
            
            _loadedPrefabs[key] = handle;
            return handle.Result;
        }

        public void ReleasePrefab(string key)
        {
            if(!_loadedPrefabs.TryGetValue(key, out var handle)) return;
            
            Addressables.Release(handle);
            _loadedPrefabs.Remove(key);
        }

        public void ReleaseAll()
        {
            foreach (var handle in _loadedPrefabs.Values)
                Addressables.Release(handle);
            
            _loadedPrefabs.Clear();
        }
    }
}