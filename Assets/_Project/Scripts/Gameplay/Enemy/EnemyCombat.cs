using _Project.Scripts.Core.Interfaces;
using _Project.Scripts.Domain.Gameplay.Configs;
using Fusion;
using UnityEngine;
using Zenject;

namespace _Project.Scripts.Gameplay.Enemy
{
    public class EnemyCombat : NetworkBehaviour
    {
        [Inject] private EnemyConfig _config;
        [Inject] private IDamageService _damageService;

        [Networked] private TickTimer AttackCooldownTimer { get; set; }

        public float AttackRange => _config.attackRange;
        public float AttackCooldown => _config.attackCooldown;

        private bool IsTargetInRange(Transform target)
        {
            if (target == null) return false;
            return Vector3.Distance(transform.position, target.position) <= _config.attackRange;
        }

        public void TryAttack(Transform target)
        {
            if (target == null) return;
            if (!IsTargetInRange(target)) return;
            if (AttackCooldownTimer.IsRunning && !AttackCooldownTimer.Expired(Runner)) return;

            var damageable = target.GetComponent<IDamageable>();
            if (damageable != null)
            {
                var hitPoint = target.position;
                var hitNormal = (target.position - transform.position).normalized;
                
                _damageService.ApplyDamage(_config.damage, damageable, hitPoint, hitNormal, gameObject);

                AttackCooldownTimer = TickTimer.CreateFromSeconds(Runner, _config.attackCooldown);
                Debug.Log($"Enemy attacked {target.name} for {_config.damage} damage");
            }
            else
            {
                Debug.LogWarning($"Target {target.name} does not implement IDamageable");
            }
        }
    }
}