using System;
using _Project.Scripts.Core.Interfaces;
using Cysharp.Threading.Tasks;
using Fusion;
using UnityEngine;
using Zenject;

namespace _Project.Scripts.Services.Network
{
    public class FusionSpawnService : ISpawnService
    {
        private readonly AddressableService _addressableService;
        private readonly FusionNetworkService _networkService;

        [Inject]
        public FusionSpawnService(AddressableService addressableService, FusionNetworkService networkService)
        {
            _addressableService = addressableService;
            _networkService = networkService;
        }
        
        public GameObject Spawn(GameObject prefab, Vector3 position, Quaternion rotation, Transform parent = null,
            bool injectDependencies = true)
        {
            if (prefab == null)
            {
                Debug.LogError("FusionSpawnService: prefab is null!");
                return null;
            }
            
            return InstantiateNetworkObject(prefab, position, rotation, parent);
        }

        public async UniTask<GameObject> SpawnAsync(string address, Vector3 position, Quaternion rotation, Transform parent = null,
            bool injectDependencies = true, Action<GameObject> onBeforeSpawn = null)
        {
            var prefab = await _addressableService.LoadPrefabAsync(address);
            if (prefab == null)
            {
                Debug.LogError($"FusionSpawnService: Could not load prefab by address: {address}");
                return null;
            }
            return InstantiateNetworkObject(prefab, position, rotation, parent, onBeforeSpawn);
        }

        private GameObject InstantiateNetworkObject(GameObject prefab, Vector3 position, Quaternion rotation,
            Transform parent, Action<GameObject> onBeforeSpawn = null)
        {
            var runner = _networkService.Runner;
            if (runner == null || !runner.IsRunning)
            {
                Debug.LogWarning($"FusionSpawnService: Spawning attempt for {prefab.name} is blocked. Runner is not ready.");
                return null;
            }

            if (!runner.IsServer)
            {
                Debug.LogWarning($"FusionSpawnService: Spawning attempt for {prefab.name} is blocked. Local instance is a pure Client.");
                return null;
            }
            
            var networkObject = prefab.GetComponent<NetworkObject>();
            if (networkObject == null)
            {
                Debug.LogWarning($"FusionSpawnService: {prefab.name} does not have NetworkObject component on it.");
                return null;
            }

            var spawnedObj = runner.Spawn(
                networkObject,
                position,
                rotation,
                inputAuthority: null,
                onBeforeSpawned: (r, obj) =>
                {
                    onBeforeSpawn?.Invoke(obj.gameObject);
                }
            );
            
            if (spawnedObj == null) return null;

            if (parent != null)
            {
                spawnedObj.transform.SetParent(parent);
            }
            
            return spawnedObj.gameObject;
        }
    }
}