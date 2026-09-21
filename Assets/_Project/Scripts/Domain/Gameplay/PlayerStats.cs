using System;
using _Project.Scripts.Domain.Gameplay.Configs;
using Zenject;

namespace _Project.Scripts.Domain.Gameplay
{
    public class PlayerStats
    {
        private readonly PlayerStatsConfig _config;
        public Func<UpgradeType, float, float> StatResolver;

        public float MaxHealth => StatResolver?
            .Invoke(UpgradeType.MaxHealth, _config.baseMaxHealth) ?? 
                                  _config.baseMaxHealth;

        public float MoveSpeed => StatResolver?
            .Invoke(UpgradeType.MoveSpeed, _config.baseMoveSpeed) 
                                  ?? _config.baseMoveSpeed;

        public float DamageMultiplier => StatResolver?
            .Invoke(UpgradeType.Damage, _config.baseDamageMultiplier) 
                                         ?? _config.baseDamageMultiplier;

        public float GrenadeCooldownMultiplier => StatResolver?
            .Invoke(UpgradeType.GrenadeCooldown, _config.baseGrenadeCooldownMultiplier) ?? 
                                                  _config.baseGrenadeCooldownMultiplier;

        public event Action<UpgradeType, float> OnUpgradeRequested;
        public event Action OnStatsChanged;

        [Inject]
        public PlayerStats(PlayerStatsConfig config)
        {
            _config = config;
        }

        public void ApplyUpgrade(UpgradeType type)
        {
            var newValue = type switch
            {
                UpgradeType.Damage => DamageMultiplier + 0.25f,
                UpgradeType.MoveSpeed => MoveSpeed + 2f,
                UpgradeType.MaxHealth => MaxHealth + 25f,
                UpgradeType.GrenadeCooldown => GrenadeCooldownMultiplier * 0.8f,
                _ => 0f
            };
            
            OnUpgradeRequested?.Invoke(type, newValue);
            OnStatsChanged?.Invoke();
        }
        
        public void InitializeBaseStats(Action<UpgradeType, float> initializers)
        {
            initializers?.Invoke(UpgradeType.MaxHealth, _config.baseMaxHealth);
            initializers?.Invoke(UpgradeType.MoveSpeed, _config.baseMoveSpeed);
            initializers?.Invoke(UpgradeType.Damage, _config.baseDamageMultiplier);
            initializers?.Invoke(UpgradeType.GrenadeCooldown, _config.baseGrenadeCooldownMultiplier);
        }
        
        public void RefreshLocalUI()
        {
            OnStatsChanged?.Invoke();
        }
    }
}