using _Project.Scripts.Core.Interfaces;
using UnityEngine;

namespace _Project.Scripts.Core.Signals
{
    public readonly struct DamageAppliedSignal
    {
        public float Amount { get; }
        public IDamageable Target { get; }
        public Vector3 HitPoint { get; }
        public Vector3 HitNormal { get; }
        public GameObject Source { get; }
        
        
        public DamageAppliedSignal(float amount, IDamageable target, Vector3 hitPoint, Vector3 hitNormal, GameObject source)
        {
            Amount = amount;
            Target = target;
            HitPoint = hitPoint;
            HitNormal = hitNormal;
            Source = source;
        }
    }
}