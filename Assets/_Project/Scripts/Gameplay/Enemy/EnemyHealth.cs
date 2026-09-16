using System;
using _Project.Scripts.Core.Interfaces;
using Fusion;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Enemy
{
    public class EnemyHealth : NetworkBehaviour, IDamageable
    {
        [SerializeField] private float maxHealth = 100f;

        [Networked]
        [OnChangedRender(nameof(OnHealthNetworkChanged))]
        private float CurrentHealth { get; set; }
        
        private EnemyBrain _brain;
        private UnityEngine.AI.NavMeshAgent _agent; // необязательно, но можно остановить агента

        public event Action<EnemyHealth> OnDeath;
        public event Action<float, float> OnHealthChanged;
    
        private void Awake()
        {
            _brain = GetComponent<EnemyBrain>();
            _agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        }
        
        public override void Spawned()
        {
            if (HasStateAuthority)
            {
                CurrentHealth = maxHealth;
            }
        }

        public void TakeDamage(float damage)
        {
            if (!HasStateAuthority) return;
            if (CurrentHealth <= 0f) return; 

            CurrentHealth -= damage;
            Debug.Log($"[Server] Enemy took {damage} damage, remaining: {CurrentHealth}");

            if (CurrentHealth <= 0f)
            {
                Die();
            }
        }

        private void Die()
        {
            Debug.Log("[Server] Enemy died, despawning...");
            OnDeath?.Invoke(this);

            if (_brain != null)
                _brain.OnDeath();

            if (_agent != null && _agent.isOnNavMesh)
                _agent.isStopped = true;

            var collider = GetComponent<Collider>();
            if (collider != null)
                collider.enabled = false;

            Runner.Despawn(Object);
        }
        
        private void OnHealthNetworkChanged()
        {
            OnHealthChanged?.Invoke(CurrentHealth, maxHealth);
        }
    }
}