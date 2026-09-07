using UnityEngine;

namespace _Project.Scripts.Domain.Gameplay.Data
{
    public class EnemySpawnContext
    {
        public string PrefabAddress;
        public Vector3 Center;
        public Transform[] PlayersTransforms;
        public float MinSpawnRadius;
        public float MaxSpawnRadius;
        public float Speed;
        public LayerMask PlayerLayer;
    }
}