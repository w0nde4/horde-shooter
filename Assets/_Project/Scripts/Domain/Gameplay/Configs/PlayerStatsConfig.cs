using UnityEngine;

namespace _Project.Scripts.Domain.Gameplay.Configs
{
    [CreateAssetMenu(fileName = "PlayerStatsConfig", menuName = "Player/Player Stats Config")]
    public class PlayerStatsConfig : ScriptableObject
    {
        [Header("Base Stats")]
        public float baseMaxHealth = 100f;
        public float baseMoveSpeed = 5f;
        public float baseDamageMultiplier = 1f;
        public float baseGrenadeCooldownMultiplier = 1f;
    }
}