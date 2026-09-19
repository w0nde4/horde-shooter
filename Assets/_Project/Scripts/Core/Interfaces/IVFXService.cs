using UnityEngine;

namespace _Project.Scripts.Core.Interfaces
{
    public interface IVFXService
    {
        GameObject Spawn(GameObject prefab, Vector3 position, Quaternion rotation);
        void Release(GameObject instance);
    }
}