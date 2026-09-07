using _Project.Scripts.Core.Interfaces;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;

namespace _Project.Scripts.Services
{
    public class SpawnService : ISpawnService
    {
        private readonly DiContainer _diContainer;
        private readonly AddressableService _addressableService;
        
        [Inject]
        public SpawnService(DiContainer diContainer, AddressableService addressableService)
        {
            _diContainer = diContainer;
            _addressableService = addressableService;
        }
        
        public GameObject Spawn(GameObject prefab, Vector3 position, Quaternion rotation, Transform parent = null,
            bool injectDependencies = true)
        {
            if (prefab == null)
            {
                Debug.LogError("SpawnService: prefab is null.");
                return null;
            }
            
            return Instantiate(prefab, position, rotation, parent, injectDependencies);
        }

        public async UniTask<GameObject> SpawnAsync(string address, Vector3 position, Quaternion rotation, Transform parent = null,
            bool injectDependencies = true)
        {
            var prefab = await _addressableService.LoadPrefabAsync(address);
            if(prefab == null) return null;
            
            return Instantiate(prefab, position, rotation, parent, injectDependencies);
        }

        private GameObject Instantiate(GameObject prefab, Vector3 position, Quaternion rotation, Transform parent,
            bool injectDependencies)
        {
            return injectDependencies
                ? _diContainer.InstantiatePrefab(prefab, position, rotation, parent)
                : Object.Instantiate(prefab, position, rotation, parent);
        }
    }
}