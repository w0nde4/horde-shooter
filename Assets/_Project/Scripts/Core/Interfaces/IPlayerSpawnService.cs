using Cysharp.Threading.Tasks;
using UnityEngine;

namespace _Project.Scripts.Core.Interfaces
{
    public interface IPlayerSpawnService
    {
        UniTask<GameObject> SpawnPlayerAsync(string ownerID, Vector3 position, Quaternion rotation, int? playerRefRaw = null);
        void DespawnPlayer(string ownerID, bool destroyObject = true);
        void DespawnAll(bool destroyObjects = true);
    }
}