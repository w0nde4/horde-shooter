using System.Collections.Generic;
using _Project.Scripts.Core.Interfaces;
using UnityEngine;
using UnityEngine.Pool;

namespace _Project.Scripts.Services.Presentation
{
    public class VFXService : IVFXService
    {
        private readonly Dictionary<GameObject, ObjectPool<GameObject>> _pools = new();
        private readonly Transform _poolRoot;

        public VFXService()
        {
            _poolRoot = new GameObject("[VFX_POOL]").transform;
            Object.DontDestroyOnLoad(_poolRoot.gameObject);
        }
        
        public GameObject Spawn(GameObject prefab, Vector3 position, Quaternion rotation)
        {
            if (prefab == null) return null;

            var pool = GetOrCreatePool(prefab);
            var instance = pool.Get();

            instance.transform.SetPositionAndRotation(position, rotation);
            return instance;
        }

        public void Release(GameObject instance)
        {
            if (instance == null) return;

            if (instance.TryGetComponent<PooledObjectMarker>(out var marker) && marker.OriginPool != null)
                marker.OriginPool.Release(instance);
            else
                Object.Destroy(instance);
        }

        private ObjectPool<GameObject> GetOrCreatePool(GameObject prefab)
        {
            if (_pools.TryGetValue(prefab, out var existingPool))
                return existingPool;

            ObjectPool<GameObject> newPool = null;
            newPool = new ObjectPool<GameObject>(
                createFunc: () =>
                {
                    var inst = Object.Instantiate(prefab, _poolRoot);
                    var marker = inst.AddComponent<PooledObjectMarker>();
                    marker.OriginPool = newPool;
                    return inst;
                },
                actionOnGet: (inst) => inst.SetActive(true),
                actionOnRelease: (inst) => inst.SetActive(false),
                actionOnDestroy: (inst) => Object.Destroy(inst),
                collectionCheck: false,
                defaultCapacity: 20,
                maxSize: 100
            );
            
            _pools.Add(prefab, newPool);
            return newPool;
        }
    }

    public class PooledObjectMarker : MonoBehaviour
    {
        public ObjectPool<GameObject> OriginPool { get; set; }
    }
}