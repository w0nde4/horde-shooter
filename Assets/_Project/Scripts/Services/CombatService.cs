using _Project.Scripts.Core.Interfaces;
using _Project.Scripts.Core.Signals;
using UnityEngine;
using Zenject;

namespace _Project.Scripts.Services
{
    public class CombatService : IDamageService
    {
        public void ApplyDamage(float amount, IDamageable target, GameObject source = null)
        {
            ApplyDamage(amount, target, Vector3.zero, Vector3.zero, source);
        }

        public void ApplyDamage(float amount, IDamageable target, Vector3 hitPoint, Vector3 hitNormal,
            GameObject source = null)
        {
            if (target == null) return;
            target.TakeDamage(amount);
        }
    }
}