using System;
using _Project.Scripts.Core.Interfaces;
using _Project.Scripts.Domain.Gameplay;
using Fusion;
using UnityEngine;
using Zenject;

namespace _Project.Scripts.Gameplay.Player
{
    public class PlayerHealth : NetworkBehaviour, IDamageable, IInitializable
    {
        [Inject] private PlayerStats _playerStats;
    
        [Networked]
        [OnChangedRender(nameof(OnHealthNetworkChanged))]
        private float CurrentHealth { get; set; }
        
        public event Action<float, float> OnHealthChanged; //current,max
        public event Action OnDeath;

        public void Initialize()
        {
            _playerStats.OnStatsChanged += OnStatsChanged;
        }

        public override void Spawned()
        {
            if (Object.HasStateAuthority)
            {
                CurrentHealth = _playerStats.MaxHealth;
            }
        }
        
        private void OnStatsChanged()
        {
            if (HasStateAuthority)
            {
                RefreshMaxHealth();
            }
        }

        public void TakeDamage(float damage)
        {
            if (!Object.HasStateAuthority) return;

            CurrentHealth -= damage;
            Debug.Log($"[Server] Player took {damage} damage, remaining: {CurrentHealth}");

            if (CurrentHealth <= 0f)
            {
                Debug.Log("[Server] Player died!");
                OnDeath?.Invoke();
            }
        }

        private void RefreshMaxHealth()
        {
            if (_playerStats == null) return;
            if(CurrentHealth < _playerStats.MaxHealth) CurrentHealth = _playerStats.MaxHealth;
            Debug.Log($"Health: {CurrentHealth}"); 
        }
        
        private void OnHealthNetworkChanged()
        {
            OnHealthChanged?.Invoke(CurrentHealth, _playerStats.MaxHealth);
        }
    }
}