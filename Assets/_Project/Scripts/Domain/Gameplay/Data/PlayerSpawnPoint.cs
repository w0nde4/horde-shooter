using UnityEngine;

namespace _Project.Scripts.Domain.Gameplay.Data
{
    public class PlayerSpawnPoint : MonoBehaviour
    {
        public Vector3 Position => transform.position;
        public Quaternion Rotation => transform.rotation;
    }
}