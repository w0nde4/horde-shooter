using _Project.Scripts.Core.Interfaces;
using UnityEngine;

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
            if (amount <= 0f) return;
            
            if(IsPlayer(target) && IsPlayer(source)) return;
            
            target.TakeDamage(amount);
        }

        private static bool IsPlayer(object obj)
        {
            return obj switch
            {
                Component c => c.GetComponentInParent<IPlayerHealth>() != null,
                GameObject go => go.GetComponentInParent<IPlayerHealth>() != null,
                _ => false
            };
        }
    }
}