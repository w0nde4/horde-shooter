using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace _Project.Scripts.Core.Interfaces
{
    public interface ISpawnService
    {
        // Sync
        GameObject Spawn(GameObject prefab, Vector3 position, Quaternion rotation, Transform parent = null,
            bool injectDependencies = true);
        
        // Async
        UniTask<GameObject> SpawnAsync(string address, Vector3 position, Quaternion rotation, Transform parent = null,
            bool injectDependencies = true, Action<GameObject> onBeforeSpawn = null);
    }
}