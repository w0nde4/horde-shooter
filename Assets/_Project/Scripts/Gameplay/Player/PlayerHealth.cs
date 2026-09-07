using System;
using _Project.Scripts.Core.Interfaces;
using _Project.Scripts.Domain.Gameplay;
using UnityEngine;
using Zenject;

namespace _Project.Scripts.Gameplay.Player
{
    public class PlayerHealth : MonoBehaviour, IDamageable, IInitializable
    {
        [Inject] private PlayerStats _playerStats;
    
        private float _currentHealth;
        
        public event Action<float, float> OnHealthChanged; //current,max
        public event Action OnDeath;

        public void Initialize()
        {
            _currentHealth = _playerStats.MaxHealth;
            _playerStats.OnStatsChanged += OnStatsChanged;
        }

        private void OnStatsChanged()
        {
            RefreshMaxHealth();
        }

        public void TakeDamage(float damage)
        {
            _currentHealth -= damage;
            Debug.Log($"Player took {damage} damage, remaining: {_currentHealth}");
            OnHealthChanged?.Invoke(_currentHealth, _playerStats.MaxHealth);

            if (_currentHealth <= 0f)
            {
                Debug.Log("Player died!");
                OnDeath?.Invoke();
                // Здесь можно добавить смерть игрока (респаун и т.п.)
            }
        }

        private void RefreshMaxHealth()
        {
            if (_playerStats == null) return;
            if(_currentHealth < _playerStats.MaxHealth) _currentHealth = _playerStats.MaxHealth;
            OnHealthChanged?.Invoke(_currentHealth, _playerStats.MaxHealth);
            Debug.Log($"Health: {_currentHealth}"); 
        }
    }
}