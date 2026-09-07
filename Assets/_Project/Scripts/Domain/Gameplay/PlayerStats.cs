using System;
using _Project.Scripts.Domain.Gameplay.Configs;
using Zenject;

namespace _Project.Scripts.Domain.Gameplay
{
    public class PlayerStats
    {
        private readonly PlayerStatsConfig _config;
    
        public float MaxHealth { get; private set; }
        public float MoveSpeed { get; private set; }
        public float DamageMultiplier { get; private set; }
        public float GrenadeCooldownMultiplier { get; private set; }

        public event Action OnStatsChanged;

        [Inject]
        public PlayerStats(PlayerStatsConfig config)
        {
            _config = config;
            ResetToBase(config);
        }

        public void ResetToBase(PlayerStatsConfig config)
        {
            MaxHealth = config.baseMaxHealth;
            MoveSpeed = config.baseMoveSpeed;
            DamageMultiplier = config.baseDamageMultiplier;
            GrenadeCooldownMultiplier = config.baseGrenadeCooldownMultiplier;
            OnStatsChanged?.Invoke();
        }

        public void ApplyUpgrade(UpgradeType type)
        {
            switch (type)
            {
                case UpgradeType.Damage:
                    DamageMultiplier += 0.25f;
                    break;
                case UpgradeType.MoveSpeed:
                    MoveSpeed += 2f;
                    break;
                case UpgradeType.MaxHealth:
                    MaxHealth += 25f;
                    break;
                case UpgradeType.GrenadeCooldown:
                    GrenadeCooldownMultiplier *= 0.8f;
                    break;
            }
            OnStatsChanged?.Invoke();
        }
    }
}