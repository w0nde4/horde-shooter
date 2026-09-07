using UnityEngine;

namespace _Project.Scripts.Core.Interfaces
{
    public interface IDamageService
    {
        void ApplyDamage(float amount, IDamageable target, GameObject source = null);
        void ApplyDamage(float amount, IDamageable target, Vector3 hitPoint, Vector3 hitNormal, GameObject source = null);
    }
}