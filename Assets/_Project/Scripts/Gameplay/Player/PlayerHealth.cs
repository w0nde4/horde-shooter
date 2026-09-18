using System;
using _Project.Scripts.Core.Interfaces;
using _Project.Scripts.Core.Signals;
using _Project.Scripts.Domain.Gameplay;
using Fusion;
using UnityEngine;
using Zenject;

namespace _Project.Scripts.Gameplay.Player
{
    public class PlayerHealth : NetworkBehaviour, IDamageable, IInitializable
    {
        [Inject] private PlayerStats _playerStats;
        [Inject] private SignalBus _signalBus;
    
        [Networked]
        [OnChangedRender(nameof(OnHealthNetworkChanged))]
        private float CurrentHealth { get; set; }

        [Networked]
        [OnChangedRender(nameof(OnDeathStateChanged))]
        private NetworkBool IsDead { get; set; }

        private float _lastRenderHealth;
        
        public event Action<float, float> OnHealthChanged; //current,max
        public event Action OnDeath;

        public void Initialize()
        {
            _playerStats.OnStatsChanged += OnStatsChanged;
        }

        public override void Spawned()
        {
            _lastRenderHealth = CurrentHealth;
            
            if (HasStateAuthority)
            {
                CurrentHealth = _playerStats.MaxHealth;
                IsDead = false;
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
            if (!HasStateAuthority) return;
            if (IsDead) return;

            CurrentHealth -= damage;
            Debug.Log($"[Server] Player took {damage} damage, remaining: {CurrentHealth}");

            if (CurrentHealth <= 0f)
            {
                CurrentHealth = 0f;
                IsDead = true;
                Debug.Log("[Server] Player died!");
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

            if (CurrentHealth < _lastRenderHealth)
            {
                var damageAmount = _lastRenderHealth - CurrentHealth;
                _signalBus.Fire(new DamageAppliedSignal(
                    damageAmount,
                    this,
                    Vector3.zero, //todo: remove stub
                    Vector3.zero,
                    gameObject
                    )
                );
            }

            _lastRenderHealth = CurrentHealth;
        }

        private void OnDeathStateChanged()
        {
            if (IsDead)
            {
                OnDeath?.Invoke();
                Debug.Log("[Client/Host] Executing local death for player.");
            }
        }

        public override void Despawned(NetworkRunner runner, bool hasState)
        {
            base.Despawned(runner, hasState);
            if (_playerStats != null)
                _playerStats.OnStatsChanged -= OnStatsChanged;
        }
    }
}