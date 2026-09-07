using _Project.Scripts.Core.Interfaces;
using UnityEngine;
using Zenject;

namespace _Project.Scripts.Gameplay.Enemy
{
    public class EnemyCombat : MonoBehaviour
    {
        [Header("Combat Settings")]
        [SerializeField] private float damage = 10f;
        [SerializeField] private float attackRange = 2f;
        [SerializeField] private float attackCooldown = 1f;
        
        [Inject] private IDamageService _damageService;

        private float _lastAttackTime = -Mathf.Infinity;

        public float AttackRange => attackRange;
        public float AttackCooldown => attackCooldown;

        public bool IsTargetInRange(Transform target)
        {
            if (target == null) return false;
            return Vector3.Distance(transform.position, target.position) <= attackRange;
        }

        public void TryAttack(Transform target)
        {
            if (target == null) return;
            if (!IsTargetInRange(target)) return;
            if (Time.time < _lastAttackTime + attackCooldown) return;

            var damageable = target.GetComponent<IDamageable>();
            if (damageable != null)
            {
                var hitPoint = target.position;
                var hitNormal = (target.position - transform.position).normalized;
                
                _damageService.ApplyDamage(damage, damageable, hitPoint, hitNormal, gameObject);

                _lastAttackTime = Time.time;
                Debug.Log($"Enemy attacked {target.name} for {damage} damage");
            }
            else
            {
                Debug.LogWarning($"Target {target.name} does not implement IDamageable");
            }
        }
    }
}