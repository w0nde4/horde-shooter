using UnityEngine;

namespace _Project.Scripts.Domain.Gameplay.Configs
{
    [CreateAssetMenu(fileName = "EnemyConfig", menuName = "Enemy/Enemy Config")]
    public class EnemyConfig : ScriptableObject
    {
        [Header("Detection")]
        public LayerMask playerLayer = ~0;
        public float detectionRadius = 100f;
        public float searchIntervalSeconds = 0.2f;
        [Range(0.5f, 1f)] public float targetSwitchThreshold = 0.2f;

        [Header("Health")]
        public float maxHealth = 100f;
        public float despawnDelay = 3f;

        [Header("Combat")]
        public float damage = 10f;
        public float attackRange = 2f;
        public float attackCooldown = 1f;
    }
}