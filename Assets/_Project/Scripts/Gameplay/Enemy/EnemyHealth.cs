using System;
using _Project.Scripts.Core.Interfaces;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Enemy
{
    public class EnemyHealth : MonoBehaviour, IDamageable
    {
        [SerializeField] private float maxHealth = 100f;

        private float _currentHealth;
        private EnemyBrain _brain;
        private UnityEngine.AI.NavMeshAgent _agent; // необязательно, но можно остановить агента

        public event Action<EnemyHealth> OnDeath;
        public event Action<float, float> OnHealthChanged;
    
        private void Awake()
        {
            _brain = GetComponent<EnemyBrain>();
            _agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        }

        private void Start()
        {
            _currentHealth = maxHealth;
        }

        public void TakeDamage(float damage)
        {
            if (_currentHealth <= 0f) return; 

            _currentHealth -= damage;
            OnHealthChanged?.Invoke(_currentHealth, maxHealth);
            Debug.Log($"Enemy took {damage} damage, remaining: {_currentHealth}");

            if (_currentHealth <= 0f)
            {
                Die();
            }
        }

        private void Die()
        {
            Debug.Log("Enemy died!");
            OnDeath?.Invoke(this);

            if (_brain != null)
                _brain.OnDeath();

            if (_agent != null && _agent.isOnNavMesh)
                _agent.isStopped = true;

            var collider = GetComponent<Collider>();
            if (collider != null)
                collider.enabled = false;

            Destroy(gameObject, 0.1f);
        }
    }
}