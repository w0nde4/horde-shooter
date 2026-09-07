using UnityEngine;

namespace _Project.Scripts.Domain.Gameplay.Configs
{
    [CreateAssetMenu(fileName = "WaveSpawnerConfig", menuName = "Waves/Wave Spawner Config")]
    public class WaveSpawnerConfig : ScriptableObject
    {
        [Header("Spawn Area")]
        public float minSpawnRadius = 10f;
        public float maxSpawnRadius = 30f;

        [Header("Enemy Limit")]
        public int maxAliveEnemies = 20;

        [Header("Enemy Prefab")]
        public string enemyPrefabAddress = "EnemyPrefab";

        [Header("Player Layer")]
        public LayerMask playerLayer;
    }
}